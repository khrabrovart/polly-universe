using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace PollyUniverse.Shared.Aws.Services;

public interface IDynamoDbService
{
    Task<Dictionary<string, AttributeValue>> Get(string tableName, Dictionary<string, AttributeValue> key);
}

public class DynamoDbService : IDynamoDbService
{
    private readonly IAmazonDynamoDB _dynamoDbClient;

    public DynamoDbService(IAmazonDynamoDB dynamoDbClient)
    {
        _dynamoDbClient = dynamoDbClient;
    }

    public async Task<Dictionary<string, AttributeValue>> Get(string tableName, Dictionary<string, AttributeValue> key)
    {
        var response = await _dynamoDbClient.GetItemAsync(tableName, key);

        return !response.IsItemSet ? null : response.Item;
    }
}
