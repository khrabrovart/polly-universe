namespace PollyUniverse.Func.Voting.Models;

public record PollMessage
{
    public required int MessageId { get; init; }

    public required byte[][] Options { get; init; }
}
