using PollyUniverse.Voting.Func.Services.Files;

namespace PollyUniverse.Voting.Func.Services;

public interface IPromptService
{
    Task<string> GetFullPrompt(string promptId);
}

public class PromptService : IPromptService
{
    private const string SystemBasePromptId = "system_base";
    private const string SystemFormatPromptId = "system_format";

    private readonly IPromptFileService _promptFileService;

    public PromptService(IPromptFileService promptFileService)
    {
        _promptFileService = promptFileService;
    }

    public async Task<string> GetFullPrompt(string promptId)
    {
        var promptsOrder = new[] { SystemBasePromptId, promptId, SystemFormatPromptId };
        var promptFilesTask = await _promptFileService.DownloadPromptFiles(promptsOrder);

        if (promptFilesTask.Values.Any(v => v == null))
        {
            throw new FileNotFoundException("One or more prompt files not found");
        }

        return string.Join(
            Environment.NewLine + Environment.NewLine,
            await Task.WhenAll(promptsOrder.Select(id => File.ReadAllTextAsync(promptFilesTask[id]))));
    }
}
