using System.Diagnostics;
using System.Text;

namespace DatabaseTools;

public static class PostgresToolRunner
{
    public static async Task RunAsync(
        string fileName,
        IReadOnlyList<string> arguments,
        string password,
        string? pgOptions,
        Action<string>? log,
        CancellationToken cancellationToken,
        Action<string>? standardOutput = null)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var startInfo = new ProcessStartInfo
        {
            FileName = ResolvePath(fileName),
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            StandardErrorEncoding = Encoding.UTF8,
            StandardOutputEncoding = Encoding.UTF8,
            CreateNoWindow = true
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        if (!string.IsNullOrWhiteSpace(password))
        {
            startInfo.Environment["PGPASSWORD"] = password;
        }

        if (!string.IsNullOrWhiteSpace(pgOptions))
        {
            startInfo.Environment["PGOPTIONS"] = pgOptions;
        }

        startInfo.Environment["LC_ALL"] = "C";
        startInfo.Environment["PGCLIENTENCODING"] = "UTF8";

        using var process = new Process { StartInfo = startInfo };
        try
        {
            process.Start();
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or FileNotFoundException)
        {
            throw new InvalidOperationException($"{fileName} was not found. Install PostgreSQL client tools or add the PostgreSQL bin folder to PATH.", ex);
        }

        var errorLines = new Queue<string>();
        var outputTask = PumpProcessOutputAsync(process.StandardOutput, standardOutput ?? log, CancellationToken.None);
        var errorTask = PumpProcessOutputAsync(process.StandardError, line =>
        {
            if (errorLines.Count == 20)
                errorLines.Dequeue();

            errorLines.Enqueue(line);
            log?.Invoke(line);
        }, CancellationToken.None);

        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            try
            {
                if (!process.HasExited)
                    process.Kill(entireProcessTree: true);
            }
            catch (InvalidOperationException) when (process.HasExited)
            {
                // The process can exit between the check and Kill.
            }

            await process.WaitForExitAsync(CancellationToken.None);
            await Task.WhenAll(outputTask, errorTask);
            throw;
        }

        await Task.WhenAll(outputTask, errorTask);

        if (process.ExitCode != 0)
        {
            var message = $"{fileName} failed with exit code {process.ExitCode}.";
            if (errorLines.Count > 0)
                message += Environment.NewLine + Environment.NewLine + string.Join(Environment.NewLine, errorLines);

            throw new InvalidOperationException(message);
        }
    }

    public static string ResolvePath(string fileName)
    {
        if (File.Exists(fileName))
        {
            return fileName;
        }

        var executableName = OperatingSystem.IsWindows() && !fileName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
            ? fileName + ".exe"
            : fileName;

        foreach (var folder in GetSearchPathFolders())
        {
            var candidate = Path.Combine(folder, executableName);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        foreach (var candidate in GetWindowsPostgresToolCandidates(executableName))
        {
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return fileName;
    }

    private static IEnumerable<string> GetSearchPathFolders()
    {
        var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        return path
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(Directory.Exists);
    }

    private static IEnumerable<string> GetWindowsPostgresToolCandidates(string executableName)
    {
        if (!OperatingSystem.IsWindows())
        {
            yield break;
        }

        var roots = new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
        };

        foreach (var root in roots.Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var postgresRoot = Path.Combine(root, "PostgreSQL");
            if (!Directory.Exists(postgresRoot))
            {
                continue;
            }

            foreach (var versionFolder in Directory.EnumerateDirectories(postgresRoot).OrderByDescending(path => Version.TryParse(Path.GetFileName(path), out var version) ? version : new Version()))
            {
                yield return Path.Combine(versionFolder, "bin", executableName);
            }
        }
    }

    private static async Task PumpProcessOutputAsync(StreamReader reader, Action<string>? log, CancellationToken cancellationToken)
    {
        while (await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false) is { } line)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                log?.Invoke(line);
            }
        }
    }

}