namespace IdeasToVote.Api.DTOs;

public sealed class VoteResponse
{
    public int IdeaId { get; init; }
    public int UserId { get; init; }
    public int Value { get; init; }
    public DateTime CreatedAt { get; init; }
}
