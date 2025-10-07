using Microsoft.Extensions.Logging;
using PollyUniverse.Func.Scheduling.Models;
using PollyUniverse.Func.Scheduling.Services.Telegram;
using WTelegram;

namespace PollyUniverse.Func.Scheduling.Services;

public interface IVotingService
{
    Task<VotingResult> WaitForPollAndVote(Client telegramClient, VotingProfile votingProfile);
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

    public async Task<VotingResult> WaitForPollAndVote(Client telegramClient, VotingProfile votingProfile)
    {
        if (TryGetFakeResult(_config.UseVotingResult, out var fakeResult))
        {
            _logger.LogInformation("Using fake voting result: {Result}", fakeResult);
            return fakeResult;
        }

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
            return VotingResult.PollNotFound;
        }

        _logger.LogInformation("Received poll message with MessageId: {MessageId}", pollMessage.MessageId);

        var voted = await _telegramVoteService.Vote(telegramClient, votingInputPeer, pollMessage, votingProfile.Session.VoteIndex);

        return voted ? VotingResult.Success : VotingResult.VoteFailed;

    }

    private static bool TryGetFakeResult(string status, out VotingResult result)
    {
        if (Enum.TryParse<VotingResult>(status, true, out var parsedResult))
        {
            result = parsedResult;
            return true;
        }

        result = default;
        return false;
    }
}
