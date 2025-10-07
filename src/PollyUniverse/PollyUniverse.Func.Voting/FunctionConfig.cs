using Microsoft.Extensions.Configuration;
using PollyUniverse.Shared.Extensions;

namespace PollyUniverse.Func.Voting;

public class FunctionConfig
{
    public FunctionConfig(IConfigurationRoot config)
    {
        SessionMetadataTable = config.GetOrThrow("SESSION_METADATA_TABLE");
        VotingProfilesTable = config.GetOrThrow("VOTING_PROFILES_TABLE");
        S3Bucket = config.GetOrThrow("S3_BUCKET");
        PollWaitingMinutes = int.Parse(config.GetOrThrow("POLL_WAITING_MINUTES"));
        BotTokenParameter = config.GetOrThrow("BOT_TOKEN_PARAMETER");
        NotificationsPeerIdParameter = config.GetOrThrow("NOTIFICATIONS_PEER_ID_PARAMETER");
        OpenAIApiKeyParameter = config.GetOrThrow("OPENAI_API_KEY_PARAMETER");
        OpenAIModel = config.GetOrThrow("OPENAI_MODEL");

        IsDev = bool.TryParse(config["DEV:ENABLED"], out var isDev) && isDev;
        UseLocalPrompts = bool.TryParse(config["DEV:USE_LOCAL_PROMPTS"], out var useLocalPrompts) && useLocalPrompts;
        UseVotingResult = config["DEV:USE_FAKE_VOTING_RESULT"];
    }

    public string SessionMetadataTable { get; set; }

    public string VotingProfilesTable { get; set; }

    public string S3Bucket { get; set; }

    public int PollWaitingMinutes { get; set; }

    public string BotTokenParameter { get; set; }

    public string NotificationsPeerIdParameter { get; set; }

    public string OpenAIApiKeyParameter { get; set; }

    public string OpenAIModel { get; set; }

    #region Development Settings

    public bool IsDev { get; set; }

    public bool UseLocalPrompts { get; set; }

    public string UseVotingResult { get; set; }

    #endregion
}
