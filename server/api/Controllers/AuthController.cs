using IdeasToVote.Api.Constants;
using IdeasToVote.Api.DTOs;
using IdeasToVote.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IdeasToVote.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    IAuthService authService,
    IUserService userService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await authService.LoginAsync(request);

        if (result.Type == LoginResultType.ValidationError)
        {
            return BadRequest(new ApiMessageResponse { Message = result.Message ?? ApiMessages.UsernamePasswordRequired });
        }

        if (result.Type == LoginResultType.InvalidCredentials)
        {
            return Unauthorized(new ApiMessageResponse { Message = result.Message ?? ApiMessages.InvalidCredentials });
        }

        return Ok(result.Response);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await authService.RegisterAsync(request);

        if (result.Type == RegisterResultType.ValidationError
            || result.Type == RegisterResultType.UsernameExists
            || result.Type == RegisterResultType.EmailExists)
        {
            return BadRequest(new ApiMessageResponse { Message = result.Message ?? ApiMessages.UsernameEmailPasswordRequired });
        }

        return CreatedAtAction(nameof(Register), result.Response);
    }

    [Authorize]
    [HttpDelete("users/{id:int}")]
    public async Task<IActionResult> DeleteUser([FromRoute] int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var authenticatedUserId))
        {
            return Unauthorized(new ApiMessageResponse { Message = ApiMessages.InvalidAuthenticationToken });
        }

        if (authenticatedUserId != id)
        {
            return Forbid();
        }

        var deleteResult = await userService.DeleteUserAsync(id);
        if (deleteResult == DeleteUserResult.NotFound)
        {
            return NotFound(new ApiMessageResponse { Message = ApiMessages.UserNotFound });
        }

        return NoContent();
    }
}
