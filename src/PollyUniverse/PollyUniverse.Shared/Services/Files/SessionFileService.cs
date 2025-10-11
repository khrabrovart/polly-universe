using PollyUniverse.Shared.Aws.Services;
using PollyUniverse.Shared.Utils;

namespace PollyUniverse.Shared.Services.Files;

public interface ISessionFileService
{
    Task<string> DownloadSessionFile(string sessionId);
}

public class SessionFileService : ISessionFileService
{
    private const string SessionsBucketPrefix = "sessions";

    private readonly IS3Service _s3Service;
    private readonly string _bucketName;
    private readonly string _tmpDirectory;

    public SessionFileService(IS3Service s3Service, ISharedConfig config)
    {
        _s3Service = s3Service;
        _bucketName = config.S3Bucket;
        _tmpDirectory = TmpDirectoryUtils.GetTmpDirectory();
    }

    public async Task<string> DownloadSessionFile(string sessionId)
    {
        var fileName = $"{sessionId}.session";
        var remoteFilePath = $"{SessionsBucketPrefix}/{fileName}";
        var localFilePath = $"{_tmpDirectory}/{remoteFilePath}";

        if (File.Exists(localFilePath))
        {
            return localFilePath;
        }

        var success = await _s3Service.Download(_bucketName, remoteFilePath, localFilePath);

        return success ? localFilePath : null;
    }
}
