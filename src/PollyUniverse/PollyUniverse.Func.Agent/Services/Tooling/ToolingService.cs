using PollyUniverse.Shared.OpenAI.Models;

namespace PollyUniverse.Func.Agent.Services.Tooling;

public interface IToolingService
{
    IEnumerable<OpenAITool> GetTools();
}
