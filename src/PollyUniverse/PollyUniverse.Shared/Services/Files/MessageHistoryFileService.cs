using PollyUniverse.Shared.Aws.Services;
using PollyUniverse.Shared.Utils;

namespace PollyUniverse.Shared.Services.Files;

public interface IMessageHistoryFileService
{
    Task<string> DownloadMessageHistoryFile(string s3Bucket, long telegramPeerId);
}

public class MessageHistoryFileService : IMessageHistoryFileService
{
    private const string MessageHistoryBucketPrefix = "message-history";

    private readonly IS3Service _s3Service;
    private readonly string _tmpDirectory;

    public MessageHistoryFileService(IS3Service s3Service)
    {
        _s3Service = s3Service;
        _tmpDirectory = TmpDirectoryUtils.GetTmpDirectory();
    }

    public async Task<string> DownloadMessageHistoryFile(string s3Bucket, long telegramPeerId)
    {
        var fileName = $"{telegramPeerId}.csv";
        var remoteFilePath = $"{MessageHistoryBucketPrefix}/{fileName}";
        var localFilePath = $"{_tmpDirectory}/{remoteFilePath}";

        if (File.Exists(localFilePath))
        {
            return localFilePath;
        }

        var success = await _s3Service.Download(s3Bucket, remoteFilePath, localFilePath);

        return success ? localFilePath : null;
    }
}
