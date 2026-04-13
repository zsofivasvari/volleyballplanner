using Microsoft.AspNetCore.Mvc;
using VolleyPlanner.API.DTOs.Auth;
using VolleyPlanner.API.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace VolleyPlanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponseDto>> Register(RegisterRequestDto request)
    {
        try
        {
            var result = await _authService.RegisterAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserProfileResponseDto>> Me()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new { message = "Érvénytelen token." });
            }

            var userId = int.Parse(userIdClaim);
            var result = await _authService.GetCurrentUserProfileAsync(userId);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
    {
        var success = await _authService.ConfirmEmailAsync(token);

        if (!success)
        {
            return BadRequest(new { message = "Érvénytelen vagy lejárt token." });
        }

        return Ok(new { message = "Az email cím sikeresen megerősítve." });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequestDto request)
    {
        await _authService.ForgotPasswordAsync(request.Email);

        return Ok(new
        {
            message = "Ha létezik ilyen email cím, elküldtük a visszaállítási linket."
        });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequestDto request)
    {
        var success = await _authService.ResetPasswordAsync(request.Token, request.NewPassword);

        if (!success)
        {
            return BadRequest(new { message = "Érvénytelen vagy lejárt token." });
        }

        return Ok(new { message = "A jelszó sikeresen módosítva." });
    }

}