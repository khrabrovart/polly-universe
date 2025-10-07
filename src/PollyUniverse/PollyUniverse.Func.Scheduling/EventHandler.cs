using Microsoft.Extensions.Logging;
using PollyUniverse.Func.Scheduling.Models;
using PollyUniverse.Func.Scheduling.Services;

namespace PollyUniverse.Func.Scheduling;

public interface IEventHandler
{
    Task Handle(VotingRequest request);
}

public class EventHandler : IEventHandler
{
    private readonly INotificationService _notificationService;
    private readonly ISessionService _sessionService;
    private readonly IVotingProfileService _votingProfileService;
    private readonly IVotingService _votingService;
    private readonly ILogger<EventHandler> _logger;

    public EventHandler(
        INotificationService notificationService,
        ISessionService sessionService,
        IVotingProfileService votingProfileService,
        IVotingService votingService,
        ILogger<EventHandler> logger)
    {
        _notificationService = notificationService;
        _sessionService = sessionService;
        _votingProfileService = votingProfileService;
        _votingService = votingService;
        _logger = logger;
    }

    public async Task Handle(VotingRequest request)
    {
        _logger.LogInformation(
            "Handling voting request for VotingProfileId: {VotingProfileId}",
            request.VotingProfileId);

        var votingProfile = await _votingProfileService.GetVotingProfile(request.VotingProfileId);
        var telegramClient = await _sessionService.InitializeTelegramClientWithSession(votingProfile.Session.Id);
        var votingResult = await _votingService.WaitForPollAndVote(telegramClient, votingProfile);
        await _notificationService.SendNotification(telegramClient, votingResult);

        _logger.LogInformation(
            "Completed voting request for VotingProfileId: {VotingProfileId}",
            request.VotingProfileId);
    }
}
