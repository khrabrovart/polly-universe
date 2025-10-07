using Amazon.Lambda.DynamoDBEvents;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace PollyUniverse.Func.Scheduling;

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
        _logger.LogInformation("Handling DynamoDB stream event with {RecordCount} records", dynamoEvent.Records.Count);

        foreach (var record in dynamoEvent.Records)
        {
            await ProcessRecord(record);
        }

        _logger.LogInformation("DynamoDB stream event handled successfully");
    }

    private async Task ProcessRecord(DynamoDBEvent.DynamodbStreamRecord record)
    {
        _logger.LogInformation("Processing record: {Record}", JsonSerializer.Serialize(record));
    }
}
