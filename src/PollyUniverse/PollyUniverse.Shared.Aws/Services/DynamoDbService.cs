using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace PollyUniverse.Shared.Aws.Services;

public interface IDynamoDbService
{
    Task<Dictionary<string, AttributeValue>[]> Scan(string tableName);

    Task<Dictionary<string, AttributeValue>> Get(string tableName, Dictionary<string, AttributeValue> key);

    Task<Dictionary<string, AttributeValue>[]> Get(string tableName, Dictionary<string, AttributeValue>[] keys);

    Task Put(string tableName, Dictionary<string, AttributeValue> item);
}

public class DynamoDbService : IDynamoDbService
{
    private readonly IAmazonDynamoDB _dynamoDbClient;

    public DynamoDbService(IAmazonDynamoDB dynamoDbClient)
    {
        _dynamoDbClient = dynamoDbClient;
    }

    public async Task<Dictionary<string, AttributeValue>[]> Scan(string tableName)
    {
        var items = new List<Dictionary<string, AttributeValue>>();

        Dictionary<string, AttributeValue> lastEvaluatedKey;

        do
        {
            var response = await _dynamoDbClient.ScanAsync(new ScanRequest(tableName));

            items.AddRange(response.Items);

            lastEvaluatedKey = response.LastEvaluatedKey;
        } while (lastEvaluatedKey != null);

        return items.ToArray();
    }

    public async Task<Dictionary<string, AttributeValue>> Get(string tableName, Dictionary<string, AttributeValue> key)
    {
        var response = await _dynamoDbClient.GetItemAsync(tableName, key);
        return !response.IsItemSet ? null : response.Item;
    }

    public async Task<Dictionary<string, AttributeValue>[]> Get(string tableName, Dictionary<string, AttributeValue>[] keys)
    {
        var requestItems = new Dictionary<string, KeysAndAttributes>
        {
            { tableName, new KeysAndAttributes { Keys = keys.ToList() } }
        };

        var response = await _dynamoDbClient.BatchGetItemAsync(new BatchGetItemRequest
        {
            RequestItems = requestItems
        });

        return response.Responses[tableName].ToArray();
    }

    public async Task Put(string tableName, Dictionary<string, AttributeValue> item)
    {
        await _dynamoDbClient.PutItemAsync(tableName, item);
    }
}
