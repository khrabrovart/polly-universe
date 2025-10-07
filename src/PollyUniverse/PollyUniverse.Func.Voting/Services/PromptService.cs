using Microsoft.Extensions.Logging;
using PollyUniverse.Func.Voting.Services.Files;

namespace PollyUniverse.Func.Voting.Services;

public interface IPromptService
{
    Task<string> GetFullPrompt(string promptId);
}

public class PromptService : IPromptService
{
    private const string LocalPromptsFolder = "./Prompts";

    private const string SystemBasePromptId = "system_base";
    private const string SystemFormatPromptId = "system_format";

    private readonly IPromptFileService _promptFileService;
    private ILogger<PromptService> _logger;
    private FunctionConfig _config;

    public PromptService(
        IPromptFileService promptFileService,
        ILogger<PromptService> logger,
        FunctionConfig config)
    {
        _promptFileService = promptFileService;
        _logger = logger;
        _config = config;
    }

    public async Task<string> GetFullPrompt(string promptId)
    {
        var orderedPrompts = new[] { SystemBasePromptId, promptId, SystemFormatPromptId };

        Dictionary<string, string> promptFilePaths;

        if (_config.UseLocalPrompts)
        {
            _logger.LogInformation("Using local prompts");
            promptFilePaths = GetLocalPromptFilePaths(orderedPrompts);
        }
        else
        {
            promptFilePaths = await _promptFileService.DownloadPromptFiles(orderedPrompts);

            if (promptFilePaths.Values.Any(v => v == null))
            {
                throw new FileNotFoundException("One or more prompt files not found");
            }
        }

        return string.Join(
            Environment.NewLine,
            await Task.WhenAll(orderedPrompts.Select(id => File.ReadAllTextAsync(promptFilePaths[id]))));
    }

    private static Dictionary<string, string> GetLocalPromptFilePaths(params string[] promptIds)
    {
        return promptIds.ToDictionary(
            id => id,
            id => Path.Combine(LocalPromptsFolder, $"{id}.md"));
    }
}
