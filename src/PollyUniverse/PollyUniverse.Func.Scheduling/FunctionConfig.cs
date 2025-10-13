using Microsoft.Extensions.Configuration;
using PollyUniverse.Shared.Utils.Extensions;

namespace PollyUniverse.Func.Scheduling;

public interface IFunctionConfig
{
    string ScheduleNamePrefix { get; }

    string SchedulerGroupName { get; }

    string TargetLambdaArn { get; }

    string ScheduleExecutionRoleArn { get; }
}

public record FunctionConfig : IFunctionConfig
{
    public FunctionConfig(IConfiguration configuration)
    {
        ScheduleNamePrefix = configuration.GetOrThrow("SCHEDULE_NAME_PREFIX");
        SchedulerGroupName = configuration.GetOrThrow("SCHEDULER_GROUP_NAME");
        TargetLambdaArn = configuration.GetOrThrow("TARGET_LAMBDA_ARN");
        ScheduleExecutionRoleArn = configuration.GetOrThrow("SCHEDULE_EXECUTION_ROLE_ARN");
    }

    public string ScheduleNamePrefix { get; }

    public string SchedulerGroupName { get; }

    public string TargetLambdaArn { get; }

    public string ScheduleExecutionRoleArn { get; }
}
