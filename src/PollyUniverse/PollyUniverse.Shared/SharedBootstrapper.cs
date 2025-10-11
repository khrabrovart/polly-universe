using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PollyUniverse.Shared.Extensions;

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

        services.AddSharedServices(configuration);

        configure(services, configuration);

        return services.BuildServiceProvider();
    }
}
