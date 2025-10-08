using System.Text.Json;
using PollyUniverse.Shared.Aws.Services;
using PollyUniverse.Shared.Models;

namespace PollyUniverse.Func.Scheduling.Services;

public interface IVotingScheduleService
{
    Task UpdateSchedule(VotingProfile votingProfile);

    Task DeleteSchedule(VotingProfile votingProfile);
}

public class VotingScheduleService : IVotingScheduleService
{
    private readonly ISchedulerService _schedulerService;
    private readonly FunctionConfig _config;

    public VotingScheduleService(
        ISchedulerService schedulerService,
        FunctionConfig config)
    {
        _schedulerService = schedulerService;
        _config = config;
    }

    public async Task UpdateSchedule(VotingProfile votingProfile)
    {
        var scheduleName = GetScheduleName(votingProfile);
        var existingSchedule = await _schedulerService.GetSchedule(scheduleName, _config.SchedulerGroupName);

        if (existingSchedule != null)
        {
            var deleted = await _schedulerService.DeleteSchedule(scheduleName, _config.SchedulerGroupName);

            if (!deleted)
            {
                throw new Exception($"Failed to delete schedule {scheduleName}");
            }
        }

        var cronExpression = ComposeCronExpression(
            votingProfile.Poll.DayOfWeek,
            votingProfile.Poll.Time,
            launchToleranceMinutes: 1);

        var payload = JsonSerializer.Serialize(new { VotingProfileId = votingProfile.Id });

        var created = await _schedulerService.CreateSchedule(
            scheduleName: scheduleName,
            groupName: _config.SchedulerGroupName,
            scheduleExpression: cronExpression,
            timezone: votingProfile.Poll.Timezone,
            isEnabled: votingProfile.Enabled,
            targetLambdaArn: _config.TargetLambdaArn,
            executionRoleArn: _config.ScheduleExecutionRoleArn,
            payload: payload);

        if (!created)
        {
            throw new Exception($"Failed to create schedule {scheduleName}");
        }
    }

    public async Task DeleteSchedule(VotingProfile votingProfile)
    {
        var scheduleName = GetScheduleName(votingProfile);
        var existingSchedule = await _schedulerService.GetSchedule(scheduleName, _config.SchedulerGroupName);

        if (existingSchedule == null)
        {
            return;
        }

        var deleted = await _schedulerService.DeleteSchedule(scheduleName, _config.SchedulerGroupName);

        if (!deleted)
        {
            throw new Exception($"Failed to delete schedule {scheduleName}");
        }
    }

    private string GetScheduleName(VotingProfile votingProfile)
    {
        return $"{_config.ScheduleNamePrefix}-{votingProfile.Id}";
    }

    private static string ComposeCronExpression(string dayOfWeek, TimeSpan time, int launchToleranceMinutes)
    {
        // AWS EventBridge launches cron jobs with a delay
        // We subtract some tolerance from the scheduled time to ensure it is already running at the expected time
        var correctedTime = time.Add(-TimeSpan.FromMinutes(launchToleranceMinutes));

        var hours = correctedTime.Hours;
        var minutes = correctedTime.Minutes;

        const string dayOfMonth = "?";
        const string month = "*";
        const string year = "*";

        return $"cron({minutes} {hours} {dayOfMonth} {month} {dayOfWeek} {year})";
    }
}
