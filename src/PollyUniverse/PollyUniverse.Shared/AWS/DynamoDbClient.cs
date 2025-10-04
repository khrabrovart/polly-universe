using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace PollyUniverse.Shared.AWS;

public interface IDynamoDbClient
{
    Task<Dictionary<string, AttributeValue>> Get(string tableName, Dictionary<string, AttributeValue> key);
}

public class DynamoDbClient : IDynamoDbClient
{
    private readonly IAmazonDynamoDB _dynamoDbClient;

    public DynamoDbClient(IAmazonDynamoDB dynamoDbClient)
    {
        _dynamoDbClient = dynamoDbClient;
    }

    public async Task<Dictionary<string, AttributeValue>> Get(string tableName, Dictionary<string, AttributeValue> key)
    {
        var response = await _dynamoDbClient.GetItemAsync(tableName, key);

        return !response.IsItemSet ? null : response.Item;
    }
}
