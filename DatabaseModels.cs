namespace DatabaseTools;

public sealed record DatabaseConnectionOptions(string Server, bool UseIntegratedAuth, string UserName, string Password);

public sealed record DatabaseInfo(
    string Name,
    string State,
    string Encoding,
    string Collation,
    decimal? SizeMb)
{
    public override string ToString() => $"{Name} ({State})";
}

public sealed record S3CredentialOptions(string AccessKey, string SecretKey)
{
    public bool HasExplicitCredentials => !string.IsNullOrWhiteSpace(AccessKey) && !string.IsNullOrWhiteSpace(SecretKey);
}

public sealed record RestoreOptions(
    string DatabaseName,
    BackupSource Source,
    string BackupPath,
    bool Replace,
    S3CredentialOptions? S3Credentials = null);

public sealed record BackupOptions(
    string DatabaseName,
    BackupSource Source,
    string BackupPath,
    bool Init,
    S3CredentialOptions? S3Credentials = null);

public enum BackupSource
{
    S3,
    LocalFile
}

public sealed record BackupHistoryItem(string Url, DateTime? FinishedAt, decimal? Size)
{
    public override string ToString()
    {
        var name = Path.GetFileName(Url);
        var sizeText = Size is null ? string.Empty : $" | {((double)Size.Value / 1024d / 1024d):N1} MB";
        var dateText = FinishedAt is null ? string.Empty : $"{FinishedAt:yyyy-MM-dd HH:mm} | ";
        return $"{dateText}{name}{sizeText}";
    }
}

public sealed record DatabaseBackupSummary(string DatabaseName, BackupHistoryItem LatestBackup)
{
    public override string ToString() => $"{DatabaseName} | {LatestBackup}";
}

public sealed record S3BackupLocation(
    string ServiceUrl,
    string ObjectUrlRoot,
    string BucketName,
    string Prefix,
    string? RegionName,
    bool IsAwsEndpoint)
{
    public static S3BackupLocation FromObjectUrl(string objectUrl)
    {
        return FromUrl(objectUrl, string.Empty);
    }

    public static S3BackupLocation FromUrl(string s3Url, string databaseName)
    {
        if (!Uri.TryCreate(s3Url.TrimEnd('/'), UriKind.Absolute, out var uri))
        {
            throw new InvalidOperationException("S3 URL must be a full URL prefix, for example https://endpoint/bucket/postgres.");
        }

        var pathParts = uri.AbsolutePath
            .Trim('/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (pathParts.Length == 0)
        {
            throw new InvalidOperationException("S3 URL must include bucket name in the path, for example https://storage.example.com/backup/postgres.");
        }

        var bucketName = pathParts[0];
        var serviceHost = uri.Authority;
        var basePrefix = Uri.UnescapeDataString(string.Join("/", pathParts.Skip(1)));
        var prefix = string.Join("/", new[] { basePrefix, databaseName }.Where(value => !string.IsNullOrWhiteSpace(value)));
        var serviceScheme = uri.Scheme.Equals("s3", StringComparison.OrdinalIgnoreCase) ? Uri.UriSchemeHttps : uri.Scheme;
        var serviceUrl = $"{serviceScheme}://{serviceHost}";
        var objectUrlRoot = $"{uri.Scheme}://{uri.Authority}/{bucketName}";
        var isAwsEndpoint = serviceHost.EndsWith(".amazonaws.com", StringComparison.OrdinalIgnoreCase);

        return new S3BackupLocation(serviceUrl, objectUrlRoot, bucketName, prefix.Trim('/'), GetAwsRegion(serviceHost), isAwsEndpoint);
    }

    public string BuildObjectUrl(string key)
    {
        return $"{ObjectUrlRoot.TrimEnd('/')}/{key.TrimStart('/')}";
    }

    private static string? GetAwsRegion(string host)
    {
        var parts = host.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var s3Index = Array.IndexOf(parts, "s3");
        return s3Index >= 0 && parts.Length > s3Index + 1 && !parts[s3Index + 1].Equals("amazonaws", StringComparison.OrdinalIgnoreCase)
            ? parts[s3Index + 1]
            : null;
    }
}

public sealed record OperationProgress(string Stage, long? BytesTransferred = null, long? TotalBytes = null);
