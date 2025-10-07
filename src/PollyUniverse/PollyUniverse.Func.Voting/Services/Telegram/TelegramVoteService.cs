using PollyUniverse.Func.Voting.Models;
using TL;
using WTelegram;

namespace PollyUniverse.Func.Voting.Services.Telegram;

public interface ITelegramVoteService
{
    Task<bool> Vote(
        Client telegramClient,
        InputPeer inputPeer,
        PollMessage pollMessage,
        int voteIndex);
}

public class TelegramVoteService : ITelegramVoteService
{
    public async Task<bool> Vote(
        Client telegramClient,
        InputPeer inputPeer,
        PollMessage pollMessage,
        int voteIndex)
    {
        var updates = await telegramClient.Messages_SendVote(
            inputPeer,
            pollMessage.MessageId,
            pollMessage.Options[voteIndex]);

        return updates.UpdateList[0] is UpdateMessagePoll;
    }
}
