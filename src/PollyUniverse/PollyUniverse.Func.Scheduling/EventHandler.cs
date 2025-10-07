using Amazon.Lambda.DynamoDBEvents;
using Microsoft.Extensions.Logging;
using PollyUniverse.Func.Scheduling.Models;
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
            await ProcessDynamoDBRecord(record);
        }

        _logger.LogInformation("DynamoDB stream event handled successfully");
    }

    private async Task ProcessDynamoDBRecord(DynamoDBEvent.DynamodbStreamRecord record)
    {
        try
        {
            _logger.LogInformation("Processing DynamoDB stream record from source: {EventSourceArn}", record.EventSourceArn);

            // Log the record details for debugging
            _logger.LogInformation("Record details: {Record}", JsonSerializer.Serialize(record));

            // For now, just demonstrate that we can receive the events

            await SchedulePoll();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing DynamoDB record");
            throw;
        }
    }

    private async Task SchedulePoll()
    {
        // This could involve:
        // 1. Creating EventBridge rules for scheduled execution
        // 2. Storing schedule information in DynamoDB
        // 3. Calculating next execution time based on DayOfWeek and UtcTime

        _logger.LogInformation("Processing DynamoDB stream event for poll scheduling");
        await Task.CompletedTask;
    }
}
