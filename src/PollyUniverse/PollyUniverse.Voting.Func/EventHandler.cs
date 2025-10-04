using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PollyUniverse.Voting.Func.Models;
using PollyUniverse.Voting.Func.Repositories;
using PollyUniverse.Voting.Func.Services;

namespace PollyUniverse.Voting.Func;

public interface IEventHandler
{
    Task Handle(LambdaRequest evt);
}

public class EventHandler : IEventHandler
{
    private readonly IVotingProfileRepository _votingProfileRepository;
    private readonly ITelegramService _telegramService;
    private readonly ILogger<EventHandler> _logger;
    private readonly FunctionConfig _config;

    public EventHandler(
        IVotingProfileRepository votingProfileRepository,
        ITelegramService telegramService,
        ILogger<EventHandler> logger,
        IOptions<FunctionConfig> config)
    {
        _votingProfileRepository = votingProfileRepository;
        _telegramService = telegramService;
        _logger = logger;
        _config = config.Value;
    }

    public async Task Handle(LambdaRequest evt)
    {
        var profile = await _votingProfileRepository.Get(evt.ProfileId);

        if (profile == null)
        {
            throw new Exception($"No voting profile found for ProfileId: {evt.ProfileId}");
        }

        var client = await _telegramService.InitializeClient(profile.TelegramClientId);

        _logger.LogInformation("Logged in as {User}", client.User.username);
    }
}
