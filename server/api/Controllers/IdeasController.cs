using IdeasToVote.Api.Constants;
using IdeasToVote.Api.DTOs;
using IdeasToVote.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IdeasToVote.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class IdeasController(IIdeaService ideaService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<IdeaResponse>>> GetAllIdeas()
    {
        var ideas = await ideaService.GetAllIdeasAsync();
        return Ok(ideas);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<IdeaResponse>> GetIdea([FromRoute] int id)
    {
        var idea = await ideaService.GetIdeaByIdAsync(id);

        if (idea is null)
        {
            return NotFound(new ApiMessageResponse { Message = ApiMessages.IdeaNotFound });
        }

        return Ok(idea);
    }

    [HttpPost]
    public async Task<ActionResult<IdeaResponse>> CreateIdea([FromBody] CreateIdeaRequest request)
    {
        if (!TryGetAuthenticatedUserId(out var authenticatedUserId))
        {
            return Unauthorized(new ApiMessageResponse { Message = ApiMessages.InvalidAuthenticationToken });
        }

        var result = await ideaService.CreateIdeaAsync(authenticatedUserId, request);
        if (result.Type == CreateIdeaResultType.ValidationError)
        {
            return BadRequest(new ApiMessageResponse { Message = result.Message ?? ApiMessages.IdeaTitleRequired });
        }

        return CreatedAtAction(nameof(GetIdea), new { id = result.Idea!.Id }, result.Idea);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<IdeaResponse>> UpdateIdea([FromRoute] int id, [FromBody] UpdateIdeaRequest request)
    {
        if (!TryGetAuthenticatedUserId(out var authenticatedUserId))
        {
            return Unauthorized(new ApiMessageResponse { Message = ApiMessages.InvalidAuthenticationToken });
        }

        var result = await ideaService.UpdateIdeaAsync(id, authenticatedUserId, request);
        if (result.Type == UpdateIdeaResultType.ValidationError)
        {
            return BadRequest(new ApiMessageResponse { Message = result.Message ?? ApiMessages.IdeaTitleRequired });
        }

        if (result.Type == UpdateIdeaResultType.NotFound)
        {
            return NotFound(new ApiMessageResponse { Message = result.Message ?? ApiMessages.IdeaNotFound });
        }

        if (result.Type == UpdateIdeaResultType.Forbidden)
        {
            return Forbid();
        }

        return Ok(result.Idea);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteIdea([FromRoute] int id)
    {
        if (!TryGetAuthenticatedUserId(out var authenticatedUserId))
        {
            return Unauthorized(new ApiMessageResponse { Message = ApiMessages.InvalidAuthenticationToken });
        }

        var deleteResult = await ideaService.DeleteIdeaAsync(id, authenticatedUserId);
        if (deleteResult == DeleteIdeaResultType.NotFound)
        {
            return NotFound(new ApiMessageResponse { Message = ApiMessages.IdeaNotFound });
        }

        if (deleteResult == DeleteIdeaResultType.Forbidden)
        {
            return Forbid();
        }

        return NoContent();
    }

    private bool TryGetAuthenticatedUserId(out int userId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out userId);
    }
}
