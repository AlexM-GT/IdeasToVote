using IdeasToVote.Api.DTOs;

namespace IdeasToVote.Api.Services;

public interface IIdeaService
{
    Task<IReadOnlyList<IdeaResponse>> GetAllIdeasAsync();
    Task<IdeaResponse?> GetIdeaByIdAsync(int id);
    Task<CreateIdeaResult> CreateIdeaAsync(int authenticatedUserId, CreateIdeaRequest request);
    Task<UpdateIdeaResult> UpdateIdeaAsync(int id, int authenticatedUserId, UpdateIdeaRequest request);
    Task<DeleteIdeaResultType> DeleteIdeaAsync(int id, int authenticatedUserId);
}
