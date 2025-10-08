using Amazon.Lambda.DynamoDBEvents;
using Microsoft.Extensions.Logging;
using PollyUniverse.Func.Scheduling.Comparers;
using PollyUniverse.Func.Scheduling.Services;
using PollyUniverse.Shared.Models;

namespace PollyUniverse.Func.Scheduling;

public interface IEventHandler
{
    Task Handle(DynamoDBEvent dynamoEvent);
}

public class EventHandler : IEventHandler
{
    private readonly IVotingProfileComparer _votingProfileComparer;
    private readonly IVotingScheduleService _votingScheduleService;
    private readonly ILogger<EventHandler> _logger;

    public EventHandler(
        IVotingProfileComparer votingProfileComparer,
        IVotingScheduleService votingScheduleService,
        ILogger<EventHandler> logger)
    {
        _votingProfileComparer = votingProfileComparer;
        _votingScheduleService = votingScheduleService;
        _logger = logger;
    }

    public async Task Handle(DynamoDBEvent dynamoEvent)
    {
        foreach (var record in dynamoEvent.Records)
        {
            await ProcessRecord(record);
        }
    }

    private async Task ProcessRecord(DynamoDBEvent.DynamodbStreamRecord record)
    {
        var oldProfile = ToVotingProfile(record.Dynamodb.OldImage);
        var newProfile = ToVotingProfile(record.Dynamodb.NewImage);

        switch (record.EventName)
        {
            case "INSERT":
                await OnInsert(newProfile);
                break;
            case "MODIFY":
                await OnModify(oldProfile, newProfile);
                break;
            case "REMOVE":
                await OnRemove(oldProfile);
                break;
            default:
                throw new InvalidOperationException($"Unknown event name: {record.EventName}");
        }
    }

    private static VotingProfile ToVotingProfile(Dictionary<string, DynamoDBEvent.AttributeValue> record)
    {
        if (record == null)
        {
            return null;
        }

        return new VotingProfile
        {
            Id = record["Id"].S,
            Enabled = record["Enabled"].BOOL ?? false,
            Poll = new VotingProfilePoll
            {
                FromId = long.Parse(record["Poll"].M["FromId"].N),
                PeerId = long.Parse(record["Poll"].M["PeerId"].N),
                DayOfWeek = Enum.Parse<DayOfWeek>(record["Poll"].M["DayOfWeek"].S),
                Time = TimeSpan.Parse(record["Poll"].M["Time"].S),
                Timezone = record["Poll"].M["Timezone"].S
            },
            Session = new VotingProfileSession
            {
                Id = record["Session"].M["Id"].S,
                Enabled = record["Session"].M["Enabled"].BOOL ?? false,
                VoteIndex = int.Parse(record["Session"].M["VoteIndex"].N)
            }
        };
    }

    private async Task OnInsert(VotingProfile newProfile)
    {
        _logger.LogInformation("Processing INSERT event for VotingProfile: {ProfileId}", newProfile?.Id);

        await _votingScheduleService.UpdateSchedule(newProfile);

        _logger.LogInformation("Schedule created");
    }

    private async Task OnModify(VotingProfile oldProfile, VotingProfile newProfile)
    {
        _logger.LogInformation("Processing MODIFY event for VotingProfile: {ProfileId}", newProfile?.Id);

        if (_votingProfileComparer.Compare(oldProfile, newProfile))
        {
            _logger.LogInformation("No changes detected between old and new profiles. Skipping update");
            return;
        }

        await _votingScheduleService.UpdateSchedule(newProfile);

        _logger.LogInformation("Schedule updated");
    }

    private async Task OnRemove(VotingProfile oldProfile)
    {
        _logger.LogInformation("Processing REMOVE event for VotingProfile: {ProfileId}", oldProfile?.Id);

        await _votingScheduleService.DeleteSchedule(oldProfile);

        _logger.LogInformation("Schedule removed");
    }
}
