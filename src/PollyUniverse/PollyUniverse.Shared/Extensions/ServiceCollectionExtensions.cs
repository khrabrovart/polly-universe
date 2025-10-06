using Amazon.DynamoDBv2;
using Amazon.S3;
using Microsoft.Extensions.DependencyInjection;
using PollyUniverse.Shared.AWS;

namespace PollyUniverse.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddSharedServices(this IServiceCollection services)
    {
        services
            .AddAWSService<IAmazonDynamoDB>()
            .AddAWSService<IAmazonS3>()

            .AddTransient<IS3Client, S3Client>()
            .AddTransient<IDynamoDbClient, DynamoDbClient>()
            ;
    }
}
