using Microsoft.Extensions.Configuration;
using PollyUniverse.Shared.Extensions;

namespace PollyUniverse.Func.Voting;

public class FunctionConfig
{
    public FunctionConfig(IConfigurationRoot config)
    {
        S3Bucket = config.GetOrThrow("S3_BUCKET");
        PollWaitingMinutes = int.Parse(config.GetOrThrow("POLL_WAITING_MINUTES"));
        BotTokenParameter = config.GetOrThrow("BOT_TOKEN_PARAMETER");
        NotificationsPeerIdParameter = config.GetOrThrow("NOTIFICATIONS_PEER_ID_PARAMETER");
        OpenAIApiKeyParameter = config.GetOrThrow("OPENAI_API_KEY_PARAMETER");
        OpenAIModel = config.GetOrThrow("OPENAI_MODEL");

        DevUseFakeVotingResult = config["DEV:USE_FAKE_VOTING_RESULT"];
        DevMuteNotifications = bool.TryParse(config["DEV:MUTE_NOTIFICATIONS"], out var muteNotifications) && muteNotifications;
    }

    public string S3Bucket { get; set; }

    public int PollWaitingMinutes { get; set; }

    public string BotTokenParameter { get; set; }

    public string NotificationsPeerIdParameter { get; set; }

    public string OpenAIApiKeyParameter { get; set; }

    public string OpenAIModel { get; set; }

    #region Development Settings

    public string DevUseFakeVotingResult { get; set; }

    public bool DevMuteNotifications { get; set; }

    #endregion
}
