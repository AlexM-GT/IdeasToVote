namespace IdeasToVote.Api.Services;

public interface IUserService
{
    Task<DeleteUserResult> DeleteUserAsync(int userId);
}
