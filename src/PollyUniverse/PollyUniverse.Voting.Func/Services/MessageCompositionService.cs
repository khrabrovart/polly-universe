using PollyUniverse.Shared.OpenAI.Services;

namespace PollyUniverse.Voting.Func.Services;

public interface IMessageCompositionService
{
    Task<string> ComposeMessage(
        string openAiApiKey,
        string openAiModel,
        string promptFilePath,
        Dictionary<string, string> parameters);
}

public class MessageCompositionService : IMessageCompositionService
{
    private readonly IOpenAIService _openAIService;

    public MessageCompositionService(IOpenAIService openAIService)
    {
        _openAIService = openAIService;
    }

    public async Task<string> ComposeMessage(
        string openAiApiKey,
        string openAiModel,
        string promptFilePath,
        Dictionary<string, string> parameters)
    {
        var promptTemplate = await File.ReadAllTextAsync(promptFilePath);
        var composedPrompt = ApplyParameters(promptTemplate, parameters);

        return await _openAIService.CompleteChat(openAiApiKey, openAiModel, composedPrompt);
    }

    private static string ApplyParameters(string prompt, Dictionary<string, string> parameters)
    {
        return parameters
            .Aggregate(prompt, (current, param) => current.Replace($"<%{param.Key}%>", param.Value));
    }
}
