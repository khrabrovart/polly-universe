using PollyUniverse.Shared.Aws.Services;
using PollyUniverse.Shared.Telegram.Models;
using PollyUniverse.Shared.Utils;

namespace PollyUniverse.Shared.Services.Files;

public interface IMessageHistoryFileService
{
    (string RemotePath, string LocalPath) GetFilePaths(TelegramShortPeerId peerId);

    Task<string> DownloadMessageHistoryFile(TelegramShortPeerId peerId);

    Task<bool> UploadMessageHistoryFile(TelegramShortPeerId peerId);
}

public class MessageHistoryFileService : IMessageHistoryFileService
{
    private const string MessageHistoryBucketPrefix = "message-history";

    private readonly IS3Service _s3Service;
    private readonly string _bucketName;
    private readonly string _tmpDirectory;

    public MessageHistoryFileService(IS3Service s3Service, ISharedConfig config)
    {
        _s3Service = s3Service;
        _bucketName = config.S3Bucket;
        _tmpDirectory = TmpDirectoryUtils.GetTmpDirectory();
    }

    public (string RemotePath, string LocalPath) GetFilePaths(TelegramShortPeerId peerId)
    {
        var fileName = $"{peerId}.csv";
        var remoteFilePath = $"{MessageHistoryBucketPrefix}/{fileName}";
        var localFilePath = $"{_tmpDirectory}/{remoteFilePath}";
        return (remoteFilePath, localFilePath);
    }

    public async Task<string> DownloadMessageHistoryFile(TelegramShortPeerId peerId)
    {
        var (remoteFilePath, localFilePath) = GetFilePaths(peerId);

        if (File.Exists(localFilePath))
        {
            return localFilePath;
        }

        var success = await _s3Service.Download(_bucketName, remoteFilePath, localFilePath);

        return success ? localFilePath : null;
    }

    public async Task<bool> UploadMessageHistoryFile(TelegramShortPeerId peerId)
    {
        var (remoteFilePath, localFilePath) = GetFilePaths(peerId);

        if (!File.Exists(localFilePath))
        {
            return false;
        }

        return await _s3Service.UploadFile(_bucketName, remoteFilePath, localFilePath);
    }
}
