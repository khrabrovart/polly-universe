using System.Text.Json;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PollyUniverse.Func.Voting.Models;
using PollyUniverse.Func.Voting.Services;
using PollyUniverse.Shared;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace PollyUniverse.Func.Voting;

public class Function
{
    private static readonly ServiceProvider ServiceProvider;

    static Function()
    {
        ServiceProvider = SharedBootstrapper.Bootstrap((services, configuration) =>
        {
            services
                .AddSingleton<IFunctionConfig>(new FunctionConfig(configuration))

                .AddSingleton<IEventHandler, EventHandler>()

                .AddSingleton<INotificationService, NotificationService>()
                .AddSingleton<IPollService, PollService>()
                .AddSingleton<ISessionService, SessionService>()
                .AddSingleton<IUserService, UserService>()
                .AddSingleton<IVotingProfileService, VotingProfileService>()
                .AddSingleton<IVotingService, VotingService>()
                ;
        });
    }

    public async Task Handle(VotingRequest request, ILambdaContext context)
    {
        var logger = ServiceProvider.GetRequiredService<ILogger<Function>>();
        var handler = ServiceProvider.GetRequiredService<IEventHandler>();

        if (request == null)
        {
            throw new ArgumentNullException(nameof(request), "Voting request cannot be null");
        }

        logger.LogInformation("Processing request \"{Request}\"", JsonSerializer.Serialize(request));

        await handler.Handle(request);
    }
}
