namespace PollyUniverse.Func.Agent.Models;

public class AgentRequest
{
    public AgentRequestMessage Message { get; set; }
}

public class AgentRequestMessage
{
    public string Text { get; set; }
}
