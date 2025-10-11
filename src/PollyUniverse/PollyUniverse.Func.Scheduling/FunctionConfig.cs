using Microsoft.Extensions.Configuration;
using PollyUniverse.Shared.Utils.Extensions;

namespace PollyUniverse.Func.Scheduling;

public interface IFunctionConfig
{
    string ScheduleNamePrefix { get; set; }

    string SchedulerGroupName { get; set; }

    string TargetLambdaArn { get; set; }

    string ScheduleExecutionRoleArn { get; set; }
}

public class FunctionConfig : IFunctionConfig
{
    public FunctionConfig(IConfiguration configuration)
    {
        ScheduleNamePrefix = configuration.GetOrThrow("SCHEDULE_NAME_PREFIX");
        SchedulerGroupName = configuration.GetOrThrow("SCHEDULER_GROUP_NAME");
        TargetLambdaArn = configuration.GetOrThrow("TARGET_LAMBDA_ARN");
        ScheduleExecutionRoleArn = configuration.GetOrThrow("SCHEDULE_EXECUTION_ROLE_ARN");
    }

    public string ScheduleNamePrefix { get; set; }

    public string SchedulerGroupName { get; set; }

    public string TargetLambdaArn { get; set; }

    public string ScheduleExecutionRoleArn { get; set; }
}
