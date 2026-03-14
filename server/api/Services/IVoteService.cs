namespace IdeasToVote.Api.Services;

public interface IVoteService
{
    Task<CastVoteResult> CastVoteAsync(int authenticatedUserId, int ideaId, int value);
}
