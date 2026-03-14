using IdeasToVote.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace IdeasToVote.Api.Services;

public class UserService(ApplicationDbContext dbContext) : IUserService
{
    public async Task<DeleteUserResult> DeleteUserAsync(int userId)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return DeleteUserResult.NotFound;
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        // Votes are restricted on User delete; remove user's votes first.
        var userVotes = dbContext.Votes.Where(v => v.UserId == userId);
        dbContext.Votes.RemoveRange(userVotes);

        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        return DeleteUserResult.Deleted;
    }
}
