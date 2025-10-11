using PollyUniverse.Shared.Services.Files;

namespace PollyUniverse.Func.Voting.Services;

public interface IPromptService
{
    Task<string> GetFullPrompt(string promptId);
}

public class PromptService : IPromptService
{
    private const string SystemBasePromptId = "voting/system_base";
    private const string SystemFormatPromptId = "voting/system_format";

    private readonly IPromptFileService _promptFileService;
    private readonly IFunctionConfig _config;

    public PromptService(
        IPromptFileService promptFileService,
        IFunctionConfig config)
    {
        _promptFileService = promptFileService;
        _config = config;
    }

    public async Task<string> GetFullPrompt(string promptId)
    {
        var orderedPrompts = new[] { SystemBasePromptId, promptId, SystemFormatPromptId };
        var promptFilePaths = await _promptFileService.DownloadPromptFiles(orderedPrompts);

        if (promptFilePaths.Values.Any(v => v == null))
        {
            throw new FileNotFoundException("One or more prompt files not found");
        }

        return string.Join(
            Environment.NewLine,
            await Task.WhenAll(orderedPrompts.Select(id => File.ReadAllTextAsync(promptFilePaths[id]))));
    }
}
