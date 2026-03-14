using IdeasToVote.Api.Constants;
using IdeasToVote.Api.Data;
using IdeasToVote.Api.DTOs;
using IdeasToVote.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace IdeasToVote.Api.Services;

public class IdeaService(ApplicationDbContext dbContext) : IIdeaService
{
    public async Task<IReadOnlyList<IdeaResponse>> GetAllIdeasAsync(int authenticatedUserId)
    {
        return await dbContext.Ideas
            .AsNoTracking()
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new IdeaResponse
            {
                Id = i.Id,
                Title = i.Title,
                Description = i.Description,
                UserId = i.UserId,
                CreatedAt = i.CreatedAt,
                UserVote = i.Votes
                    .Where(v => v.UserId == authenticatedUserId)
                    .Select(v => (int?)v.Value)
                    .FirstOrDefault()
            })
            .ToListAsync();
    }

    public async Task<IdeaResponse?> GetIdeaByIdAsync(int id, int authenticatedUserId)
    {
        return await dbContext.Ideas
            .AsNoTracking()
            .Where(i => i.Id == id)
            .Select(i => new IdeaResponse
            {
                Id = i.Id,
                Title = i.Title,
                Description = i.Description,
                UserId = i.UserId,
                CreatedAt = i.CreatedAt,
                UserVote = i.Votes
                    .Where(v => v.UserId == authenticatedUserId)
                    .Select(v => (int?)v.Value)
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CreateIdeaResult> CreateIdeaAsync(int authenticatedUserId, CreateIdeaRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return new CreateIdeaResult
            {
                Type = CreateIdeaResultType.ValidationError,
                Message = ApiMessages.IdeaTitleRequired
            };
        }

        var idea = new Idea
        {
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            UserId = authenticatedUserId,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Ideas.Add(idea);
        await dbContext.SaveChangesAsync();

        return new CreateIdeaResult
        {
            Type = CreateIdeaResultType.Created,
            Idea = ToResponse(idea)
        };
    }

    public async Task<UpdateIdeaResult> UpdateIdeaAsync(int id, int authenticatedUserId, UpdateIdeaRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return new UpdateIdeaResult
            {
                Type = UpdateIdeaResultType.ValidationError,
                Message = ApiMessages.IdeaTitleRequired
            };
        }

        var idea = await dbContext.Ideas.FirstOrDefaultAsync(i => i.Id == id);
        if (idea is null)
        {
            return new UpdateIdeaResult
            {
                Type = UpdateIdeaResultType.NotFound,
                Message = ApiMessages.IdeaNotFound
            };
        }

        if (idea.UserId != authenticatedUserId)
        {
            return new UpdateIdeaResult
            {
                Type = UpdateIdeaResultType.Forbidden
            };
        }

        idea.Title = request.Title.Trim();
        idea.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        await dbContext.SaveChangesAsync();

        return new UpdateIdeaResult
        {
            Type = UpdateIdeaResultType.Updated,
            Idea = ToResponse(idea)
        };
    }

    public async Task<DeleteIdeaResultType> DeleteIdeaAsync(int id, int authenticatedUserId)
    {
        var idea = await dbContext.Ideas.FirstOrDefaultAsync(i => i.Id == id);
        if (idea is null)
        {
            return DeleteIdeaResultType.NotFound;
        }

        if (idea.UserId != authenticatedUserId)
        {
            return DeleteIdeaResultType.Forbidden;
        }

        dbContext.Ideas.Remove(idea);
        await dbContext.SaveChangesAsync();

        return DeleteIdeaResultType.Deleted;
    }

    private static IdeaResponse ToResponse(Idea idea)
    {
        return new IdeaResponse
        {
            Id = idea.Id,
            Title = idea.Title,
            Description = idea.Description,
            UserId = idea.UserId,
            CreatedAt = idea.CreatedAt
        };
    }
}
