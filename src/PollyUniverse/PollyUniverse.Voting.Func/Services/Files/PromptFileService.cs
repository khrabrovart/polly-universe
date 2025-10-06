using PollyUniverse.Shared.Aws.Services;

namespace PollyUniverse.Voting.Func.Services.Files;

public interface IPromptFileService
{
    Task<string> DownloadPromptFile(string promptId);
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
}
