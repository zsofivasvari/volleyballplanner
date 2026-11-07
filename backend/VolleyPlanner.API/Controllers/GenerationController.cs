using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolleyPlanner.API.DTOs.Generation;
using VolleyPlanner.API.Interfaces;
using System.Security.Claims;

namespace VolleyPlanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GenerationController : ControllerBase
{
    private readonly IGenerationService _generationService;

    public GenerationController(IGenerationService generationService)
    {
        _generationService = generationService;
    }

    [HttpPost]
    public async Task<ActionResult<GeneratedTrainingPlanResponseDto>> Generate(
        GenerateTrainingPlanRequestDto request)
    {
        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim) ||
            !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var result =
                await _generationService.GenerateAsync(
                    userId,
                    request);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }
}