using IdeasToVote.Api.Data;
using IdeasToVote.Api.DTOs;
using IdeasToVote.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace IdeasToVote.Api.Services;

public class VoteService(ApplicationDbContext dbContext) : IVoteService
{
    public async Task<CastVoteResult> CastVoteAsync(int authenticatedUserId, int ideaId, int value)
    {
        if (value < 1 || value > 5)
        {
            return new CastVoteResult { Type = CastVoteResultType.InvalidValue };
        }

        var ideaExists = await dbContext.Ideas.AnyAsync(i => i.Id == ideaId);
        if (!ideaExists)
        {
            return new CastVoteResult { Type = CastVoteResultType.IdeaNotFound };
        }

        var vote = await dbContext.Votes
            .FirstOrDefaultAsync(v => v.IdeaId == ideaId && v.UserId == authenticatedUserId);

        if (vote is null)
        {
            vote = new Vote
            {
                IdeaId = ideaId,
                UserId = authenticatedUserId,
                Value = value,
                CreatedAt = DateTime.UtcNow
            };
            dbContext.Votes.Add(vote);
        }
        else
        {
            vote.Value = value;
        }

        await dbContext.SaveChangesAsync();

        return new CastVoteResult
        {
            Type = CastVoteResultType.Success,
            Vote = new VoteResponse
            {
                IdeaId = vote.IdeaId,
                UserId = vote.UserId,
                Value = vote.Value,
                CreatedAt = vote.CreatedAt
            }
        };
    }
}
