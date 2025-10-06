using PollyUniverse.Voting.Func.Models;
using TL;
using WTelegram;

namespace PollyUniverse.Voting.Func.Services.Telegram;

public interface ITelegramPollService
{
    Task<PollMessage> WaitForPollMessage(Client telegramClient, VotingProfilePoll pollDescriptor);
}

public class TelegramPollService : ITelegramPollService
{
    private readonly FunctionConfig _config;

    public TelegramPollService(FunctionConfig config)
    {
        _config = config;
    }

    public async Task<PollMessage> WaitForPollMessage(Client telegramClient, VotingProfilePoll pollDescriptor)
    {
        var tcs = new TaskCompletionSource<PollMessage>();

        var handler = (UpdatesBase updatesBase) =>
        {
            FindPollMessage(pollDescriptor, updatesBase, tcs);
            return Task.CompletedTask;
        };

        telegramClient.OnUpdates += handler;

        var timeoutTask = Task.Delay(TimeSpan.FromMinutes(_config.PollWaitingMinutes));
        var pollTask = tcs.Task;
        var completedTask = await Task.WhenAny(pollTask, timeoutTask);

        telegramClient.OnUpdates -= handler;

        return completedTask == pollTask ? pollTask.Result : null;
    }

    private static void FindPollMessage(VotingProfilePoll pollDescriptor, UpdatesBase updatesBase, TaskCompletionSource<PollMessage> tcs)
    {
        foreach (var update in updatesBase.UpdateList)
        {
            if (update is not UpdateNewChannelMessage { message: Message { media: MessageMediaPoll poll } message })
            {
                continue;
            }

            var messageDate = message.Date;
            var messageFromId = message.From.ID;
            var messagePeerId = message.Peer.ID;

            var expectedDate = DateTime.UtcNow.Date + pollDescriptor.UtcTime;

            if (messageDate < expectedDate || messageFromId != pollDescriptor.FromId || messagePeerId != pollDescriptor.PeerId)
            {
                continue;
            }

            var pollMessage = new PollMessage
            {
                MessageId = message.id,
                Options = poll.poll.answers.Select(a => a.option).ToArray()
            };

            tcs.TrySetResult(pollMessage);
        }
    }
}
