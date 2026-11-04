using System.Globalization;
using System.Text.RegularExpressions;
using Npgsql;

namespace DatabaseTools;

public static class DatabaseService
{
    public static async Task CreateDatabaseAsync(
        DatabaseConnectionOptions options,
        string databaseName,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException("Database name is required.");
        }

        await using var connection = new NpgsqlConnection(BuildConnectionString(options, "postgres"));
        await connection.OpenAsync(cancellationToken);

        await ExecuteNonQueryAsync(connection, $"CREATE DATABASE {QuoteIdentifier(databaseName)}", cancellationToken);
    }

    public static async Task DropDatabaseAsync(
        DatabaseConnectionOptions options,
        string databaseName,
        Action<string>? log,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException("Database name is required.");
        }

        await using var connection = new NpgsqlConnection(BuildConnectionString(options, "postgres"));
        await connection.OpenAsync(cancellationToken);

        // Without this the drop fails outright if anything still holds a session open.
        log?.Invoke($"Closing existing connections to {databaseName}...");
        await using (var terminate = connection.CreateCommand())
        {
            terminate.CommandText = """
                                    SELECT pg_terminate_backend(pid)
                                    FROM pg_stat_activity
                                    WHERE datname = @database AND pid <> pg_backend_pid();
                                    """;
            terminate.Parameters.AddWithValue("database", databaseName);
            terminate.CommandTimeout = 0;
            await terminate.ExecuteNonQueryAsync(cancellationToken);
        }

        log?.Invoke($"Dropping database {databaseName}...");
        await ExecuteNonQueryAsync(connection, $"DROP DATABASE {QuoteIdentifier(databaseName)}", cancellationToken);
    }

    public static async Task<IReadOnlyList<DatabaseInfo>> GetDatabasesAsync(
        DatabaseConnectionOptions options,
        CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(BuildConnectionString(options, "postgres"));
        await connection.OpenAsync(cancellationToken);

        const string sql = """
                           SELECT
                               datname,
                               CASE WHEN datallowconn THEN 'ONLINE' ELSE 'NO CONNECTIONS' END AS state,
                               pg_encoding_to_char(encoding) AS encoding,
                               datcollate
                           FROM pg_database
                           WHERE NOT datistemplate
                           ORDER BY datname;
                           """;

        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var databases = new List<DatabaseInfo>();
        while (await reader.ReadAsync(cancellationToken))
        {
            databases.Add(new DatabaseInfo(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                null));
        }

        return databases;
    }

    public static async Task<IReadOnlyDictionary<string, decimal>> GetDatabaseSizesAsync(
        DatabaseConnectionOptions options,
        CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(BuildConnectionString(options, "postgres"));
        await connection.OpenAsync(cancellationToken);
        const string sql = """
                           SELECT datname, ROUND(pg_database_size(oid)::numeric / 1024.0 / 1024.0, 2)
                           FROM pg_database
                           WHERE NOT datistemplate
                             AND (has_database_privilege(oid, 'CONNECT')
                                  OR pg_has_role(current_user, 'pg_read_all_stats', 'MEMBER'));
                           """;
        await using var command = new NpgsqlCommand(sql, connection) { CommandTimeout = 30 };
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var sizes = new Dictionary<string, decimal>(StringComparer.Ordinal);
        while (await reader.ReadAsync(cancellationToken))
        {
            if (!reader.IsDBNull(1))
                sizes.Add(reader.GetString(0), reader.GetDecimal(1));
        }
        return sizes;
    }

    public static async Task TestConnectionAsync(DatabaseConnectionOptions options, CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(BuildConnectionString(options, "postgres"));
        await connection.OpenAsync(cancellationToken);
    }

    public static async Task RestoreDatabaseAsync(
        DatabaseConnectionOptions connectionOptions,
        RestoreOptions restoreOptions,
        Action<string>? log,
        CancellationToken cancellationToken,
        IProgress<OperationProgress>? progress = null)
    {
        var restorePath = restoreOptions.BackupPath;
        var tempFile = string.Empty;
        var databaseMayHaveChanged = false;
        try
        {
            if (restoreOptions.Source == BackupSource.S3)
            {
                var credentials = restoreOptions.S3Credentials ?? throw new InvalidOperationException("S3 credentials are required for restore from S3.");
                tempFile = CreateTempDumpPath(restoreOptions.BackupPath);
                await S3BackupService.DownloadAsync(restoreOptions.BackupPath, tempFile, credentials, log, cancellationToken, progress);
                restorePath = tempFile;
            }

            if (!File.Exists(restorePath))
                throw new FileNotFoundException("Backup file was not found.", restorePath);

            progress?.Report(new OperationProgress("Checking backup"));
            log?.Invoke("Checking backup archive and required extensions...");
            var archive = await ReadArchiveAsync(restorePath, log, cancellationToken);
            await CheckExtensionsAsync(connectionOptions, restoreOptions.DatabaseName, archive.Extensions, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            var args = new List<string>
            {
                "--verbose", "--exit-on-error", "--no-owner", "--no-password",
                "--dbname", BuildLibpqConnectionString(connectionOptions, restoreOptions.DatabaseName),
                "--jobs", archive.Jobs.ToString(CultureInfo.InvariantCulture), restorePath
            };

            if (restoreOptions.Replace)
            {
                progress?.Report(new OperationProgress("Cleaning schemas"));
                databaseMayHaveChanged = true;
                await CleanDatabaseSchemasAsync(connectionOptions, restoreOptions.DatabaseName, log, cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(new OperationProgress("Restoring database"));
            log?.Invoke($"Starting pg_restore from {restoreOptions.BackupPath} ({archive.Jobs} parallel job(s))");
            databaseMayHaveChanged = true;
            await PostgresToolRunner.RunAsync("pg_restore", args, connectionOptions.Password, "-c maintenance_work_mem=256MB", log, cancellationToken);
        }
        catch (OperationCanceledException ex) when (databaseMayHaveChanged)
        {
            throw new OperationCanceledException($"Restore cancelled. Database '{restoreOptions.DatabaseName}' may be partially restored. Review it before using it.", ex, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException && databaseMayHaveChanged)
        {
            throw new InvalidOperationException($"Restore failed. Database '{restoreOptions.DatabaseName}' may be partially restored.\r\n\r\n{ex.Message}", ex);
        }
        finally
        {
            DeleteTempFile(tempFile, log);
        }
    }

    private static async Task<(string[] Extensions, int Jobs)> ReadArchiveAsync(
        string path,
        Action<string>? log,
        CancellationToken cancellationToken)
    {
        var extensions = new HashSet<string>(StringComparer.Ordinal);
        var jobs = 1;
        await PostgresToolRunner.RunAsync("pg_restore", ["--list", path], string.Empty, null, log, cancellationToken, line =>
        {
            var match = Regex.Match(line, @"^\d+;\s+\d+\s+\d+\s+EXTENSION - (.+)$");
            if (match.Success)
                extensions.Add(match.Groups[1].Value.Trim());

            if (Regex.IsMatch(line, @"^;\s*Format:\s*CUSTOM$"))
                jobs = Math.Clamp(Environment.ProcessorCount, 1, 4);
        });

        return (extensions.ToArray(), jobs);
    }

    private static async Task CheckExtensionsAsync(
        DatabaseConnectionOptions options,
        string databaseName,
        string[] required,
        CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(BuildConnectionString(options, databaseName));
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand("SELECT name FROM pg_available_extensions WHERE name = ANY(@names)", connection);
        command.Parameters.AddWithValue("names", required);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var available = new HashSet<string>(StringComparer.Ordinal);
        while (await reader.ReadAsync(cancellationToken))
            available.Add(reader.GetString(0));

        var missing = required.Where(name => !available.Contains(name)).ToArray();
        if (missing.Length > 0)
            throw new InvalidOperationException($"Restore requires unavailable PostgreSQL extensions: {string.Join(", ", missing)}. Install them on server '{options.Server}' before restoring. The database has not been changed.");
    }

    public static async Task BackupDatabaseAsync(
        DatabaseConnectionOptions connectionOptions,
        BackupOptions backupOptions,
        Action<string>? log,
        CancellationToken cancellationToken,
        IProgress<OperationProgress>? progress = null)
    {
        var destination = backupOptions.Source == BackupSource.LocalFile ? Path.GetFullPath(backupOptions.BackupPath) : null;
        if (destination is not null)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            if (File.Exists(destination) && !backupOptions.Init)
                throw new InvalidOperationException("Backup file already exists. Enable overwrite or choose another file name.");
        }

        var tempFile = destination is null ? CreateTempDumpPath(backupOptions.BackupPath) : destination + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            var args = new List<string>
            {
                "--format=custom", "--verbose", "--no-owner", "--no-password", "--file", tempFile,
                BuildLibpqConnectionString(connectionOptions, backupOptions.DatabaseName)
            };

            progress?.Report(new OperationProgress("Creating backup"));
            log?.Invoke($"Starting pg_dump to {backupOptions.BackupPath}");
            await PostgresToolRunner.RunAsync("pg_dump", args, connectionOptions.Password, null, log, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            if (destination is not null)
            {
                File.Move(tempFile, destination, overwrite: backupOptions.Init);
            }
            else
            {
                var credentials = backupOptions.S3Credentials ?? throw new InvalidOperationException("S3 credentials are required for backup to S3.");
                await S3BackupService.UploadAsync(tempFile, backupOptions.BackupPath, credentials, backupOptions.Init, log, cancellationToken, progress);
            }
        }
        finally
        {
            DeleteTempFile(tempFile, log);
        }
    }

    public static async Task ShrinkDatabaseAsync(
        DatabaseConnectionOptions options,
        string databaseName,
        Action<string>? log,
        CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(BuildConnectionString(options, databaseName));
        await connection.OpenAsync(cancellationToken);

        log?.Invoke($"Running VACUUM on {databaseName}...");
        await ExecuteNonQueryAsync(connection, "VACUUM", cancellationToken);
    }

    private static async Task ExecuteNonQueryAsync(NpgsqlConnection connection, string sql, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.CommandTimeout = 0;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static string CreateTempDumpPath(string backupPath)
    {
        var fileName = Path.GetFileName(Uri.TryCreate(backupPath, UriKind.Absolute, out var uri) ? uri.LocalPath : backupPath);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            fileName = $"{Guid.NewGuid():N}.dump";
        }

        var folder = Path.Combine(Path.GetTempPath(), "DatabaseTools");
        Directory.CreateDirectory(folder);
        return Path.Combine(folder, $"{Guid.NewGuid():N}-{fileName}");
    }

    private static void DeleteTempFile(string path, Action<string>? log)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return;
        }

        try
        {
            File.Delete(path);
        }
        catch (Exception ex)
        {
            log?.Invoke($"Unable to delete temporary dump file {path}: {ex.Message}");
        }
    }

    private static string BuildConnectionString(DatabaseConnectionOptions options, string database)
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = GetHost(options.Server),
            Port = GetPort(options.Server),
            Database = database,
            Timeout = 60,
            CommandTimeout = 0
        };

        if (!options.UseIntegratedAuth)
        {
            builder.Username = options.UserName;
            builder.Password = options.Password;
        }

        return builder.ConnectionString;
    }

    private static async Task CleanDatabaseSchemasAsync(
        DatabaseConnectionOptions options,
        string databaseName,
        Action<string>? log,
        CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(BuildConnectionString(options, databaseName));
        await connection.OpenAsync(cancellationToken);

        log?.Invoke($"Cleaning schemas in database {databaseName}...");
        await ExecuteNonQueryAsync(
            connection,
            """
            DO $$
            DECLARE
                schema_name text;
            BEGIN
                FOR schema_name IN
                    SELECT nspname
                    FROM pg_namespace
                    WHERE nspname NOT IN ('pg_catalog', 'information_schema')
                      AND nspname NOT LIKE 'pg_toast%'
                      AND nspname NOT LIKE 'pg_temp_%'
                LOOP
                    EXECUTE format('DROP SCHEMA IF EXISTS %I CASCADE', schema_name);
                END LOOP;
            END
            $$;

            CREATE SCHEMA IF NOT EXISTS public;
            """,
            cancellationToken);
    }

    private static string BuildLibpqConnectionString(DatabaseConnectionOptions options, string database)
    {
        var parts = new List<string>
        {
            $"host={QuoteLibpqValue(GetHost(options.Server))}",
            $"port={GetPort(options.Server)}",
            $"dbname={QuoteLibpqValue(database)}"
        };

        if (!options.UseIntegratedAuth)
        {
            parts.Add($"user={QuoteLibpqValue(options.UserName)}");
        }

        return string.Join(" ", parts);
    }

    private static string GetHost(string server)
    {
        if (string.IsNullOrWhiteSpace(server))
        {
            return "localhost";
        }

        var value = server.Trim();
        var lastColon = value.LastIndexOf(':');
        return lastColon > 0 && int.TryParse(value[(lastColon + 1)..], out _)
            ? value[..lastColon]
            : value;
    }

    private static int GetPort(string server)
    {
        var value = server.Trim();
        var lastColon = value.LastIndexOf(':');
        return lastColon > 0 && int.TryParse(value[(lastColon + 1)..], out var port)
            ? port
            : 5432;
    }

    private static string QuoteIdentifier(string value) => "\"" + value.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";

    private static string QuoteLibpqValue(string value) => "'" + value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("'", "\\'", StringComparison.Ordinal) + "'";
}
