using IdeasToVote.Api.DTOs;

namespace IdeasToVote.Api.Services;

public enum CastVoteResultType
{
    IdeaNotFound,
    InvalidValue,
    Success
}

public sealed class CastVoteResult
{
    public CastVoteResultType Type { get; init; }
    public VoteResponse? Vote { get; init; }
}
