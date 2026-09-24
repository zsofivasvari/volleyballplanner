using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolleyPlanner.API.DTOs.SportRole;
using VolleyPlanner.API.Interfaces;

namespace VolleyPlanner.API.Controllers;

[ApiController]
[Route("api/users/me/sport-roles")]
[Authorize]
public class SportRolesController : ControllerBase
{
    private readonly ISportRoleService _sportRoleService;

    public SportRolesController(ISportRoleService sportRoleService)
    {
        _sportRoleService = sportRoleService;
    }

    [HttpGet]
    public async Task<ActionResult<SportRolesResponseDto>> GetSportRoles()
    {
        try
        {
            var userId = GetCurrentUserId();

            var result =
                await _sportRoleService.GetUserSportRolesAsync(userId);

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

    [HttpPut]
    public async Task<ActionResult<SportRolesResponseDto>> UpdateSportRoles(
        UpdateSportRolesRequestDto request)
    {
        try
        {
            var userId = GetCurrentUserId();

            var result =
                await _sportRoleService.UpdateUserSportRolesAsync(
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

    private int GetCurrentUserId()
    {
        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) ||
            !int.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException(
                "Érvénytelen vagy hiányzó felhasználói azonosító.");
        }

        return userId;
    }
}