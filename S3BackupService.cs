using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace DatabaseTools;

public static class S3BackupService
{
    public static async Task<IReadOnlyList<BackupHistoryItem>> GetBackupsAsync(
        string databaseName,
        string s3Url,
        S3CredentialOptions s3Credentials,
        CancellationToken cancellationToken)
    {
        var location = S3BackupLocation.FromUrl(s3Url, databaseName);
        using var client = CreateS3Client(location, s3Credentials);

        var backups = new List<BackupHistoryItem>();
        string? continuationToken = null;

        do
        {
            var response = await client.ListObjectsV2Async(new ListObjectsV2Request
            {
                BucketName = location.BucketName,
                Prefix = location.Prefix.TrimEnd('/') + "/",
                ContinuationToken = continuationToken
            }, cancellationToken);

            backups.AddRange((response.S3Objects ?? [])
                .Where(IsBackupObject)
                .Select(item => new BackupHistoryItem(
                    location.BuildObjectUrl(item.Key),
                    item.LastModified,
                    item.Size)));

            continuationToken = response.NextContinuationToken;
        } while (!string.IsNullOrWhiteSpace(continuationToken));

        return backups
            .OrderByDescending(item => item.FinishedAt)
            .ToList();
    }

    public static async Task<IReadOnlyList<DatabaseBackupSummary>> GetLatestBackupsAsync(
        string s3Url,
        S3CredentialOptions s3Credentials,
        CancellationToken cancellationToken)
    {
        var location = S3BackupLocation.FromUrl(s3Url, string.Empty);
        using var client = CreateS3Client(location, s3Credentials);

        var latestBackups = new Dictionary<string, DatabaseBackupSummary>(StringComparer.Ordinal);
        string? continuationToken = null;

        do
        {
            var response = await client.ListObjectsV2Async(new ListObjectsV2Request
            {
                BucketName = location.BucketName,
                Prefix = string.IsNullOrEmpty(location.Prefix) ? null : location.Prefix + "/",
                ContinuationToken = continuationToken
            }, cancellationToken);

            foreach (var item in response.S3Objects ?? [])
            {
                if (!IsBackupObject(item))
                    continue;

                var databaseName = GetDatabaseNameFromKey(item.Key, location.Prefix);
                if (string.IsNullOrEmpty(databaseName))
                    continue;

                if (latestBackups.TryGetValue(databaseName, out var current)
                    && Nullable.Compare(item.LastModified, current.LatestBackup.FinishedAt) <= 0)
                {
                    continue;
                }

                latestBackups[databaseName] = new DatabaseBackupSummary(
                    databaseName,
                    new BackupHistoryItem(location.BuildObjectUrl(item.Key), item.LastModified, item.Size));
            }

            continuationToken = response.NextContinuationToken;
        } while (!string.IsNullOrWhiteSpace(continuationToken));

        return latestBackups.Values
            .OrderBy(summary => summary.DatabaseName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string GetDatabaseNameFromKey(string key, string basePrefix)
    {
        var relative = string.IsNullOrEmpty(basePrefix) ? key : key[(basePrefix.Length + 1)..];
        var slashIndex = relative.IndexOf('/');
        return slashIndex > 0 ? relative[..slashIndex] : string.Empty;
    }

    public static string BuildBackupUrl(string backupFile, string s3Url)
    {
        if (Uri.TryCreate(backupFile, UriKind.Absolute, out _))
        {
            return backupFile;
        }

        return $"{s3Url.TrimEnd('/')}/{backupFile.TrimStart('/')}";
    }

    public static async Task UploadAsync(
        string localPath,
        string backupUrl,
        S3CredentialOptions credentials,
        bool overwrite,
        Action<string>? log,
        CancellationToken cancellationToken,
        IProgress<OperationProgress>? progress = null)
    {
        var location = S3BackupLocation.FromObjectUrl(backupUrl);
        using var client = CreateS3Client(location, credentials);

        if (!overwrite && await S3ObjectExistsAsync(client, location.BucketName, location.Prefix, cancellationToken))
        {
            throw new InvalidOperationException("S3 backup file already exists. Enable overwrite or choose another file name.");
        }

        log?.Invoke($"Uploading dump to S3: {backupUrl}");
        var request = new PutObjectRequest
        {
            BucketName = location.BucketName,
            Key = location.Prefix,
            FilePath = localPath
        };
        var lastReport = Environment.TickCount64;
        request.StreamTransferProgress += (_, e) =>
        {
            if (Environment.TickCount64 - lastReport >= 100 || e.TransferredBytes == e.TotalBytes)
            {
                progress?.Report(new OperationProgress("Uploading backup", e.TransferredBytes, e.TotalBytes));
                lastReport = Environment.TickCount64;
            }
        };
        progress?.Report(new OperationProgress("Uploading backup", 0, new FileInfo(localPath).Length));
        await client.PutObjectAsync(request, cancellationToken);
    }

    public static async Task DownloadAsync(
        string backupUrl,
        string localPath,
        S3CredentialOptions credentials,
        Action<string>? log,
        CancellationToken cancellationToken,
        IProgress<OperationProgress>? progress = null)
    {
        var location = S3BackupLocation.FromObjectUrl(backupUrl);
        using var client = CreateS3Client(location, credentials);

        Directory.CreateDirectory(Path.GetDirectoryName(localPath)!);
        log?.Invoke($"Downloading dump from S3: {backupUrl}");
        using var response = await client.GetObjectAsync(location.BucketName, location.Prefix, cancellationToken);
        await using var stream = File.Create(localPath);
        var buffer = new byte[128 * 1024];
        long transferred = 0;
        var lastReport = Environment.TickCount64;
        progress?.Report(new OperationProgress("Downloading backup", 0, response.ContentLength));
        int count;
        while ((count = await response.ResponseStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
        {
            await stream.WriteAsync(buffer.AsMemory(0, count), cancellationToken).ConfigureAwait(false);
            transferred += count;
            if (Environment.TickCount64 - lastReport >= 100)
            {
                progress?.Report(new OperationProgress("Downloading backup", transferred, response.ContentLength));
                lastReport = Environment.TickCount64;
            }
        }
        progress?.Report(new OperationProgress("Downloading backup", transferred, response.ContentLength));
    }

    private static async Task<bool> S3ObjectExistsAsync(IAmazonS3 client, string bucketName, string key, CancellationToken cancellationToken)
    {
        try
        {
            await client.GetObjectMetadataAsync(bucketName, key, cancellationToken);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    private static IAmazonS3 CreateS3Client(S3BackupLocation location, S3CredentialOptions credentials)
    {
        var config = new AmazonS3Config
        {
            ForcePathStyle = true
        };

        if (location.IsAwsEndpoint)
        {
            config.RegionEndpoint = RegionEndpoint.GetBySystemName(location.RegionName ?? "us-east-1");
        }
        else
        {
            config.ServiceURL = location.ServiceUrl;
            config.AuthenticationRegion = location.RegionName ?? "us-east-1";
        }

        if (credentials.HasExplicitCredentials)
        {
            return new AmazonS3Client(
                new BasicAWSCredentials(credentials.AccessKey, credentials.SecretKey),
                config);
        }

        return new AmazonS3Client(config);
    }

    private static bool IsBackupObject(S3Object item)
    {
        return item.Key.EndsWith(".dump", StringComparison.OrdinalIgnoreCase)
               || item.Key.EndsWith(".backup", StringComparison.OrdinalIgnoreCase);
    }
}