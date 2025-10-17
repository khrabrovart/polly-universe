using Microsoft.Extensions.Logging;
using PollyUniverse.Func.Voting.Models;
using PollyUniverse.Func.Voting.Services;
using PollyUniverse.Shared.Models;
using WTelegram;

namespace PollyUniverse.Func.Voting;

public interface IEventHandler
{
    Task Handle(VotingRequest request);
}

public class EventHandler : IEventHandler
{
    private readonly INotificationService _notificationService;
    private readonly IUserService _userService;
    private readonly IVotingProfileService _votingProfileService;
    private readonly IVotingService _votingService;
    private readonly ILogger<EventHandler> _logger;

    public EventHandler(
        INotificationService notificationService,
        IUserService userService,
        IVotingProfileService votingProfileService,
        IVotingService votingService,
        ILogger<EventHandler> logger)
    {
        _notificationService = notificationService;
        _userService = userService;
        _votingProfileService = votingProfileService;
        _votingService = votingService;
        _logger = logger;
    }

    public async Task Handle(VotingRequest request)
    {
        _logger.LogInformation("Handling voting request for voting profile \"{VotingProfileId}\"", request.VotingProfileId);

        var votingProfile = await _votingProfileService.GetVotingProfile(request.VotingProfileId);

        if (!votingProfile.Enabled)
        {
            _logger.LogWarning("Voting profile \"{VotingProfileId}\" is disabled", request.VotingProfileId);
            return;
        }

        if (HasDuplicateUsers(votingProfile))
        {
            _logger.LogError("Duplicate users found in voting profile \"{VotingProfileId}\"", request.VotingProfileId);
            return;
        }

        var telegramClientTasks = votingProfile.Users
            .Where(s => s.Enabled)
            .Select(async user =>
            {
                var client = await _userService.InitializeTelegramClientForUser(user.Id);
                return (User: user, Client: client);
            })
            .ToArray();

        await Task.WhenAll(telegramClientTasks);

        var telegramClients = telegramClientTasks
            .Select(task => task.Result)
            .ToArray();

        var userTasks = telegramClients
            .Select(x => ProcessUser(x.Client, votingProfile.Poll, x.User));

        await Task.WhenAll(userTasks);

        _logger.LogInformation("Voting request complete for voting profile \"{VotingProfileId}\"", request.VotingProfileId);
    }

    private async Task ProcessUser(
        Client telegramClient,
        VotingProfilePoll pollDescriptor,
        VotingProfileUser userDescriptor)
    {
        try
        {
            _logger.LogInformation("Voting starting for user \"{UserId}\", waiting for poll to appear...", userDescriptor.Id);

            var votingResult = await _votingService.WaitForPollAndVote(telegramClient, pollDescriptor, userDescriptor);
            await _notificationService.SendNotification(telegramClient, votingResult);

            _logger.LogInformation("Voting complete for user \"{UserId}\" with result \"{VotingResult}\"", userDescriptor.Id, votingResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing user \"{UserId}\": {ErrorMessage}", userDescriptor.Id, ex.Message);
        }
    }

    private static bool HasDuplicateUsers(VotingProfile votingProfile)
    {
        var duplicateUsers = votingProfile.Users
            .GroupBy(s => s.Id)
            .Count(g => g.Count() > 1);

        return duplicateUsers > 0;
    }
}
