namespace PollyUniverse.Voting.Func.Models;

public class VotingProfile
{
    public string Id { get; set; }

    public VotingProfileSession Session { get; set; }

    public VotingProfilePoll Poll { get; set; }
}

public class VotingProfilePoll
{
    public long FromId { get; set; }

    public long PeerId { get; set; }

    public string DayOfWeek { get; set; }

    public TimeSpan UtcTime { get; set; }
}

public class VotingProfileSession
{
    public string Id { get; set; }

    public int VoteIndex { get; set; }
}
