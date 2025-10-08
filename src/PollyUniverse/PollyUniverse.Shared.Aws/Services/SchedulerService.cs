using System.Net;
using Amazon.Scheduler;
using Amazon.Scheduler.Model;

namespace PollyUniverse.Shared.Aws.Services;

public interface ISchedulerService
{
    Task<bool> CreateSchedule(
        string scheduleName,
        string groupName,
        string scheduleExpression,
        string timezone,
        bool isEnabled,
        string targetLambdaArn,
        string executionRoleArn,
        string payload);

    Task<bool> DeleteSchedule(string scheduleName, string groupName);

    Task<GetScheduleResponse> GetSchedule(string scheduleName, string groupName);
}

public class SchedulerService : ISchedulerService
{
    private readonly IAmazonScheduler _scheduler;

    public SchedulerService(IAmazonScheduler scheduler)
    {
        _scheduler = scheduler;
    }

    public async Task<bool> CreateSchedule(
        string scheduleName,
        string groupName,
        string scheduleExpression,
        string timezone,
        bool isEnabled,
        string targetLambdaArn,
        string executionRoleArn,
        string payload)
    {
        var request = new CreateScheduleRequest
        {
            Name = scheduleName,
            GroupName = groupName,
            ScheduleExpression = scheduleExpression,
            ScheduleExpressionTimezone = timezone,
            State = isEnabled ? ScheduleState.ENABLED : ScheduleState.DISABLED,
            ActionAfterCompletion = ActionAfterCompletion.NONE,
            FlexibleTimeWindow = new FlexibleTimeWindow
            {
                Mode = FlexibleTimeWindowMode.OFF
            },
            Target = new Target
            {
                Arn = targetLambdaArn,
                RoleArn = executionRoleArn,
                Input = payload
            }
        };

        var response = await _scheduler.CreateScheduleAsync(request);
        return response.HttpStatusCode == HttpStatusCode.OK;
    }

    public async Task<bool> DeleteSchedule(string scheduleName, string groupName)
    {
        var request = new DeleteScheduleRequest
        {
            Name = scheduleName,
            GroupName = groupName
        };

        var response = await _scheduler.DeleteScheduleAsync(request);
        return response.HttpStatusCode == HttpStatusCode.OK;
    }

    public async Task<GetScheduleResponse> GetSchedule(string scheduleName, string groupName)
    {
        var request = new GetScheduleRequest
        {
            Name = scheduleName,
            GroupName = groupName
        };

        try
        {
            return await _scheduler.GetScheduleAsync(request);
        }
        catch (ResourceNotFoundException)
        {
            return null;
        }
    }
}
