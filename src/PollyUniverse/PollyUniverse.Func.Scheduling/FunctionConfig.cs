using Microsoft.Extensions.Configuration;

namespace PollyUniverse.Func.Scheduling;

public class FunctionConfig
{
    public FunctionConfig(IConfigurationRoot config)
    {
        IsDev = bool.TryParse(config["DEV:ENABLED"], out var isDev) && isDev;
    }


    #region Development Settings

    public bool IsDev { get; set; }

    #endregion
}
