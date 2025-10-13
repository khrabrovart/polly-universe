using Microsoft.Extensions.Configuration;
using PollyUniverse.Shared.Utils.Extensions;

namespace PollyUniverse.Func.Agent;

public interface IFunctionConfig
{
    string BotTokenParameter { get; set; }

    string OpenAIApiKeyParameter { get; set; }

    string OpenAIModel { get; set; }

    int HistoryLength { get; set; }

    #region Development Settings

    bool DevFakeService { get; set; }

    #endregion
}

public class FunctionConfig : IFunctionConfig
{
    public FunctionConfig(IConfiguration configuration)
    {
        BotTokenParameter = configuration.GetOrThrow("BOT_TOKEN_PARAMETER");
        OpenAIApiKeyParameter = configuration.GetOrThrow("OPENAI_API_KEY_PARAMETER");
        OpenAIModel = configuration.GetOrThrow("OPENAI_MODEL");
        HistoryLength = int.Parse(configuration.GetOrThrow("HISTORY_LENGTH"));

        DevFakeService = bool.TryParse(configuration["DEV:FAKE_SERVICE"], out var devFakeService) && devFakeService;
    }

    public string BotTokenParameter { get; set; }

    public string OpenAIApiKeyParameter { get; set; }

    public string OpenAIModel { get; set; }

    public int HistoryLength { get; set; }

    #region Development Settings

    public bool DevFakeService { get; set; }

    #endregion
}
