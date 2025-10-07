using Microsoft.Extensions.Logging;
using PollyUniverse.Voting.Func.Models;
using PollyUniverse.Voting.Func.Services.Telegram;
using WTelegram;

namespace PollyUniverse.Voting.Func.Services;

public interface IVotingService
{
    Task WaitForPollAndVote(Client telegramClient, VotingProfile votingProfile);
}

public class VotingService : IVotingService
{
    private readonly ITelegramPeerService _telegramPeerService;
    private readonly ITelegramPollService _telegramPollService;
    private readonly ITelegramVoteService _telegramVoteService;
    private readonly ILogger<VotingService> _logger;
    private readonly FunctionConfig _config;

    public VotingService(
        ITelegramPeerService telegramPeerService,
        ITelegramPollService telegramPollService,
        ITelegramVoteService telegramVoteService,
        ILogger<VotingService> logger,
        FunctionConfig config)
    {
        _telegramPeerService = telegramPeerService;
        _telegramPollService = telegramPollService;
        _telegramVoteService = telegramVoteService;
        _logger = logger;
        _config = config;
    }

    public async Task WaitForPollAndVote(Client telegramClient, VotingProfile votingProfile)
    {
        var votingInputPeer = await _telegramPeerService.GetInputPeer(telegramClient, votingProfile.Poll.PeerId);

        if (votingInputPeer == null)
        {
            throw new Exception($"No input peer found for voting: {votingProfile.Poll.PeerId}");
        }

        _logger.LogInformation("Waiting for poll message");

        var pollMessage = await _telegramPollService.WaitForPollMessage(
            telegramClient,
            votingProfile.Poll,
            TimeSpan.FromMinutes(_config.PollWaitingMinutes));

        if (pollMessage == null)
        {
            throw new Exception("No poll message received within the waiting period");
        }

        _logger.LogInformation("Received poll message with MessageId: {MessageId}", pollMessage.MessageId);

        var voted = await _telegramVoteService.Vote(telegramClient, votingInputPeer, pollMessage, votingProfile.Session.VoteIndex);

        if (!voted)
        {
            throw new Exception($"Failed to vote on poll message: {pollMessage.MessageId}");
        }
    }
}
