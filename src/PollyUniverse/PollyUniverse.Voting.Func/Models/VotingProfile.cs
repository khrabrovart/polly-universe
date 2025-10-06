namespace PollyUniverse.Voting.Func.Models;

public class VotingProfile
{
    public string Id { get; set; }

    public string SessionId { get; set; }

    public VotingProfilePoll Poll { get; set; }

    public VotingProfileVote Vote { get; set; }
}

public class VotingProfilePoll
{
    public long FromId { get; set; }

    public long PeerId { get; set; }

    public TimeSpan UtcTime { get; set; }
}

public class VotingProfileVote
{
    public int Index { get; set; }
}
