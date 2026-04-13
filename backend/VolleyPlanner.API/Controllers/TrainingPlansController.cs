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
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized();
        }

        var userId = int.Parse(userIdClaim);

        var plans = await _context.TrainingPlans
            .Where(tp => tp.UserId == userId)
            .OrderByDescending(tp => tp.CreatedAt)
            .Select(tp => new TrainingPlanListItemDto
            {
                Id = tp.Id,
                Title = tp.Title,
                SportType = tp.SportType.ToString(),
                TargetDuration = tp.TargetDuration,
                PrimaryFocus = tp.PrimaryFocus,
                CreatedAt = tp.CreatedAt
            })
            .ToListAsync();

        return Ok(plans);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TrainingPlanDetailsDto>> GetById(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized();
        }

        var userId = int.Parse(userIdClaim);

        var plan = await _context.TrainingPlans
            .Include(tp => tp.Items)
                .ThenInclude(i => i.Exercise)
            .FirstOrDefaultAsync(tp => tp.Id == id && tp.UserId == userId);

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
            CreatedAt = plan.CreatedAt,
            Items = plan.Items
                .OrderBy(i => i.OrderIndex)
                .Select(i => new TrainingPlanDetailsItemDto
                {
                    ExerciseId = i.ExerciseId,
                    ExerciseTitle = i.Exercise.Title,
                    OrderIndex = i.OrderIndex,
                    SectionName = i.SectionName,
                    PlannedDuration = i.PlannedDuration
                })
                .ToList()
        };

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<TrainingPlanDetailsDto>> Create(CreateTrainingPlanRequestDto request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized();
        }

        var userId = int.Parse(userIdClaim);

        if (!Enum.TryParse<SportType>(request.SportType, true, out var sportType))
        {
            return BadRequest(new { message = "Érvénytelen SportType." });
        }

        var exerciseIds = request.Items.Select(i => i.ExerciseId).Distinct().ToList();
        var existingExerciseIds = await _context.Exercises
            .Where(e => exerciseIds.Contains(e.Id))
            .Select(e => e.Id)
            .ToListAsync();

        if (existingExerciseIds.Count != exerciseIds.Count)
        {
            return BadRequest(new { message = "Az egyik vagy több ExerciseId nem létezik." });
        }

        var plan = new TrainingPlan
        {
            UserId = userId,
            Title = request.Title,
            SportType = sportType,
            PlanType = request.PlanType,
            TargetDuration = request.TargetDuration,
            TargetIntensity = request.TargetIntensity,
            TargetLevel = request.TargetLevel,
            PrimaryFocus = request.PrimaryFocus,
            CreatedAt = DateTime.UtcNow
        };

        _context.TrainingPlans.Add(plan);
        await _context.SaveChangesAsync();

        var items = request.Items.Select(i => new TrainingPlanItem
        {
            TrainingPlanId = plan.Id,
            ExerciseId = i.ExerciseId,
            OrderIndex = i.OrderIndex,
            SectionName = i.SectionName,
            PlannedDuration = i.PlannedDuration
        }).ToList();

        _context.TrainingPlanItems.AddRange(items);
        await _context.SaveChangesAsync();

        var createdPlan = await _context.TrainingPlans
            .Include(tp => tp.Items)
                .ThenInclude(i => i.Exercise)
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
            CreatedAt = createdPlan.CreatedAt,
            Items = createdPlan.Items
                .OrderBy(i => i.OrderIndex)
                .Select(i => new TrainingPlanDetailsItemDto
                {
                    ExerciseId = i.ExerciseId,
                    ExerciseTitle = i.Exercise.Title,
                    OrderIndex = i.OrderIndex,
                    SectionName = i.SectionName,
                    PlannedDuration = i.PlannedDuration
                })
                .ToList()
        };

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}