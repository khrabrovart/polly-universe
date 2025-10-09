using Microsoft.Extensions.Configuration;
using PollyUniverse.Shared.Extensions;

namespace PollyUniverse.Func.Agent;

public class FunctionConfig
{
    public FunctionConfig(IConfigurationRoot config)
    {
        ScheduleNamePrefix = config.GetOrThrow("SCHEDULE_NAME_PREFIX");
        SchedulerGroupName = config.GetOrThrow("SCHEDULER_GROUP_NAME");
        TargetLambdaArn = config.GetOrThrow("TARGET_LAMBDA_ARN");
        ScheduleExecutionRoleArn = config.GetOrThrow("SCHEDULE_EXECUTION_ROLE_ARN");

        IsDev = bool.TryParse(config["DEV:ENABLED"], out var isDev) && isDev;
    }

    public string ScheduleNamePrefix { get; set; }

    public string SchedulerGroupName { get; set; }

    public string TargetLambdaArn { get; set; }

    public string ScheduleExecutionRoleArn { get; set; }

    #region Development Settings

    public bool IsDev { get; set; }

    #endregion
}
