using PollyUniverse.Voting.Func.Models;
using TL;
using WTelegram;

namespace PollyUniverse.Voting.Func.Services.Telegram;

public interface ITelegramVoteService
{
    Task<bool> Vote(
        Client telegramClient,
        InputPeer inputPeer,
        PollMessage pollMessage,
        VotingProfileVote voteDescriptor);
}

public class TelegramVoteService : ITelegramVoteService
{
    public async Task<bool> Vote(
        Client telegramClient,
        InputPeer inputPeer,
        PollMessage pollMessage,
        VotingProfileVote voteDescriptor)
    {
        var update = await telegramClient.Messages_SendVote(inputPeer, pollMessage.MessageId, pollMessage.Options[voteDescriptor.Index]);
        return update.UpdateList[0] is UpdateMessagePoll;
    }
}
