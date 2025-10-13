using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PollyUniverse.Shared.Aws.Extensions;
using PollyUniverse.Shared.OpenAI.Extensions;
using PollyUniverse.Shared.Repositories;
using PollyUniverse.Shared.Services;
using PollyUniverse.Shared.Services.Files;
using PollyUniverse.Shared.Telegram.Extensions;

namespace PollyUniverse.Shared;

public static class SharedBootstrapper
{
    public static ServiceProvider Bootstrap(Action<IServiceCollection, IConfiguration> configure)
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
            builder.AddFilter("Amazon", LogLevel.Warning);
            builder.AddFilter("AWSSDK", LogLevel.Warning);
        });

        services.AddSingleton<ISharedConfig>(new SharedConfig(configuration));

        services
            .AddAwsServices()
            .AddOpenAIServices()
            .AddTelegramServices();

        services
            .AddSingleton<ISessionMetadataRepository, SessionMetadataRepository>()
            .AddSingleton<IUserRepository, UserRepository>()
            .AddSingleton<IVotingProfileRepository, VotingProfileRepository>()

            .AddSingleton<IMessageHistoryFileService, MessageHistoryFileService>()
            .AddSingleton<IPromptFileService, PromptFileService>()
            .AddSingleton<ISessionFileService, SessionFileService>()
            .AddSingleton<IDictionaryFileService, DictionaryFileService>()

            .AddSingleton<IMessageHistoryService, MessageHistoryService>()
            .AddSingleton<IPromptService, PromptService>()
            .AddSingleton<IDictionaryService, DictionaryService>()
            ;

        configure(services, configuration);

        return services.BuildServiceProvider();
    }
}
