using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolleyPlanner.API.DTOs.Generation;
using VolleyPlanner.API.Interfaces;

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
        try
        {
            var result = await _generationService.GenerateAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}