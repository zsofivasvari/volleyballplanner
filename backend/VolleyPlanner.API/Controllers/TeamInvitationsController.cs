using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolleyPlanner.API.DTOs.TeamInvitation;
using VolleyPlanner.API.DTOs.Tournament;
using VolleyPlanner.API.Interfaces;

namespace VolleyPlanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TeamInvitationsController
    : ControllerBase
{
    private readonly ITeamInvitationService
        _teamInvitationService;

    public TeamInvitationsController(
        ITeamInvitationService teamInvitationService)
    {
        _teamInvitationService =
            teamInvitationService;
    }

    [HttpPost]
    public async Task<
        ActionResult<TeamInvitationResponseDto>>
        Create(
            CreateTeamInvitationRequestDto request)
    {
        try
        {
            var userId =
                GetCurrentUserId();

            var result =
                await _teamInvitationService
                    .CreateAsync(
                        userId,
                        request);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new
            {
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpGet("received")]
    public async Task<
        ActionResult<
            List<TeamInvitationResponseDto>>>
        GetReceived()
    {
        var userId =
            GetCurrentUserId();

        var result =
            await _teamInvitationService
                .GetReceivedAsync(userId);

        return Ok(result);
    }

    [HttpGet("sent")]
    public async Task<
        ActionResult<
            List<TeamInvitationResponseDto>>>
        GetSent()
    {
        var userId =
            GetCurrentUserId();

        var result =
            await _teamInvitationService
                .GetSentAsync(userId);

        return Ok(result);
    }

    [HttpPost("{id:int}/accept")]
    public async Task<
        ActionResult<TournamentEntryResponseDto>>
        Accept(int id)
    {
        try
        {
            var userId =
                GetCurrentUserId();

            var result =
                await _teamInvitationService
                    .AcceptAsync(
                        id,
                        userId);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new
            {
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPost("{id:int}/reject")]
    public async Task<
        ActionResult<TeamInvitationResponseDto>>
        Reject(int id)
    {
        try
        {
            var userId =
                GetCurrentUserId();

            var result =
                await _teamInvitationService
                    .RejectAsync(
                        id,
                        userId);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new
            {
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    private int GetCurrentUserId()
    {
        var userIdClaim =
            User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) ||
            !int.TryParse(
                userIdClaim,
                out var userId))
        {
            throw new UnauthorizedAccessException(
                "Érvénytelen vagy hiányzó felhasználói azonosító.");
        }

        return userId;
    }
}