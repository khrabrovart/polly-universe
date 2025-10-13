using Microsoft.Extensions.Configuration;
using PollyUniverse.Shared.Utils.Extensions;

namespace PollyUniverse.Func.Agent;

public interface IFunctionConfig
{
    string BotTokenParameter { get; }

    string OpenAIApiKeyParameter { get; }

    string OpenAIModel { get; }

    int HistoryLength { get; }

    #region Development Settings

    bool DevFakeService { get; }

    #endregion
}

public record FunctionConfig : IFunctionConfig
{
    public FunctionConfig(IConfiguration configuration)
    {
        BotTokenParameter = configuration.GetOrThrow("BOT_TOKEN_PARAMETER");
        OpenAIApiKeyParameter = configuration.GetOrThrow("OPENAI_API_KEY_PARAMETER");
        OpenAIModel = configuration.GetOrThrow("OPENAI_MODEL");
        HistoryLength = int.Parse(configuration.GetOrThrow("HISTORY_LENGTH"));

        DevFakeService = bool.TryParse(configuration["DEV:FAKE_SERVICE"], out var devFakeService) && devFakeService;
    }

    public string BotTokenParameter { get; }

    public string OpenAIApiKeyParameter { get; }

    public string OpenAIModel { get; }

    public int HistoryLength { get; }

    #region Development Settings

    public bool DevFakeService { get; }

    #endregion
}
