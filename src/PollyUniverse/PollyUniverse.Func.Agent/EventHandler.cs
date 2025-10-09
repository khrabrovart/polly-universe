using Microsoft.Extensions.Logging;

namespace PollyUniverse.Func.Agent;

public interface IEventHandler
{
    Task Handle(DynamoDBEvent dynamoEvent);
}

public class EventHandler : IEventHandler
{
    private readonly ILogger<EventHandler> _logger;

    public EventHandler(
        ILogger<EventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(DynamoDBEvent dynamoEvent)
    {
        foreach (var record in dynamoEvent.Records)
        {
            await ProcessRecord(record);
        }
    }
}
