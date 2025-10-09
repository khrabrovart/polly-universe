using Microsoft.Extensions.Configuration;
using PollyUniverse.Shared.Extensions;

namespace PollyUniverse.Func.Agent;

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
