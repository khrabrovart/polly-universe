using PollyUniverse.Shared.AWS;

namespace PollyUniverse.Voting.Func.Services;

public interface ISessionService
{
    Task<string> DownloadSessionFile(string sessionId);
}

public class SessionService : ISessionService
{
    private const string SessionBucketPrefix = "sessions";
    private const string LocalSessionFileName = "default";

    private readonly IS3Client _s3Client;
    private readonly FunctionConfig _config;
    private readonly string _localSessionFilePath;

    public SessionService(IS3Client s3Client, FunctionConfig config)
    {
        _s3Client = s3Client;
        _config = config;

        var localFolder = _config.IsDev ? "./tmp" : "/tmp";
        _localSessionFilePath = $"{localFolder}/{LocalSessionFileName}.session";
    }

    public async Task<string> DownloadSessionFile(string sessionId)
    {
        var success = await _s3Client.Download(
            _config.S3Bucket,
            $"{SessionBucketPrefix}/{sessionId}.session",
            _localSessionFilePath);

        return success ? _localSessionFilePath : null;
    }
}
