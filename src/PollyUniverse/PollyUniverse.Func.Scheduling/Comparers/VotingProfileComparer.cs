using PollyUniverse.Shared.Models;

namespace PollyUniverse.Func.Scheduling.Comparers;

public interface IVotingProfileComparer
{
    bool Compare(VotingProfile votingProfile1, VotingProfile votingProfile2);
}

public class VotingProfileComparer : IVotingProfileComparer
{
    public bool Compare(VotingProfile votingProfile1, VotingProfile votingProfile2)
    {
        // Only compare relevant fields for scheduling, like enabled status and poll time settings

        if (votingProfile1.Enabled != votingProfile2.Enabled) return false;

        if (votingProfile1.Poll.DayOfWeek != votingProfile2.Poll.DayOfWeek) return false;
        if (votingProfile1.Poll.Time != votingProfile2.Poll.Time) return false;
        if (votingProfile1.Poll.Timezone != votingProfile2.Poll.Timezone) return false;

        return true;
    }
}
