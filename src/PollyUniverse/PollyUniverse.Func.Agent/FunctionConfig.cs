using Microsoft.Extensions.Configuration;

namespace PollyUniverse.Func.Agent;

public interface IFunctionConfig
{
}

public class FunctionConfig : IFunctionConfig
{
    public FunctionConfig(IConfiguration configuration)
    {
    }
}
