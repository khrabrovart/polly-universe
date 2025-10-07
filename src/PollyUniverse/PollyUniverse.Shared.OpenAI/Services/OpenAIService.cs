using OpenAI.Chat;

namespace PollyUniverse.Shared.OpenAI.Services;

public interface IOpenAIService
{
    Task<string> CompleteChat(string apiKey, string model, string message);
}

public class OpenAIService : IOpenAIService
{
    public async Task<string> CompleteChat(string apiKey, string model, string message)
    {
        var client = new ChatClient(model, apiKey);

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(message)
        };

        var response = await client.CompleteChatAsync(messages);

        return response.Value.Content[0].Text;
    }
}
