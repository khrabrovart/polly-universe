using PollyUniverse.Shared.Aws.Services;

namespace PollyUniverse.Func.Voting.Services.Files;

public interface IPromptFileService
{
    Task<string> DownloadPromptFile(string promptId);
    Task<Dictionary<string, string>> DownloadPromptFiles(params string[] promptIds);
}

public class PromptFileService : IPromptFileService
{
    private const string PromptBucketPrefix = "prompts";

    private readonly IS3Service _s3Service;
    private readonly FunctionConfig _config;
    private readonly string _localFolder;

    public PromptFileService(IS3Service s3Service, FunctionConfig config)
    {
        _s3Service = s3Service;
        _config = config;
        _localFolder = _config.IsDev ? "./tmp" : "/tmp";
    }

    public async Task<string> DownloadPromptFile(string promptId)
    {
        var fileName = $"{promptId}.md";
        var remoteFilePath = $"{PromptBucketPrefix}/{fileName}";
        var localFilePath = $"{_localFolder}/{fileName}";

        var success = await _s3Service.Download(_config.S3Bucket, remoteFilePath, localFilePath);

        return success ? localFilePath : null;
    }

    public async Task<Dictionary<string, string>> DownloadPromptFiles(params string[] promptIds)
    {
        var tasks = promptIds.Select(async promptId =>
        {
            var filePath = await DownloadPromptFile(promptId);
            return (promptId, filePath);
        });

        var results = await Task.WhenAll(tasks);

        return results.ToDictionary(result => result.promptId, result => result.filePath);
    }
}
