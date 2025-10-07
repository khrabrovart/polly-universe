using PollyUniverse.Func.Scheduling.Models;
using PollyUniverse.Func.Scheduling.Repositories;

namespace PollyUniverse.Func.Scheduling.Services;

public interface IVotingProfileService
{
    Task<VotingProfile> GetVotingProfile(string votingProfileId);
}

public class VotingProfileService : IVotingProfileService
{
    private readonly IVotingProfileRepository _votingProfileRepository;

    public VotingProfileService(IVotingProfileRepository votingProfileRepository)
    {
        _votingProfileRepository = votingProfileRepository;
    }

    public async Task<VotingProfile> GetVotingProfile(string votingProfileId)
    {
        var votingProfile = await _votingProfileRepository.Get(votingProfileId);
        return votingProfile ?? throw new Exception($"No voting profile found: {votingProfileId}");
    }
}
