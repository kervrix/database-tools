using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using DatabaseTools;

internal static partial class Program
{
    private static readonly string TestFolder = Path.Combine(Path.GetTempPath(), "database-tools-checks", Guid.NewGuid().ToString("N"));
    [STAThread]
    private static int Main(string[] args)
    {
        Console.OutputEncoding = new UTF8Encoding(false);
        if (TryRunFakeTool(args, out var exitCode))
            return exitCode;
        if (args is ["--stderr"])
        {
            for (var i = 1; i <= 30; i++)
                Console.Error.WriteLine($"Помилка рядок {i}");
            return 1;
        }
        if (args is ["--wait", var marker])
        {
            File.WriteAllText(marker, Environment.ProcessId.ToString());
            Thread.Sleep(TimeSpan.FromSeconds(30));
            return 0;
        }

        if (args is ["--success"])
            return 0;

        Directory.CreateDirectory(TestFolder);
        if (args is ["--postgres-checks"])
        {
            CheckPostgresAsync().GetAwaiter().GetResult();
            return 0;
        }
        if (args is ["--render-forms"])
        {
            RenderForms();
            return 0;
        }
        if (args is ["--ui-checks"])
        {
            CheckOperationUi();
            return 0;
        }
        CheckToolErrorsAsync().GetAwaiter().GetResult();
        CheckCancellationAsync().GetAwaiter().GetResult();
        CheckBackupsAsync().GetAwaiter().GetResult();
        CheckSettingsAsync().GetAwaiter().GetResult();
        CheckLog();
        CheckAtomicBackupAsync().GetAwaiter().GetResult();
        CheckPostgresAsync().GetAwaiter().GetResult();
        CheckS3Profile();
        CheckOperationUi();
        Console.WriteLine("All behavioral checks passed.");
        return 0;
    }

    private static Task InvokeToolAsync(string path, string[] args, Action<string>? log = null, CancellationToken token = default)
        => PostgresToolRunner.RunAsync(path, args, string.Empty, null, log, token);

    private static async Task CheckToolErrorsAsync()
    {
        var log = new List<string>();
        var restoreTool = PostgresToolRunner.ResolvePath("pg_restore");
        if (File.Exists(restoreTool))
        {
            try
            {
                await InvokeToolAsync(restoreTool, ["--list", Path.Combine(TestFolder, "missing.dump")], log.Add);
                throw new Exception("Missing dump should fail.");
            }
            catch (InvalidOperationException ex)
            {
                Assert(ex.Message.Contains("missing.dump") && ex.Message.Contains("exit code 1"), "Actual pg_restore stderr is included in the exception.");
            }
        }

        var shell = Environment.ProcessPath!;
        try
        {
            await InvokeToolAsync(shell, ["--stderr"], log.Add);
            throw new Exception("Fake tool should fail.");
        }
        catch (InvalidOperationException ex)
        {
            Assert(ex.Message.Contains("Помилка рядок 30") && !ex.Message.Contains("Помилка рядок 10\r\n"), "UTF-8 stderr stays readable and tail is bounded.");
        }
        await InvokeToolAsync(shell, ["--success"]);
        Console.WriteLine("PASS: successful process produces no false error.");
    }

    private static async Task CheckCancellationAsync()
    {
        var shell = Environment.ProcessPath!;
        var marker = Path.Combine(TestFolder, "cancelled.pid");
        File.Delete(marker);
        using var cancellation = new CancellationTokenSource();
        var task = InvokeToolAsync(shell, ["--wait", marker], token: cancellation.Token);
        for (var i = 0; i < 100 && !File.Exists(marker); i++)
            await Task.Delay(50);
        Assert(File.Exists(marker), "Cancellation test process started.");
        var pid = int.Parse((await File.ReadAllTextAsync(marker)).Trim());
        cancellation.Cancel();
        try
        {
            await task;
            throw new Exception("Cancellation should propagate.");
        }
        catch (OperationCanceledException)
        {
            var stopped = false;
            try { stopped = Process.GetProcessById(pid).HasExited; }
            catch (ArgumentException) { stopped = true; }
            Assert(stopped, "Cancellation stops the child before returning.");
        }
    }

    private static async Task CheckBackupsAsync()
    {
        using var portProbe = new TcpListener(IPAddress.Loopback, 0);
        portProbe.Start();
        var port = ((IPEndPoint)portProbe.LocalEndpoint).Port;
        portProbe.Stop();
        var endpoint = $"http://127.0.0.1:{port}";
        using var listener = new HttpListener();
        listener.Prefixes.Add(endpoint + "/");
        listener.Start();
        string Item(string key, string date) => $"<Contents><Key>{key}</Key><LastModified>{date}</LastModified><Size>10</Size><StorageClass>STANDARD</StorageClass></Contents>";
        var pages = new[]
        {
            "<IsTruncated>true</IsTruncated><NextContinuationToken>next</NextContinuationToken>" + Item("postgres/a/old.dump", "2026-10-01T00:00:00Z") + Item("postgres/b/b.dump", "2026-10-02T00:00:00Z"),
            "<IsTruncated>false</IsTruncated>" + Item("postgres/a/new.dump", "2026-10-09T00:00:00Z") + Item("postgres/a/equal.dump", "2026-10-09T00:00:00Z") + Item("postgres/a/ignore.txt", "2026-10-10T00:00:00Z"),
            "<IsTruncated>false</IsTruncated>",
            "<IsTruncated>false</IsTruncated>"
        };
        var serve = Task.Run(async () =>
        {
            for (var i = 0; i < pages.Length; i++)
            {
                var context = await listener.GetContextAsync();
                if (i == 3)
                    Assert(context.Request.QueryString["prefix"] == "postgres/a/", "Backup listing uses the complete database folder prefix.");
                if (i == 1)
                    Assert(context.Request.QueryString["continuation-token"] == "next", "Continuation token is forwarded.");
                var bytes = Encoding.UTF8.GetBytes("<ListBucketResult xmlns=\"http://s3.amazonaws.com/doc/2006-03-01/\"><Name>bucket</Name>" + pages[i] + "</ListBucketResult>");
                context.Response.ContentType = "application/xml";
                context.Response.ContentLength64 = bytes.Length;
                await context.Response.OutputStream.WriteAsync(bytes);
                context.Response.Close();
            }
        });
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var credentials = new S3CredentialOptions("test", "test");
        var latest = await S3BackupService.GetLatestBackupsAsync(endpoint + "/bucket/postgres", credentials, timeout.Token);
        Assert(latest.Count == 2 && latest[0].DatabaseName == "a" && latest[0].LatestBackup.Url.EndsWith("new.dump"), "Latest backup across pages, equal dates, filter and sorting preserved.");
        Assert((await S3BackupService.GetLatestBackupsAsync(endpoint + "/bucket/postgres", credentials, timeout.Token)).Count == 0, "Empty latest-backup listing works.");
        Assert((await S3BackupService.GetBackupsAsync("a", endpoint + "/bucket/postgres", credentials, timeout.Token)).Count == 0, "Empty per-database listing works.");
        await serve;
    }

    private static async Task CheckSettingsAsync()
    {
        var service = new SettingsService();
        var path = Path.Combine(TestFolder, "settings.json");
        typeof(SettingsService).GetField("_settingsPath", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(service, path);
        await service.SaveAsync(new AppSettings { LastSqlConnectionName = "original" }, CancellationToken.None);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        try { await service.SaveAsync(new AppSettings { LastSqlConnectionName = "changed" }, cancellation.Token); }
        catch (OperationCanceledException) { }
        Assert((await service.LoadAsync(CancellationToken.None)).LastSqlConnectionName == "original" && Directory.GetFiles(TestFolder, "settings.json.*.tmp").Length == 0, "Cancelled save retains original settings and removes temporary file.");
    }

    private static void CheckLog()
    {
        using var log = new OperationLog(TestFolder);
        for (var i = 0; i < 2500; i++)
            log.Write($"{i}: " + new string('x', 100));
        var display = log.ReadBatch() + log.ReadBatch();
        log.Flush();
        Assert(log.ReadBatch().Length == 0 && display.Contains("2499:") && !display.Contains("] 0:"), "Pending UI log is bounded and keeps the latest messages.");
        using var reader = new StreamReader(new FileStream(log.FilePath!, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
        var file = reader.ReadToEnd();
        Assert(file.Contains("] 0:") && file.Contains("] 2499:"), "The log file retains messages omitted from the UI.");
    }

    private static void CheckS3Profile()
    {
        const string url = "https://s3.example.test/bucket/postgres/team";
        var parse = typeof(S3CredentialsForm).GetMethod("ParseS3ProfileUrl", BindingFlags.Static | BindingFlags.NonPublic)!;
        var parts = parse.Invoke(null, [url])!;
        var endpoint = (string)parts.GetType().GetProperty("EndpointUrl")!.GetValue(parts)!;
        var bucket = (string)parts.GetType().GetProperty("Bucket")!.GetValue(parts)!;
        using var form = new S3ProfileEditForm("test", endpoint, bucket, "", "", false);
        Assert(form.BuildS3Url() == url, "Editing an S3 profile preserves the entire prefix.");
    }

    private static void Assert(bool condition, string description)
    {
        if (!condition) throw new Exception(description);
        Console.WriteLine("PASS: " + description);
    }
}