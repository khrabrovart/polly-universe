using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.Logging;
using PollyUniverse.Func.Agent.Models;

namespace PollyUniverse.Func.Agent;

public interface IEventHandler
{
    Task Handle(APIGatewayProxyRequest request);
}

public class EventHandler : IEventHandler
{
    private readonly ILogger<EventHandler> _logger;

    public EventHandler(
        ILogger<EventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(APIGatewayProxyRequest request)
    {
        var agentRequest = JsonSerializer.Deserialize<AgentRequest>(request.Body);

        _logger.LogInformation("Received message: {Text}", agentRequest?.Message?.Text);
    }
}
