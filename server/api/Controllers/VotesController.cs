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
public class VotesController(IVoteService voteService) : ControllerBase
{
    [HttpPut("{ideaId:int}")]
    public async Task<ActionResult<VoteResponse>> CastVote([FromRoute] int ideaId, [FromBody] CastVoteRequest request)
    {
        if (!TryGetAuthenticatedUserId(out var authenticatedUserId))
        {
            return Unauthorized(new ApiMessageResponse { Message = ApiMessages.InvalidAuthenticationToken });
        }

        var result = await voteService.CastVoteAsync(authenticatedUserId, ideaId, request.Value);

        if (result.Type == CastVoteResultType.IdeaNotFound)
        {
            return NotFound(new ApiMessageResponse { Message = ApiMessages.IdeaNotFound });
        }

        if (result.Type == CastVoteResultType.InvalidValue)
        {
            return BadRequest(new ApiMessageResponse { Message = ApiMessages.VoteValueInvalid });
        }

        return Ok(result.Vote);
    }

    private bool TryGetAuthenticatedUserId(out int userId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out userId);
    }
}
