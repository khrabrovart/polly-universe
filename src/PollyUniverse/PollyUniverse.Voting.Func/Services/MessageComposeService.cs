using PollyUniverse.Shared.OpenAI.Services;

namespace PollyUniverse.Voting.Func.Services;

public interface IMessageComposeService
{
    Task<string> ComposeMessage(
        string openAiApiKey,
        string openAiModel,
        string prompt,
        Dictionary<string, string> parameters);
}

public class MessageComposeService : IMessageComposeService
{
    private readonly IOpenAIService _openAIService;

    public MessageComposeService(IOpenAIService openAIService)
    {
        _openAIService = openAIService;
    }

    public async Task<string> ComposeMessage(
        string openAiApiKey,
        string openAiModel,
        string prompt,
        Dictionary<string, string> parameters)
    {
        var composedPrompt = ApplyParameters(prompt, parameters);

        return await _openAIService.CompleteChat(openAiApiKey, openAiModel, composedPrompt);
    }

    private static string ApplyParameters(string prompt, Dictionary<string, string> parameters)
    {
        if (parameters == null || !parameters.Any())
        {
            return prompt;
        }

        return parameters
            .Aggregate(prompt, (current, param) => current.Replace($"<%{param.Key}%>", param.Value));
    }
}
