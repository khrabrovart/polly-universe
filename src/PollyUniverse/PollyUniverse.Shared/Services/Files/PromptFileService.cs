using PollyUniverse.Shared.Aws.Services;
using PollyUniverse.Shared.Utils;

namespace PollyUniverse.Shared.Services.Files;

public interface IPromptFileService
{
    Task<Dictionary<string, string>> DownloadPromptFiles(string[] promptIds);
}

public class PromptFileService : IPromptFileService
{
    private const string PromptsBucketPrefix = "prompts";

    private readonly IS3Service _s3Service;
    private readonly string _bucketName;
    private readonly string _tmpDirectory;

    public PromptFileService(IS3Service s3Service, ISharedConfig config)
    {
        _s3Service = s3Service;
        _bucketName = config.S3Bucket;
        _tmpDirectory = TmpDirectoryUtils.GetTmpDirectory();
    }

    public async Task<Dictionary<string, string>> DownloadPromptFiles(string[] promptIds)
    {
        var tasks = promptIds.Select(async promptId =>
        {
            var filePath = await DownloadPromptFile(promptId);
            return (promptId, filePath);
        });

        var results = await Task.WhenAll(tasks);

        return results.ToDictionary(result => result.promptId, result => result.filePath);
    }

    private async Task<string> DownloadPromptFile(string promptId)
    {
        var fileName = $"{promptId}.md";
        var remoteFilePath = $"{PromptsBucketPrefix}/{fileName}";
        var localFilePath = $"{_tmpDirectory}/{remoteFilePath}";

        if (File.Exists(localFilePath))
        {
            return localFilePath;
        }

        var success = await _s3Service.Download(_bucketName, remoteFilePath, localFilePath);

        return success ? localFilePath : null;
    }
}
