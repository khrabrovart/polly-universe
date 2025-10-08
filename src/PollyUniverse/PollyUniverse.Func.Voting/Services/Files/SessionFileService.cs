using PollyUniverse.Shared.Aws.Services;

namespace PollyUniverse.Func.Voting.Services.Files;

public interface ISessionFileService
{
    Task<string> DownloadSessionFile(string sessionId);
}

public class SessionFileService : ISessionFileService
{
    private const string SessionBucketPrefix = "sessions";

    private readonly IS3Service _s3Service;
    private readonly FunctionConfig _config;
    private readonly string _localFolder;

    public SessionFileService(IS3Service s3Service, FunctionConfig config)
    {
        _s3Service = s3Service;
        _config = config;
        _localFolder = _config.DevUseLocalTmpDirectory ? "./tmp" : "/tmp";
    }

    public async Task<string> DownloadSessionFile(string sessionId)
    {
        var fileName = $"{sessionId}.session";
        var remoteFilePath = $"{SessionBucketPrefix}/{fileName}";
        var localFilePath = $"{_localFolder}/{fileName}";

        if (File.Exists(localFilePath))
        {
            return localFilePath;
        }

        var success = await _s3Service.Download(_config.S3Bucket, remoteFilePath, localFilePath);

        return success ? localFilePath : null;
    }
}
