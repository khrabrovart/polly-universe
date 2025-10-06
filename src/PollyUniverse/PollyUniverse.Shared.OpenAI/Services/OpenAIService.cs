using OpenAI;

namespace PollyUniverse.Shared.OpenAI.Services;

public interface IOpenAIService
{
}

public class OpenAIService : IOpenAIService
{
    private readonly OpenAIClient _client;

    public OpenAIService(OpenAIClient client)
    {
        _client = client;
    }

    public async Task GetResponse()
    {

    }
}
