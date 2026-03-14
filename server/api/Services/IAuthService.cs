using IdeasToVote.Api.DTOs;

namespace IdeasToVote.Api.Services;

public interface IAuthService
{
    Task<LoginResult> LoginAsync(LoginRequest request);
    Task<RegisterResult> RegisterAsync(RegisterRequest request);
}
