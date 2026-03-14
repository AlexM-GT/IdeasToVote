using IdeasToVote.Api.DTOs;

namespace IdeasToVote.Api.Services;

public enum CreateIdeaResultType
{
    ValidationError,
    Created
}

public sealed class CreateIdeaResult
{
    public CreateIdeaResultType Type { get; init; }
    public string? Message { get; init; }
    public IdeaResponse? Idea { get; init; }
}

public enum UpdateIdeaResultType
{
    ValidationError,
    NotFound,
    Forbidden,
    Updated
}

public sealed class UpdateIdeaResult
{
    public UpdateIdeaResultType Type { get; init; }
    public string? Message { get; init; }
    public IdeaResponse? Idea { get; init; }
}

public enum DeleteIdeaResultType
{
    NotFound,
    Forbidden,
    Deleted
}
