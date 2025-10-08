using Microsoft.Extensions.Logging;
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
    private readonly ILogger<PollService> _logger;

    public PollService(ILogger<PollService> logger)
    {
        _logger = logger;
    }

    public async Task<PollMessage> WaitForPollMessage(
        Client telegramClient,
        VotingProfilePoll pollDescriptor,
        TimeSpan timeout)
    {
        var tcs = new TaskCompletionSource<PollMessage>();
        var pollUtcDateTime = GetPollUtcDateTime(pollDescriptor);

        var handler = (UpdatesBase updatesBase) =>
        {
            FindPollMessage(pollDescriptor, pollUtcDateTime, updatesBase, tcs);
            return Task.CompletedTask;
        };

        telegramClient.OnUpdates += handler;

        var timeoutTask = Task.Delay(timeout);
        var pollTask = tcs.Task;
        var completedTask = await Task.WhenAny(pollTask, timeoutTask);

        telegramClient.OnUpdates -= handler;

        return completedTask == pollTask ? pollTask.Result : null;
    }

    private void FindPollMessage(
        VotingProfilePoll pollDescriptor,
        DateTime pollUtcDateTime,
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

            _logger.LogInformation("Poll|Criteria - FromId: {PollFromId}|{CriteriaFromId}, PeerId: {PollPeerId}|{CriteriaPeerId}, Time: {PollUtcDateTime}|{CriteriaUtcDateTime}",
                messageFromId, pollDescriptor.FromId,
                messagePeerId, pollDescriptor.PeerId,
                messageUtcDateTime, pollUtcDateTime);

            if (messageUtcDateTime < pollUtcDateTime || messageFromId != pollDescriptor.FromId || messagePeerId != pollDescriptor.PeerId)
            {
                _logger.LogInformation("Skipping poll message because it does not match the criteria");
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

    private static DateTime GetPollUtcDateTime(VotingProfilePoll pollDescriptor)
    {
        var pollTimezone = TimeZoneInfo.FindSystemTimeZoneById(pollDescriptor.Timezone);
        var timezoneCurrentTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, pollTimezone);

        var pollTimezoneDateTime = timezoneCurrentTime.Date.Add(pollDescriptor.Time);
        var pollUtcDateTime = TimeZoneInfo.ConvertTimeToUtc(pollTimezoneDateTime, pollTimezone);

        return pollUtcDateTime;
    }
}
