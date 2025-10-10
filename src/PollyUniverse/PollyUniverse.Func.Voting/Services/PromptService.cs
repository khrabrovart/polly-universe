using Microsoft.Extensions.Logging;
using PollyUniverse.Shared.Services.Files;

namespace PollyUniverse.Func.Voting.Services;

public interface IPromptService
{
    Task<string> GetFullPrompt(string promptId);
}

public class PromptService : IPromptService
{
    private const string SystemBasePromptId = "system_base";
    private const string SystemFormatPromptId = "system_format";

    private readonly IPromptFileService _promptFileService;
    private readonly FunctionConfig _config;

    public PromptService(
        IPromptFileService promptFileService,
        FunctionConfig config)
    {
        _promptFileService = promptFileService;
        _config = config;
    }

    public async Task<string> GetFullPrompt(string promptId)
    {
        var orderedPrompts = new[] { SystemBasePromptId, promptId, SystemFormatPromptId };
        var promptFilePaths = await _promptFileService.DownloadPromptFiles(_config.S3Bucket, orderedPrompts);

        if (promptFilePaths.Values.Any(v => v == null))
        {
            throw new FileNotFoundException("One or more prompt files not found");
        }

        return string.Join(
            Environment.NewLine,
            await Task.WhenAll(orderedPrompts.Select(id => File.ReadAllTextAsync(promptFilePaths[id]))));
    }
}
