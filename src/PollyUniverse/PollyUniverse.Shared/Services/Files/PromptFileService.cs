using PollyUniverse.Shared.Aws.Services;
using PollyUniverse.Shared.Utils;

namespace PollyUniverse.Shared.Services.Files;

public interface IPromptFileService
{
    Task<Dictionary<string, string>> DownloadPromptFiles(string s3Bucket, string[] promptIds);
}

public class PromptFileService : IPromptFileService
{
    private const string PromptsBucketPrefix = "prompts";

    private readonly IS3Service _s3Service;
    private readonly string _tmpDirectory;

    public PromptFileService(IS3Service s3Service)
    {
        _s3Service = s3Service;
        _tmpDirectory = TmpDirectoryUtils.GetTmpDirectory();
    }

    public async Task<Dictionary<string, string>> DownloadPromptFiles(string s3Bucket, string[] promptIds)
    {
        var tasks = promptIds.Select(async promptId =>
        {
            var filePath = await DownloadPromptFile(s3Bucket, promptId);
            return (promptId, filePath);
        });

        var results = await Task.WhenAll(tasks);

        return results.ToDictionary(result => result.promptId, result => result.filePath);
    }

    private async Task<string> DownloadPromptFile(string s3Bucket, string promptId)
    {
        var fileName = $"{promptId}.md";
        var remoteFilePath = $"{PromptsBucketPrefix}/{fileName}";
        var localFilePath = $"{_tmpDirectory}/{remoteFilePath}";

        if (File.Exists(localFilePath))
        {
            return localFilePath;
        }

        var success = await _s3Service.Download(s3Bucket, remoteFilePath, localFilePath);

        return success ? localFilePath : null;
    }
}
