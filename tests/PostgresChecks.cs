using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using DatabaseTools;
using Npgsql;

internal static partial class Program
{
    private static bool TryRunFakeTool(string[] args, out int exitCode)
    {
        exitCode = 0;
        var mode = Environment.GetEnvironmentVariable("DBTOOLS_CHECK_TOOL");
        var name = Path.GetFileNameWithoutExtension(Environment.ProcessPath);
        if (mode is null || name is not ("pg_dump" or "pg_restore"))
            return false;

        if (name == "pg_restore")
        {
            Console.WriteLine(";     Format: CUSTOM");
            Console.WriteLine("2; 3079 16384 EXTENSION - codex_missing_extension");
            return true;
        }

        var output = args[Array.IndexOf(args, "--file") + 1];
        File.WriteAllText(output, "new backup");
        if (mode == "fail")
        {
            Console.Error.WriteLine("pg_dump: simulated failure");
            exitCode = 1;
        }
        return true;
    }

    private static async Task WithFakeToolAsync(string name, string mode, Func<Task> action)
    {
        // The apphost finds the test assembly beside itself; no external tools or credentials are required.
        var path = Path.Combine(AppContext.BaseDirectory, name + ".exe");
        var previousPath = Environment.GetEnvironmentVariable("PATH");
        var previousMode = Environment.GetEnvironmentVariable("DBTOOLS_CHECK_TOOL");
        File.Copy(Environment.ProcessPath!, path, overwrite: false);
        try
        {
            Environment.SetEnvironmentVariable("PATH", AppContext.BaseDirectory + Path.PathSeparator + previousPath);
            Environment.SetEnvironmentVariable("DBTOOLS_CHECK_TOOL", mode);
            await action();
        }
        finally
        {
            Environment.SetEnvironmentVariable("PATH", previousPath);
            Environment.SetEnvironmentVariable("DBTOOLS_CHECK_TOOL", previousMode);
            File.Delete(path);
        }
    }

    private static async Task CheckAtomicBackupAsync()
    {
        var path = Path.Combine(TestFolder, "atomic.dump");
        await File.WriteAllTextAsync(path, "original backup");
        var connection = new DatabaseConnectionOptions("localhost", false, "test", "");
        var backup = new BackupOptions("test", BackupSource.LocalFile, path, true);
        await WithFakeToolAsync("pg_dump", "fail", async () =>
        {
            try
            {
                await DatabaseService.BackupDatabaseAsync(connection, backup, null, CancellationToken.None);
                throw new Exception("Failed backup should throw.");
            }
            catch (InvalidOperationException ex)
            {
                Assert(ex.Message.Contains("simulated failure"), "Backup failure reaches the caller.");
            }
        });
        Assert(await File.ReadAllTextAsync(path) == "original backup" && Directory.GetFiles(TestFolder, "atomic.dump.*.tmp").Length == 0,
            "Failed overwrite preserves the original backup and removes temporary output.");
        await WithFakeToolAsync("pg_dump", "success", () => DatabaseService.BackupDatabaseAsync(connection, backup, null, CancellationToken.None));
        Assert(await File.ReadAllTextAsync(path) == "new backup", "Successful backup replaces the destination.");
    }

    private static async Task CheckPostgresAsync()
    {
        var initdb = PostgresToolRunner.ResolvePath("initdb");
        var pgctl = PostgresToolRunner.ResolvePath("pg_ctl");
        if (!File.Exists(initdb) || !File.Exists(pgctl))
        {
            Console.WriteLine("SKIP: isolated PostgreSQL integration checks require installed server tools.");
            return;
        }

        using var portProbe = new TcpListener(IPAddress.Loopback, 0);
        portProbe.Start();
        var port = ((IPEndPoint)portProbe.LocalEndpoint).Port;
        portProbe.Stop();
        var data = Path.Combine(TestFolder, "postgres");
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(1));
        await PostgresToolRunner.RunAsync(initdb, ["-D", data, "--auth=trust", "--username=checks", "--encoding=UTF8", "--locale=C"], "", null, null, timeout.Token);
        try
        {
            await RunPgCtlAsync(pgctl, ["-D", data, "-l", Path.Combine(TestFolder, "postgres.log"), "-o", $"-h 127.0.0.1 -p {port}", "-w", "start"], timeout.Token);
            var options = new DatabaseConnectionOptions($"127.0.0.1:{port}", false, "checks", "");
            var connectionString = $"Host=127.0.0.1;Port={port};Username=checks;Database=postgres;Pooling=false";
            await using var source = new NpgsqlConnection(connectionString);
            await source.OpenAsync(timeout.Token);
            await using (var command = new NpgsqlCommand("CREATE TABLE source_data(id integer); INSERT INTO source_data VALUES (1), (2); CREATE EXTENSION hstore;", source))
                await command.ExecuteNonQueryAsync(timeout.Token);

            await DatabaseService.TestConnectionAsync(options, timeout.Token);
            await DatabaseService.CreateDatabaseAsync(options, "restore_checks", timeout.Token);
            var databases = await DatabaseService.GetDatabasesAsync(options, timeout.Token);
            Assert(databases.Any(database => database.Name == "restore_checks")
                   && databases.All(database => database.SizeMb is null),
                "Database listing returns metadata without waiting for sizes.");
            var sizes = await DatabaseService.GetDatabaseSizesAsync(options, timeout.Token);
            Assert(sizes["restore_checks"] > 0 && sizes["postgres"] > 0,
                "Separate size query returns sizes keyed by database name.");
            using (var cancelledSizes = new CancellationTokenSource())
            {
                cancelledSizes.Cancel();
                try
                {
                    await DatabaseService.GetDatabaseSizesAsync(options, cancelledSizes.Token);
                    throw new Exception("Cancelled size loading should throw.");
                }
                catch (OperationCanceledException)
                {
                    Assert(true, "Background size query supports cancellation.");
                }
            }
            await using var target = new NpgsqlConnection(connectionString.Replace("Database=postgres", "Database=restore_checks"));
            await target.OpenAsync(timeout.Token);
            await using (var command = new NpgsqlCommand("CREATE TABLE preserved(id integer); INSERT INTO preserved VALUES (9)", target))
                await command.ExecuteNonQueryAsync(timeout.Token);

            var invalidDump = Path.Combine(TestFolder, "invalid.dump");
            await File.WriteAllTextAsync(invalidDump, "This is not a PostgreSQL archive", timeout.Token);
            var restore = new RestoreOptions("restore_checks", BackupSource.LocalFile, invalidDump, true);
            try
            {
                await DatabaseService.RestoreDatabaseAsync(options, restore, null, timeout.Token);
                throw new Exception("Invalid archive should fail.");
            }
            catch (InvalidOperationException ex)
            {
                Assert(ex.Message.Contains("pg_restore") && !ex.Message.Contains("partially restored"), "Invalid archive fails before changing the database.");
            }
            await AssertPreservedAsync(target, timeout.Token);

            await WithFakeToolAsync("pg_restore", "missing", async () =>
            {
                try
                {
                    await DatabaseService.RestoreDatabaseAsync(options, restore, null, timeout.Token);
                    throw new Exception("Unavailable extension should fail.");
                }
                catch (InvalidOperationException ex)
                {
                    Assert(ex.Message.Contains("codex_missing_extension") && ex.Message.Contains("has not been changed"), "Unavailable extension is reported before cleanup.");
                }
            });
            await AssertPreservedAsync(target, timeout.Token);

            var archivePath = Path.Combine(TestFolder, "roundtrip.dump");
            await DatabaseService.BackupDatabaseAsync(options, new BackupOptions("postgres", BackupSource.LocalFile, archivePath, false), null, timeout.Token);
            var inspect = typeof(DatabaseService).GetMethod("ReadArchiveAsync", BindingFlags.Static | BindingFlags.NonPublic)!;
            var archive = await (Task<(string[] Extensions, int Jobs)>)inspect.Invoke(null, [archivePath, null, timeout.Token])!;
            Assert(archive.Extensions.Contains("hstore") && archive.Jobs == Math.Clamp(Environment.ProcessorCount, 1, 4), "Real archive extensions and custom-format parallelism are detected.");

            await DatabaseService.RestoreDatabaseAsync(options, restore with { BackupPath = archivePath }, null, timeout.Token);
            await using (var command = new NpgsqlCommand("SELECT count(*) FROM source_data", target))
                Assert((long)(await command.ExecuteScalarAsync(timeout.Token))! == 2, "Real backup and restore round trip preserves data.");
            await using (var command = new NpgsqlCommand("SELECT count(*) FROM pg_namespace n, aclexplode(coalesce(n.nspacl, acldefault('n', n.nspowner))) a WHERE n.nspname = 'public' AND a.grantee = 0 AND a.privilege_type = 'CREATE'", target))
                Assert((long)(await command.ExecuteScalarAsync(timeout.Token))! == 0, "Schema cleanup does not grant CREATE to PUBLIC.");
        }
        finally
        {
            if (File.Exists(Path.Combine(data, "postmaster.pid")))
                await RunPgCtlAsync(pgctl, ["-D", data, "-m", "immediate", "-w", "stop"], CancellationToken.None);
        }
    }

    private static async Task RunPgCtlAsync(string path, string[] args, CancellationToken token)
    {
        // A daemon keeps inherited pipe handles open, so pg_ctl must not use the finite-tool output pump.
        var start = new ProcessStartInfo(path) { UseShellExecute = false, CreateNoWindow = true };
        foreach (var arg in args)
            start.ArgumentList.Add(arg);
        using var process = Process.Start(start)!;
        await process.WaitForExitAsync(token);
        Assert(process.ExitCode == 0, "Temporary PostgreSQL lifecycle command completed.");
    }

    private static async Task AssertPreservedAsync(NpgsqlConnection connection, CancellationToken token)
    {
        await using var command = new NpgsqlCommand("SELECT id FROM preserved", connection);
        Assert((int)(await command.ExecuteScalarAsync(token))! == 9, "Preflight failure leaves existing tables and data untouched.");
    }
}
