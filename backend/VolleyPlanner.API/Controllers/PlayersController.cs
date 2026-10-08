using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolleyPlanner.API.DTOs.Player;
using VolleyPlanner.API.Interfaces;

namespace VolleyPlanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlayersController : ControllerBase
{
    private readonly IPlayerService _playerService;

    public PlayersController(
        IPlayerService playerService)
    {
        _playerService = playerService;
    }

    [HttpGet("by-code/{playerCode}")]
    public async Task<ActionResult<PlayerSearchResponseDto>>
        GetByCode(string playerCode)
    {
        try
        {
            var currentUserId =
                GetCurrentUserId();

            var player =
                await _playerService.GetPlayerByCodeAsync(
                    playerCode,
                    currentUserId);

            if (player == null)
            {
                return NotFound(new
                {
                    message =
                        "Nem található ilyen játékoskóddal játékos."
                });
            }

            return Ok(player);
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