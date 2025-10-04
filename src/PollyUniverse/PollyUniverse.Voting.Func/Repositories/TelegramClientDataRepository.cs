using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Options;
using PollyUniverse.Shared.AWS;
using PollyUniverse.Voting.Func.Models;

namespace PollyUniverse.Voting.Func.Repositories;

public interface ITelegramClientDataRepository
{
    Task<TelegramClientData> Get(string clientId);
}

public class TelegramClientDataRepository : ITelegramClientDataRepository
{
    private readonly IDynamoDbClient _dynamoDbClient;
    private readonly FunctionConfig _config;

    public TelegramClientDataRepository(
        IDynamoDbClient dynamoDbClient,
        IOptions<FunctionConfig> config)
    {
        _dynamoDbClient = dynamoDbClient;
        _config = config.Value;
    }

    public async Task<TelegramClientData> Get(string clientId)
    {
        var key = new Dictionary<string, AttributeValue>
        {
            { "Id", new AttributeValue { S = clientId } }
        };

        var item = await _dynamoDbClient.Get(_config.TelegramClientDataTable, key);

        return item == null
            ? null
            : new TelegramClientData
            {
                Id = item["Id"].S,
                ApiId = int.Parse(item["ApiId"].N),
                ApiHash = item["ApiHash"].S,
                PhoneNumber = item["PhoneNumber"].S
            };
    }
}
