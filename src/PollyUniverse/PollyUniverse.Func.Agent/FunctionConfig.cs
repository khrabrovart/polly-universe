using Microsoft.Extensions.Configuration;
using PollyUniverse.Shared.Utils.Extensions;

namespace PollyUniverse.Func.Agent;

public interface IFunctionConfig
{
    string BotTokenParameter { get; set; }

    string OpenAIApiKeyParameter { get; set; }

    string OpenAIModel { get; set; }
}

public class FunctionConfig : IFunctionConfig
{
    public FunctionConfig(IConfiguration configuration)
    {
        BotTokenParameter = configuration.GetOrThrow("BOT_TOKEN_PARAMETER");
        OpenAIApiKeyParameter = configuration.GetOrThrow("OPENAI_API_KEY_PARAMETER");
        OpenAIModel = configuration.GetOrThrow("OPENAI_MODEL");
    }

    public string BotTokenParameter { get; set; }

    public string OpenAIApiKeyParameter { get; set; }

    public string OpenAIModel { get; set; }
}
