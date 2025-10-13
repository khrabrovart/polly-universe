using Microsoft.Extensions.Configuration;
using PollyUniverse.Shared.Utils.Extensions;

namespace PollyUniverse.Shared;

public interface ISharedConfig
{
    string S3Bucket { get; }

    string UsersTable { get; }

    string VotingProfilesTable { get; }
}

public record SharedConfig : ISharedConfig
{
    public SharedConfig(IConfiguration configuration)
    {
        S3Bucket = configuration.GetOrThrow("S3_BUCKET");
        UsersTable = configuration.GetOrThrow("USERS_TABLE");
        VotingProfilesTable = configuration.GetOrThrow("VOTING_PROFILES_TABLE");
    }

    public string S3Bucket { get; }

    public string UsersTable { get; }

    public string VotingProfilesTable { get; }
}
