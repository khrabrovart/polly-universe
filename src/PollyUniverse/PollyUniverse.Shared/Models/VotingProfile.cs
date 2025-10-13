using PollyUniverse.Shared.Telegram.Models;

namespace PollyUniverse.Shared.Models;

public class VotingProfile
{
    public required string Id { get; set; }

    public required bool Enabled { get; set; }

    public required string Description { get; set; }

    public required VotingProfilePoll Poll { get; set; }

    public required List<VotingProfileUser> Users { get; set; }
}

public class VotingProfilePoll
{
    public required long FromId { get; set; }

    public required TelegramShortPeerId PeerId { get; set; }

    public required DayOfWeek DayOfWeek { get; set; }

    public required TimeSpan Time { get; set; }

    public required string Timezone { get; set; }
}

public class VotingProfileUser
{
    public required string Id { get; set; }

    public required bool Enabled { get; set; }

    public required int VoteIndex { get; set; }

    public required int VoteDelaySeconds { get; set; }
}
