namespace IdeasToVote.Api.DTOs;

public sealed class IdeaResponse
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int UserId { get; init; }
    public DateTime CreatedAt { get; init; }
}
