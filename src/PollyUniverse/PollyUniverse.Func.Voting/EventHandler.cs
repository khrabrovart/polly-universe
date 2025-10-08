using Microsoft.Extensions.Logging;
using PollyUniverse.Func.Voting.Models;
using PollyUniverse.Func.Voting.Services;
using PollyUniverse.Shared.Models;

namespace PollyUniverse.Func.Voting;

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

        var duplicateSessions = votingProfile.Sessions
            .GroupBy(s => s.Id)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateSessions.Any())
        {
            _logger.LogError(
                "Duplicate session IDs found in VotingProfileId: {VotingProfileId}",
                request.VotingProfileId);

            return;
        }

        var sessionTasks = votingProfile.Sessions
            .Select(session => ProcessSession(votingProfile.Poll, session));

        await Task.WhenAll(sessionTasks);

        _logger.LogInformation(
            "Completed voting request for VotingProfileId: {VotingProfileId}",
            request.VotingProfileId);
    }

    private async Task ProcessSession(VotingProfilePoll pollDescriptor, VotingProfileSession sessionDescriptor)
    {
        _logger.LogInformation("Voting starting for session {SessionId}", sessionDescriptor.Id);

        var telegramClient = await _sessionService.InitializeTelegramClientWithSession(sessionDescriptor.Id);
        var votingResult = await _votingService.WaitForPollAndVote(telegramClient, pollDescriptor, sessionDescriptor.VoteIndex);
        await _notificationService.SendNotification(telegramClient, votingResult);

        _logger.LogInformation("Voting complete for session {SessionId} with result {VotingResult}", sessionDescriptor.Id, votingResult);
    }
}
