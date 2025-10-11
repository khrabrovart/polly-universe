using PollyUniverse.Shared.Services.Files;

namespace PollyUniverse.Shared.Services;

public interface IPromptService
{
    Task<string> GetPrompt(string[] orderedPromptIds, Dictionary<string, string> parameters);
}

public class PromptService : IPromptService
{
    private readonly IPromptFileService _promptFileService;

    public PromptService(IPromptFileService promptFileService)
    {
        _promptFileService = promptFileService;
    }

    public async Task<string> GetPrompt(string[] orderedPromptIds, Dictionary<string, string> parameters)
    {
        var promptFilePaths = await _promptFileService.DownloadPromptFiles(orderedPromptIds);

        if (promptFilePaths.Values.Any(v => v == null))
        {
            throw new FileNotFoundException("One or more prompt files not found");
        }

        var prompt = string.Join(
            Environment.NewLine,
            await Task.WhenAll(orderedPromptIds.Select(id => File.ReadAllTextAsync(promptFilePaths[id]))));

        return ApplyParameters(prompt, parameters);
    }

    private static string ApplyParameters(string prompt, Dictionary<string, string> parameters)
    {
        if (parameters == null || parameters.Count == 0)
        {
            return prompt;
        }

        return parameters.Aggregate(
            prompt,
            (current, param) => current.Replace($"<%{param.Key}%>", param.Value));
    }
}
