using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Microsoft.Extensions.Logging;
using PollyUniverse.Func.Agent.Models;
using PollyUniverse.Func.Agent.Services;
using PollyUniverse.Shared.Services;

namespace PollyUniverse.Func.Agent;

public interface IEventHandler
{
    Task Handle(APIGatewayProxyRequest request);
}

public class EventHandler : IEventHandler
{
    private readonly IMessageService _messageService;
    private readonly IDictionaryService _dictionaryService;
    private readonly ILogger<EventHandler> _logger;

    public EventHandler(
        IMessageService messageService,
        IDictionaryService dictionaryService,
        ILogger<EventHandler> logger)
    {
        _messageService = messageService;
        _dictionaryService = dictionaryService;
        _logger = logger;
    }

    public async Task Handle(APIGatewayProxyRequest request)
    {
        var agentRequest = JsonSerializer.Deserialize<AgentRequest>(request.Body);

        _logger.LogInformation("Received message: {Message}", JsonSerializer.Serialize(agentRequest.Message));

        await _dictionaryService.PreloadDictionary();

        await _messageService.Reply(agentRequest.Message);

        _logger.LogInformation("Message processed successfully");
    }
}
