using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolleyPlanner.API.Data;
using VolleyPlanner.API.DTOs.TrainingSession;
using VolleyPlanner.API.Entities;
using VolleyPlanner.API.Enums;

namespace VolleyPlanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TrainingSessionsController : ControllerBase
{
    private readonly AppDbContext _context;

    public TrainingSessionsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TrainingSessionListItemDto>>> GetAll()
    {
        var sessions = await _context.TrainingSessions
            .Where(ts => ts.Status == "Open")
            .OrderBy(ts => ts.StartTime)
            .Select(ts => new TrainingSessionListItemDto
            {
                Id = ts.Id,
                Title = ts.Title,
                SportType = ts.SportType.ToString(),
                StartTime = ts.StartTime,
                EndTime = ts.EndTime,
                Location = ts.Location,
                MaxParticipants = ts.MaxParticipants,
                TargetLevel = ts.TargetLevel,
                Status = ts.Status,
                OrganizerUserId = ts.OrganizerUserId,
                OrganizerName = ts.OrganizerUser.Name
            })
            .ToListAsync();

        return Ok(sessions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TrainingSessionDetailsDto>> GetById(int id)
    {
        var session = await _context.TrainingSessions
            .Where(ts => ts.Id == id)
            .Select(ts => new TrainingSessionDetailsDto
            {
                Id = ts.Id,
                OrganizerUserId = ts.OrganizerUserId,
                OrganizerName = ts.OrganizerUser.Name,
                TrainingPlanId = ts.TrainingPlanId,
                TrainingPlanTitle = ts.TrainingPlan != null
                    ? ts.TrainingPlan.Title
                    : null,
                Title = ts.Title,
                Description = ts.Description,
                SportType = ts.SportType.ToString(),
                StartTime = ts.StartTime,
                EndTime = ts.EndTime,
                Location = ts.Location,
                MaxParticipants = ts.MaxParticipants,
                TargetLevel = ts.TargetLevel,
                Status = ts.Status,
                CreatedAt = ts.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (session == null)
        {
            return NotFound(new
            {
                message = "Az edzés nem található."
            });
        }

        return Ok(session);
    }

    [HttpGet("mine")]
    public async Task<ActionResult<IEnumerable<TrainingSessionListItemDto>>> GetMine()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var sessions = await _context.TrainingSessions
            .Where(ts => ts.OrganizerUserId == userId.Value)
            .OrderByDescending(ts => ts.StartTime)
            .Select(ts => new TrainingSessionListItemDto
            {
                Id = ts.Id,
                Title = ts.Title,
                SportType = ts.SportType.ToString(),
                StartTime = ts.StartTime,
                EndTime = ts.EndTime,
                Location = ts.Location,
                MaxParticipants = ts.MaxParticipants,
                TargetLevel = ts.TargetLevel,
                Status = ts.Status,
                OrganizerUserId = ts.OrganizerUserId,
                OrganizerName = ts.OrganizerUser.Name
            })
            .ToListAsync();

        return Ok(sessions);
    }

    [HttpPost]
    public async Task<ActionResult<TrainingSessionDetailsDto>> Create(
        CreateTrainingSessionRequestDto request)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var isOrganizerCoach = await _context.UserSportRoles
            .AnyAsync(usr =>
                usr.UserId == userId.Value &&
                usr.RoleType == SportRoleType.OrganizerCoach);

        if (!isOrganizerCoach)
        {
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(new
            {
                message = "Az edzés címe kötelező."
            });
        }

        if (string.IsNullOrWhiteSpace(request.Location))
        {
            return BadRequest(new
            {
                message = "A helyszín megadása kötelező."
            });
        }

        if (request.EndTime <= request.StartTime)
        {
            return BadRequest(new
            {
                message = "A befejezési időnek későbbinek kell lennie, mint a kezdési idő."
            });
        }

        if (request.MaxParticipants <= 0)
        {
            return BadRequest(new
            {
                message = "A maximális létszámnak pozitív értéknek kell lennie."
            });
        }

        if (!Enum.TryParse<SportType>(
                request.SportType,
                true,
                out var sportType))
        {
            return BadRequest(new
            {
                message = "Érvénytelen sportág."
            });
        }

        TrainingPlan? trainingPlan = null;

        if (request.TrainingPlanId.HasValue)
        {
            trainingPlan = await _context.TrainingPlans
                .FirstOrDefaultAsync(tp =>
                    tp.Id == request.TrainingPlanId.Value &&
                    tp.UserId == userId.Value);

            if (trainingPlan == null)
            {
                return BadRequest(new
                {
                    message = "A kiválasztott edzésterv nem található vagy nem a bejelentkezett felhasználóhoz tartozik."
                });
            }

            if (trainingPlan.SportType != sportType)
            {
                return BadRequest(new
                {
                    message = "Az edzés sportágának meg kell egyeznie a kiválasztott edzésterv sportágával."
                });
            }
        }

        var trainingSession = new TrainingSession
        {
            OrganizerUserId = userId.Value,
            TrainingPlanId = trainingPlan?.Id,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            SportType = sportType,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Location = request.Location.Trim(),
            MaxParticipants = request.MaxParticipants,
            TargetLevel = request.TargetLevel.Trim(),
            Status = "Open",
            CreatedAt = DateTime.UtcNow
        };

        _context.TrainingSessions.Add(trainingSession);
        await _context.SaveChangesAsync();

        var result = await _context.TrainingSessions
            .Where(ts => ts.Id == trainingSession.Id)
            .Select(ts => new TrainingSessionDetailsDto
            {
                Id = ts.Id,
                OrganizerUserId = ts.OrganizerUserId,
                OrganizerName = ts.OrganizerUser.Name,
                TrainingPlanId = ts.TrainingPlanId,
                TrainingPlanTitle = ts.TrainingPlan != null
                    ? ts.TrainingPlan.Title
                    : null,
                Title = ts.Title,
                Description = ts.Description,
                SportType = ts.SportType.ToString(),
                StartTime = ts.StartTime,
                EndTime = ts.EndTime,
                Location = ts.Location,
                MaxParticipants = ts.MaxParticipants,
                TargetLevel = ts.TargetLevel,
                Status = ts.Status,
                CreatedAt = ts.CreatedAt
            })
            .FirstAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result);
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

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