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
        _logger.LogInformation("Handling voting request for voting profile \"{VotingProfileId}\"", request.VotingProfileId);

        var votingProfile = await _votingProfileService.GetVotingProfile(request.VotingProfileId);

        if (HasDuplicateSessions(votingProfile))
        {
            _logger.LogError("Duplicate sessions found in voting profile \"{VotingProfileId}\"", request.VotingProfileId);
            return;
        }

        var sessionTasks = votingProfile.Sessions
            .Select(session => ProcessSession(votingProfile.Poll, session));

        await Task.WhenAll(sessionTasks);

        _logger.LogInformation("Completed voting request for voting profile \"{VotingProfileId}\"", request.VotingProfileId);
    }

    private async Task ProcessSession(VotingProfilePoll pollDescriptor, VotingProfileSession sessionDescriptor)
    {
        try
        {
            _logger.LogInformation("Voting starting for session \"{SessionId}\"", sessionDescriptor.Id);

            var telegramClient = await _sessionService.InitializeTelegramClientWithSession(sessionDescriptor.Id);
            var votingResult = await _votingService.WaitForPollAndVote(telegramClient, pollDescriptor, sessionDescriptor.VoteIndex);
            await _notificationService.SendNotification(telegramClient, votingResult);

            _logger.LogInformation("Voting complete for session \"{SessionId}\" with result \"{VotingResult}\"", sessionDescriptor.Id, votingResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing session \"{SessionId}\": {ErrorMessage}", sessionDescriptor.Id, ex.Message);
        }
    }

    private static bool HasDuplicateSessions(VotingProfile votingProfile)
    {
        var duplicateSessions = votingProfile.Sessions
            .GroupBy(s => s.Id)
            .Count(g => g.Count() > 1);

        return duplicateSessions > 0;
    }
}
