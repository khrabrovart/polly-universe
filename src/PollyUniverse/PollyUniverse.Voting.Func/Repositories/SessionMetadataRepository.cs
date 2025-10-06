using Amazon.DynamoDBv2.Model;
using PollyUniverse.Shared.Aws.Services;
using PollyUniverse.Voting.Func.Models;

namespace PollyUniverse.Voting.Func.Repositories;

public interface ISessionMetadataRepository
{
    Task<SessionMetadata> Get(string clientId);
}

public class SessionMetadataRepository : ISessionMetadataRepository
{
    private readonly IDynamoDbService _dynamoDbService;
    private readonly FunctionConfig _config;

    public SessionMetadataRepository(
        IDynamoDbService dynamoDbService,
        FunctionConfig config)
    {
        _dynamoDbService = dynamoDbService;
        _config = config;
    }

    public async Task<SessionMetadata> Get(string clientId)
    {
        var key = new Dictionary<string, AttributeValue>
        {
            { "Id", new AttributeValue { S = clientId } }
        };

        var item = await _dynamoDbService.Get(_config.SessionMetadataTable, key);

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
