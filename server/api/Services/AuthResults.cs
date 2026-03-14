using IdeasToVote.Api.DTOs;

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
