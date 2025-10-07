using PollyUniverse.Shared.OpenAI.Services;
using PollyUniverse.Func.Scheduling.Models;

namespace PollyUniverse.Func.Scheduling.Services;

public interface IMessageComposeService
{
    Task<string> ComposeMessage(
        VotingResult votingResult,
        long userId,
        string userName,
        string openAiApiKey,
        string openAiModel,
        string prompt,
        Dictionary<string, string> parameters);
}

public class MessageComposeService : IMessageComposeService
{
    private readonly IOpenAIService _openAIService;

    private static readonly Dictionary<VotingResult, string> VotingResultMarks = new()
    {
        { VotingResult.Success, "✅"},
        { VotingResult.PollNotFound, "🔎"},
        { VotingResult.VoteFailed, "❌"}
    };

    public MessageComposeService(IOpenAIService openAIService)
    {
        _openAIService = openAIService;
    }

    public async Task<string> ComposeMessage(
        VotingResult votingResult,
        long userId,
        string userName,
        string openAiApiKey,
        string openAiModel,
        string prompt,
        Dictionary<string, string> parameters)
    {
        var composedPrompt = ApplyParameters(prompt, parameters);
        var generatedMessage =  await _openAIService.CompleteChat(openAiApiKey, openAiModel, composedPrompt);

        return PostprocessMessage(generatedMessage, userId, userName, votingResult);
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

    private static string PostprocessMessage(string message, long userId, string userName, VotingResult votingResult)
    {
        var userMention = $"[{userName}](tg://user?id={userId})";
        var mark = VotingResultMarks.GetValueOrDefault(votingResult, string.Empty);

        return $"{mark} {message}".Replace("PLAYER_NAME", userMention);
    }
}
