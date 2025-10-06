using Microsoft.Extensions.Configuration;
using PollyUniverse.Shared.Extensions;

namespace PollyUniverse.Voting.Func;

public class FunctionConfig
{
    public FunctionConfig(IConfigurationRoot config)
    {
        SessionMetadataTable = config.GetOrThrow("SESSION_METADATA_TABLE");
        VotingProfilesTable = config.GetOrThrow("VOTING_PROFILES_TABLE");
        S3Bucket = config.GetOrThrow("S3_BUCKET");
        PollWaitingMinutes = int.Parse(config.GetOrThrow("POLL_WAITING_MINUTES"));
        IsDev = bool.Parse(config.GetOrThrow("DEV:ENABLED"));
    }

    public string SessionMetadataTable { get; set; }

    public string VotingProfilesTable { get; set; }

    public string S3Bucket { get; set; }

    public int PollWaitingMinutes { get; set; }

    public bool IsDev { get; set; }
}
