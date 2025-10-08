using PollyUniverse.Func.Voting.Models;
using PollyUniverse.Shared.Models;
using TL;
using WTelegram;

namespace PollyUniverse.Func.Voting.Services;

public interface IPollService
{
    Task<PollMessage> WaitForPollMessage(
        Client telegramClient,
        VotingProfilePoll pollDescriptor,
        TimeSpan timeout);
}

public class PollService : IPollService
{
    public async Task<PollMessage> WaitForPollMessage(
        Client telegramClient,
        VotingProfilePoll pollDescriptor,
        TimeSpan timeout)
    {
        var tcs = new TaskCompletionSource<PollMessage>();

        var handler = (UpdatesBase updatesBase) =>
        {
            FindPollMessage(pollDescriptor, updatesBase, tcs);
            return Task.CompletedTask;
        };

        telegramClient.OnUpdates += handler;

        var timeoutTask = Task.Delay(timeout);
        var pollTask = tcs.Task;
        var completedTask = await Task.WhenAny(pollTask, timeoutTask);

        telegramClient.OnUpdates -= handler;

        return completedTask == pollTask ? pollTask.Result : null;
    }

    private static void FindPollMessage(
        VotingProfilePoll pollDescriptor,
        UpdatesBase updatesBase,
        TaskCompletionSource<PollMessage> tcs)
    {
        foreach (var update in updatesBase.UpdateList)
        {
            if (update is not UpdateNewChannelMessage { message: Message { media: MessageMediaPoll poll } message })
            {
                continue;
            }

            var messageUtcDateTime = message.Date;
            var messageFromId = message.From.ID;
            var messagePeerId = message.Peer.ID;

            var pollTimezone = TimeZoneInfo.FindSystemTimeZoneById(pollDescriptor.Timezone);
            var timezoneCurrentTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, pollTimezone);

            var minPollTimezoneDateTime = timezoneCurrentTime.Date.Add(pollDescriptor.Time);
            var minPollUtcDateTime = TimeZoneInfo.ConvertTimeToUtc(minPollTimezoneDateTime);

            if (messageUtcDateTime < minPollUtcDateTime || messageFromId != pollDescriptor.FromId || messagePeerId != pollDescriptor.PeerId)
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
