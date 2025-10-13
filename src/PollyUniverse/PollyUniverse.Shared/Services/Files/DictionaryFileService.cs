using PollyUniverse.Shared.Aws.Services;
using PollyUniverse.Shared.Utils;

namespace PollyUniverse.Shared.Services.Files;

public interface IDictionaryFileService
{
    Task<string> DownloadDictionaryFile();
}

public class DictionaryFileService : IDictionaryFileService
{
    private const string DictionaryBucketPrefix = "dictionary";

    private readonly IS3Service _s3Service;
    private readonly string _bucketName;
    private readonly string _tmpDirectory;

    public DictionaryFileService(IS3Service s3Service, ISharedConfig config)
    {
        _s3Service = s3Service;
        _bucketName = config.S3Bucket;
        _tmpDirectory = TmpDirectoryUtils.GetTmpDirectory();
    }

    public async Task<string> DownloadDictionaryFile()
    {
        var fileName = "dictionary.json";
        var remoteFilePath = $"{DictionaryBucketPrefix}/{fileName}";
        var localFilePath = $"{_tmpDirectory}/{remoteFilePath}";

        if (File.Exists(localFilePath))
        {
            return localFilePath;
        }

        var success = await _s3Service.Download(_bucketName, remoteFilePath, localFilePath);

        return success ? localFilePath : null;
    }
}
