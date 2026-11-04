using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DatabaseTools;

public sealed class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly string _settingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Kervrix",
        "DatabaseTools",
        "settings.json");

    public async Task<AppSettings> LoadAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_settingsPath))
        {
            return new AppSettings();
        }

        await using var stream = File.OpenRead(_settingsPath);
        var settings = await JsonSerializer.DeserializeAsync<AppSettings>(stream, cancellationToken: cancellationToken);
        return settings ?? new AppSettings();
    }

    public async Task SaveAsync(AppSettings settings, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);

        var tempPath = _settingsPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            await using (var stream = File.Create(tempPath))
            {
                await JsonSerializer.SerializeAsync(stream, settings, JsonOptions, cancellationToken);
            }

            File.Move(tempPath, _settingsPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    public string ProtectSecret(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var data = Encoding.UTF8.GetBytes(value);
        var encrypted = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(encrypted);
    }

    public string UnprotectSecret(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var encrypted = Convert.FromBase64String(value);
        var data = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(data);
    }
}

public sealed class AppSettings
{
    public List<SqlConnectionProfile> SqlConnections { get; set; } = [];
    public List<SqlConnectionProfile> PostgresConnections
    {
        get => SqlConnections;
        set => SqlConnections = value;
    }

    public List<S3CredentialProfile> S3Credentials { get; set; } = [];
    public string? LastSqlConnectionName { get; set; }
    public string? LastPostgresConnectionName
    {
        get => LastSqlConnectionName;
        set => LastSqlConnectionName = value;
    }

    public string? LastS3CredentialName { get; set; }

    public SqlConnectionProfile? FindSqlConnection(string? name)
    {
        return SqlConnections.FirstOrDefault(profile => HasName(profile.Name, name));
    }

    public S3CredentialProfile? FindS3Credential(string? name)
    {
        return S3Credentials.FirstOrDefault(profile => HasName(profile.Name, name));
    }

    private static bool HasName(string value, string? name)
    {
        return value.Equals(name, StringComparison.OrdinalIgnoreCase);
    }
}

public sealed class SqlConnectionProfile
{
    public string Name { get; set; } = string.Empty;
    public string Server { get; set; } = string.Empty;
    public bool WindowsAuth { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string ProtectedPassword { get; set; } = string.Empty;
}

public sealed class S3CredentialProfile
{
    public string Name { get; set; } = string.Empty;
    public string S3Url { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string ProtectedSecretKey { get; set; } = string.Empty;
}