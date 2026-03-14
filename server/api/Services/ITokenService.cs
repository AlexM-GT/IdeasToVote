using IdeasToVote.Api.Models;

namespace IdeasToVote.Api.Services;

public interface ITokenService
{
    string GenerateToken(User user);
}
