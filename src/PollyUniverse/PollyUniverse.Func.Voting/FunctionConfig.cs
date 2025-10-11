using Microsoft.Extensions.Configuration;
using PollyUniverse.Shared.Utils.Extensions;

namespace PollyUniverse.Func.Voting;

public interface IFunctionConfig
{
    int PollWaitingMinutes { get; set; }

    string BotTokenParameter { get; set; }

    string NotificationsPeerIdParameter { get; set; }

    string OpenAIApiKeyParameter { get; set; }

    string OpenAIModel { get; set; }

    #region Development Settings

    string DevFakeVotingResult { get; set; }

    bool DevMuteNotifications { get; set; }

    #endregion
}

public class FunctionConfig : IFunctionConfig
{
    public FunctionConfig(IConfiguration configuration)
    {
        PollWaitingMinutes = int.Parse(configuration.GetOrThrow("POLL_WAITING_MINUTES"));
        BotTokenParameter = configuration.GetOrThrow("BOT_TOKEN_PARAMETER");
        NotificationsPeerIdParameter = configuration.GetOrThrow("NOTIFICATIONS_PEER_ID_PARAMETER");
        OpenAIApiKeyParameter = configuration.GetOrThrow("OPENAI_API_KEY_PARAMETER");
        OpenAIModel = configuration.GetOrThrow("OPENAI_MODEL");

        DevFakeVotingResult = configuration["DEV:FAKE_VOTING_RESULT"];
        DevMuteNotifications = bool.TryParse(configuration["DEV:MUTE_NOTIFICATIONS"], out var muteNotifications) && muteNotifications;
    }

    public int PollWaitingMinutes { get; set; }

    public string BotTokenParameter { get; set; }

    public string NotificationsPeerIdParameter { get; set; }

    public string OpenAIApiKeyParameter { get; set; }

    public string OpenAIModel { get; set; }

    #region Development Settings

    public string DevFakeVotingResult { get; set; }

    public bool DevMuteNotifications { get; set; }

    #endregion
}
