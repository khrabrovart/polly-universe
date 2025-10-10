using PollyUniverse.Shared.Aws.Services;
using PollyUniverse.Shared.Utils;

namespace PollyUniverse.Shared.Services.Files;

public interface ISessionFileService
{
    Task<string> DownloadSessionFile(string s3Bucket, string sessionId);
}

public class SessionFileService : ISessionFileService
{
    private const string SessionsBucketPrefix = "sessions";

    private readonly IS3Service _s3Service;
    private readonly string _tmpDirectory;

    public SessionFileService(IS3Service s3Service)
    {
        _s3Service = s3Service;
        _tmpDirectory = TmpDirectoryUtils.GetTmpDirectory();
    }

    public async Task<string> DownloadSessionFile(string s3Bucket, string sessionId)
    {
        var fileName = $"{sessionId}.session";
        var remoteFilePath = $"{SessionsBucketPrefix}/{fileName}";
        var localFilePath = $"{_tmpDirectory}/{remoteFilePath}";

        if (File.Exists(localFilePath))
        {
            return localFilePath;
        }

        var success = await _s3Service.Download(s3Bucket, remoteFilePath, localFilePath);

        return success ? localFilePath : null;
    }
}
