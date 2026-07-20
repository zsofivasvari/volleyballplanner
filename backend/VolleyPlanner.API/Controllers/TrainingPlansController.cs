using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolleyPlanner.API.Data;
using VolleyPlanner.API.DTOs.TrainingPlan;
using VolleyPlanner.API.Entities;
using VolleyPlanner.API.Enums;

namespace VolleyPlanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TrainingPlansController : ControllerBase
{
    private readonly AppDbContext _context;

    public TrainingPlansController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TrainingPlanListItemDto>>> GetAll()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var plans = await _context.TrainingPlans
            .Where(tp => tp.UserId == userId.Value)
            .OrderByDescending(tp => tp.CreatedAt)
            .Select(tp => new TrainingPlanListItemDto
            {
                Id = tp.Id,
                Title = tp.Title,
                SportType = tp.SportType.ToString(),
                TargetDuration = tp.TargetDuration,
                PrimaryFocus = tp.PrimaryFocus,
                PlayerCount = tp.PlayerCount,
                CreatedAt = tp.CreatedAt
            })
            .ToListAsync();

        return Ok(plans);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TrainingPlanDetailsDto>> GetById(int id)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var plan = await _context.TrainingPlans
            .Include(tp => tp.Items)
                .ThenInclude(item => item.Exercise)
            .FirstOrDefaultAsync(tp => tp.Id == id && tp.UserId == userId.Value);

        if (plan == null)
        {
            return NotFound(new { message = "Az edzésterv nem található." });
        }

        var result = new TrainingPlanDetailsDto
        {
            Id = plan.Id,
            Title = plan.Title,
            SportType = plan.SportType.ToString(),
            PlanType = plan.PlanType,
            TargetDuration = plan.TargetDuration,
            TargetIntensity = plan.TargetIntensity,
            TargetLevel = plan.TargetLevel,
            PrimaryFocus = plan.PrimaryFocus,
            PlayerCount = plan.PlayerCount,
            CreatedAt = plan.CreatedAt,
            Items = plan.Items
                .OrderBy(item => item.OrderIndex)
                .Select(item => new TrainingPlanDetailsItemDto
                {
                    ExerciseId = item.ExerciseId,
                    ExerciseTitle = item.Exercise.Title,
                    OrderIndex = item.OrderIndex,
                    SectionName = item.SectionName,
                    PlannedDuration = item.PlannedDuration
                })
                .ToList()
        };

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<TrainingPlanDetailsDto>> Create(CreateTrainingPlanRequestDto request)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        if (!Enum.TryParse<SportType>(request.SportType, true, out var sportType))
        {
            return BadRequest(new { message = "Érvénytelen sportág." });
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(new { message = "Az edzésterv címe kötelező." });
        }

        if (request.TargetDuration <= 0)
        {
            return BadRequest(new { message = "Az edzésterv időtartamának pozitív értéknek kell lennie." });
        }

        if (sportType == SportType.BeachVolleyball &&
            (!request.PlayerCount.HasValue || request.PlayerCount.Value <= 0))
        {
            return BadRequest(new { message = "Strandröplabda edzéstervnél a játékosok száma kötelező." });
        }

        if (request.Items == null || request.Items.Count == 0)
        {
            return BadRequest(new { message = "Az edzéstervnek legalább egy gyakorlatot tartalmaznia kell." });
        }

        var exerciseIds = request.Items
            .Select(item => item.ExerciseId)
            .Distinct()
            .ToList();

        var existingExerciseIds = await _context.Exercises
            .Where(exercise => exerciseIds.Contains(exercise.Id))
            .Select(exercise => exercise.Id)
            .ToListAsync();

        if (existingExerciseIds.Count != exerciseIds.Count)
        {
            return BadRequest(new { message = "Az egyik vagy több ExerciseId nem létezik." });
        }

        var plan = new TrainingPlan
        {
            UserId = userId.Value,
            Title = request.Title,
            SportType = sportType,
            PlanType = string.IsNullOrWhiteSpace(request.PlanType)
                ? "Single"
                : request.PlanType,
            TargetDuration = request.TargetDuration,
            TargetIntensity = request.TargetIntensity,
            TargetLevel = request.TargetLevel,
            PrimaryFocus = request.PrimaryFocus,
            PlayerCount = sportType == SportType.BeachVolleyball
                ? request.PlayerCount
                : null,
            CreatedAt = DateTime.UtcNow
        };

        _context.TrainingPlans.Add(plan);
        await _context.SaveChangesAsync();

        var planItems = request.Items
            .Select(item => new TrainingPlanItem
            {
                TrainingPlanId = plan.Id,
                ExerciseId = item.ExerciseId,
                OrderIndex = item.OrderIndex,
                SectionName = item.SectionName,
                PlannedDuration = item.PlannedDuration
            })
            .ToList();

        _context.TrainingPlanItems.AddRange(planItems);
        await _context.SaveChangesAsync();

        var createdPlan = await _context.TrainingPlans
            .Include(tp => tp.Items)
                .ThenInclude(item => item.Exercise)
            .FirstAsync(tp => tp.Id == plan.Id);

        var result = new TrainingPlanDetailsDto
        {
            Id = createdPlan.Id,
            Title = createdPlan.Title,
            SportType = createdPlan.SportType.ToString(),
            PlanType = createdPlan.PlanType,
            TargetDuration = createdPlan.TargetDuration,
            TargetIntensity = createdPlan.TargetIntensity,
            TargetLevel = createdPlan.TargetLevel,
            PrimaryFocus = createdPlan.PrimaryFocus,
            PlayerCount = createdPlan.PlayerCount,
            CreatedAt = createdPlan.CreatedAt,
            Items = createdPlan.Items
                .OrderBy(item => item.OrderIndex)
                .Select(item => new TrainingPlanDetailsItemDto
                {
                    ExerciseId = item.ExerciseId,
                    ExerciseTitle = item.Exercise.Title,
                    OrderIndex = item.OrderIndex,
                    SectionName = item.SectionName,
                    PlannedDuration = item.PlannedDuration
                })
                .ToList()
        };

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim))
        {
            return null;
        }

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        return userId;
    }
}