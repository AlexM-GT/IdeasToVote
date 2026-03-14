using IdeasToVote.Api.Data;
using IdeasToVote.Api.Constants;
using IdeasToVote.Api.DTOs;
using IdeasToVote.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace IdeasToVote.Api.Services;

public enum LoginResultType
{
    ValidationError,
    InvalidCredentials,
    Success
}

public sealed class LoginResult
{
    public LoginResultType Type { get; init; }
    public string? Message { get; init; }
    public AuthResponse? Response { get; init; }
}

public enum RegisterResultType
{
    ValidationError,
    UsernameExists,
    EmailExists,
    Success
}

public sealed class RegisterResult
{
    public RegisterResultType Type { get; init; }
    public string? Message { get; init; }
    public AuthResponse? Response { get; init; }
}

public interface IAuthService
{
    Task<LoginResult> LoginAsync(LoginRequest request);
    Task<RegisterResult> RegisterAsync(RegisterRequest request);
}

public class AuthService(
    ApplicationDbContext dbContext,
    IPasswordService passwordService,
    ITokenService tokenService) : IAuthService
{
    public async Task<LoginResult> LoginAsync(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return new LoginResult
            {
                Type = LoginResultType.ValidationError,
                Message = ApiMessages.UsernamePasswordRequired
            };
        }

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (user == null || !passwordService.VerifyPassword(request.Password, user.PasswordHash, user.Salt))
        {
            return new LoginResult
            {
                Type = LoginResultType.InvalidCredentials,
                Message = ApiMessages.InvalidCredentials
            };
        }

        var token = tokenService.GenerateToken(user);

        return new LoginResult
        {
            Type = LoginResultType.Success,
            Response = new AuthResponse
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Token = token
            }
        };
    }

    public async Task<RegisterResult> RegisterAsync(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username)
            || string.IsNullOrWhiteSpace(request.Email)
            || string.IsNullOrWhiteSpace(request.Password))
        {
            return new RegisterResult
            {
                Type = RegisterResultType.ValidationError,
                Message = ApiMessages.UsernameEmailPasswordRequired
            };
        }

        if (await dbContext.Users.AnyAsync(u => u.Username == request.Username))
        {
            return new RegisterResult
            {
                Type = RegisterResultType.UsernameExists,
                Message = ApiMessages.UsernameAlreadyExists
            };
        }

        if (await dbContext.Users.AnyAsync(u => u.Email == request.Email))
        {
            return new RegisterResult
            {
                Type = RegisterResultType.EmailExists,
                Message = ApiMessages.EmailAlreadyExists
            };
        }

        var (passwordHash, salt) = passwordService.HashPassword(request.Password);
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHash,
            Salt = salt,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var token = tokenService.GenerateToken(user);

        return new RegisterResult
        {
            Type = RegisterResultType.Success,
            Response = new AuthResponse
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Token = token
            }
        };
    }
}
