namespace PollyUniverse.Voting.Func;

public class FunctionConfig
{
    public string SessionMetadataTable { get; set; }

    public string VotingProfilesTable { get; set; }

    public string S3Bucket { get; set; }

    public bool IsDev { get; set; }
}
