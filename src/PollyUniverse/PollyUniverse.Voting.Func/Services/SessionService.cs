using PollyUniverse.Shared.Aws.Services;

namespace PollyUniverse.Voting.Func.Services;

public interface ISessionService
{
    Task<string> DownloadSessionFile(string sessionId);
}

public class SessionService : ISessionService
{
    private const string SessionBucketPrefix = "sessions";
    private const string LocalSessionFileName = "default";

    private readonly IS3Service _is3Service;
    private readonly FunctionConfig _config;
    private readonly string _localSessionFilePath;

    public SessionService(IS3Service is3Service, FunctionConfig config)
    {
        _is3Service = is3Service;
        _config = config;

        var localFolder = _config.IsDev ? "./tmp" : "/tmp";
        _localSessionFilePath = $"{localFolder}/{LocalSessionFileName}.session";
    }

    public async Task<string> DownloadSessionFile(string sessionId)
    {
        var success = await _is3Service.Download(
            _config.S3Bucket,
            $"{SessionBucketPrefix}/{sessionId}.session",
            _localSessionFilePath);

        return success ? _localSessionFilePath : null;
    }
}
