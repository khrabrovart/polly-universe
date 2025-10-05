using Amazon.DynamoDBv2.Model;
using PollyUniverse.Shared.AWS;
using PollyUniverse.Voting.Func.Models;

namespace PollyUniverse.Voting.Func.Repositories;

public interface ISessionMetadataRepository
{
    Task<SessionMetadata> Get(string clientId);
}

public class SessionMetadataRepository : ISessionMetadataRepository
{
    private readonly IDynamoDbClient _dynamoDbClient;
    private readonly FunctionConfig _config;

    public SessionMetadataRepository(
        IDynamoDbClient dynamoDbClient,
        FunctionConfig config)
    {
        _dynamoDbClient = dynamoDbClient;
        _config = config;
    }

    public async Task<SessionMetadata> Get(string clientId)
    {
        var key = new Dictionary<string, AttributeValue>
        {
            { "Id", new AttributeValue { S = clientId } }
        };

        var item = await _dynamoDbClient.Get(_config.SessionMetadataTable, key);

        return item == null
            ? null
            : new SessionMetadata
            {
                Id = item["Id"].S,
                ApiHash = item["ApiHash"].S,
                PhoneNumber = item["PhoneNumber"].S
            };
    }
}
