using PollyUniverse.Func.Voting.Models;
using PollyUniverse.Func.Voting.Services.Telegram;
using PollyUniverse.Shared.Models;
using WTelegram;

namespace PollyUniverse.Func.Voting.Services;

public interface IVotingService
{
    Task<VotingResult> WaitForPollAndVote(
        Client telegramClient,
        VotingProfilePoll pollDescriptor,
        VotingProfileSession sessionDescriptor);
}

public class VotingService : IVotingService
{
    private readonly ITelegramPeerService _telegramPeerService;
    private readonly ITelegramVoteService _telegramVoteService;
    private readonly IPollService _pollService;
    private readonly FunctionConfig _config;

    public VotingService(
        ITelegramPeerService telegramPeerService,
        ITelegramVoteService telegramVoteService,
        IPollService pollService,
        FunctionConfig config)
    {
        _telegramPeerService = telegramPeerService;
        _telegramVoteService = telegramVoteService;
        _pollService = pollService;
        _config = config;
    }

    public async Task<VotingResult> WaitForPollAndVote(
        Client telegramClient,
        VotingProfilePoll pollDescriptor,
        VotingProfileSession sessionDescriptor)
    {
        if (TryGetFakeResult(_config.DevUseFakeVotingResult, out var fakeResult))
        {
            return fakeResult;
        }

        var votingInputPeer = await _telegramPeerService.GetInputPeer(telegramClient, pollDescriptor.PeerId);

        if (votingInputPeer == null)
        {
            throw new Exception($"No input peer found for voting: {pollDescriptor.PeerId}");
        }

        var pollMessage = await _pollService.WaitForPollMessage(
            telegramClient,
            pollDescriptor,
            TimeSpan.FromMinutes(_config.PollWaitingMinutes));

        if (pollMessage == null)
        {
            return VotingResult.PollNotFound;
        }

        var voted = await _telegramVoteService.Vote(telegramClient, votingInputPeer, pollMessage, sessionDescriptor.VoteIndex);
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
