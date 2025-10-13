using Microsoft.Extensions.Configuration;
using PollyUniverse.Shared.Utils.Extensions;

namespace PollyUniverse.Func.Voting;

public interface IFunctionConfig
{
    int PollWaitingMinutes { get; }

    string BotTokenParameter { get; }

    string NotificationsPeerIdParameter { get; }

    string OpenAIApiKeyParameter { get; }

    string OpenAIModel { get; }

    #region Development Settings

    string DevFakeVotingResult { get; }

    bool DevMuteNotifications { get; }

    #endregion
}

public record FunctionConfig : IFunctionConfig
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

    public int PollWaitingMinutes { get; }

    public string BotTokenParameter { get; }

    public string NotificationsPeerIdParameter { get; }

    public string OpenAIApiKeyParameter { get; }

    public string OpenAIModel { get; }

    #region Development Settings

    public string DevFakeVotingResult { get; }

    public bool DevMuteNotifications { get; }

    #endregion
}
