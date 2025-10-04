using Microsoft.Extensions.Logging;
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

    public EventHandler(
        IVotingProfileRepository votingProfileRepository,
        ITelegramService telegramService,
        ILogger<EventHandler> logger)
    {
        _votingProfileRepository = votingProfileRepository;
        _telegramService = telegramService;
        _logger = logger;
    }

    public async Task Handle(LambdaRequest evt)
    {
        _logger.LogInformation("Getting voting profile for ProfileId: {ProfileId}", evt.ProfileId);

        var profile = await _votingProfileRepository.Get(evt.ProfileId);

        if (profile == null)
        {
            throw new Exception($"No voting profile found for ProfileId: {evt.ProfileId}");
        }

        _logger.LogInformation("Initializing Telegram client for SessionId: {SessionId}", profile.SessionId);

        var client = await _telegramService.InitializeClient(profile.SessionId);

        _logger.LogInformation("Logged in as {User}", client.User.username);
    }
}
