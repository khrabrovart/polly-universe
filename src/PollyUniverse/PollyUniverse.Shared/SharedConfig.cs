using Microsoft.Extensions.Configuration;
using PollyUniverse.Shared.Utils.Extensions;

namespace PollyUniverse.Shared;

public interface ISharedConfig
{
    string S3Bucket { get; set; }

    string SessionMetadataTable { get; set; }

    string VotingProfilesTable { get; set; }
}

public class SharedConfig : ISharedConfig
{
    public SharedConfig(IConfiguration configuration)
    {
        S3Bucket = configuration.GetOrThrow("S3_BUCKET");
        SessionMetadataTable = configuration.GetOrThrow("SESSION_METADATA_TABLE");
        VotingProfilesTable = configuration.GetOrThrow("VOTING_PROFILES_TABLE");
    }

    public string S3Bucket { get; set; }

    public string SessionMetadataTable { get; set; }

    public string VotingProfilesTable { get; set; }
}
