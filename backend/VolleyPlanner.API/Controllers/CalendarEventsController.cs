using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VolleyPlanner.API.Data;
using VolleyPlanner.API.DTOs.Planner;
using VolleyPlanner.API.Entities;

namespace VolleyPlanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CalendarEventsController : ControllerBase
{
    private readonly AppDbContext _context;
    private const int CalendarStartHour = 6;
    private const int CalendarEndHour = 22;

    public CalendarEventsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CalendarEventDto>>> GetAll()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var events = await _context.CalendarEvents
            .Where(e => e.UserId == userId)
            .OrderBy(e => e.StartTime)
            .Select(e => new CalendarEventDto
            {
                Id = e.Id,
                TrainingPlanId = e.TrainingPlanId,
                Title = e.Title,
                SportType = e.SportType.ToString(),
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                Status = e.Status
            })
            .ToListAsync();

        return Ok(events);
    }

    [HttpPost]
    public async Task<ActionResult<CalendarEventDto>> Create(CreateCalendarEventRequestDto request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var trainingPlan = await _context.TrainingPlans
            .FirstOrDefaultAsync(tp => tp.Id == request.TrainingPlanId && tp.UserId == userId);

        if (trainingPlan == null)
        {
            return BadRequest(new { message = "A kiválasztott edzésterv nem található." });
        }

        if (request.EndTime <= request.StartTime)
        {
            return BadRequest(new { message = "A befejezési időnek későbbinek kell lennie, mint a kezdési idő." });
        }

        var startTimeOfDay = request.StartTime.TimeOfDay;
        var endTimeOfDay = request.EndTime.TimeOfDay;

        if (startTimeOfDay < TimeSpan.FromHours(CalendarStartHour) ||
            startTimeOfDay >= TimeSpan.FromHours(CalendarEndHour))
        {
            return BadRequest(new { message = "A kezdési idő csak 06:00 és 21:30 között lehet." });
        }

        if (endTimeOfDay <= TimeSpan.FromHours(CalendarStartHour) ||
            endTimeOfDay > TimeSpan.FromHours(CalendarEndHour))
        {
            return BadRequest(new { message = "A befejezési idő csak 06:30 és 22:00 között lehet." });
        }

        var actualDurationMinutes = (int)(request.EndTime - request.StartTime).TotalMinutes;

        if (actualDurationMinutes != trainingPlan.TargetDuration)
        {
            return BadRequest(new { message = "Az esemény időtartamának meg kell egyeznie a kiválasztott edzésterv időtartamával." });
        }

        var hasConflict = await _context.CalendarEvents.AnyAsync(e =>
            e.UserId == userId &&
            e.StartTime < request.EndTime &&
            e.EndTime > request.StartTime);

        if (hasConflict)
        {
            return BadRequest(new { message = "Ebben az időpontban már van egy másik eseményed." });
        }

        var calendarEvent = new CalendarEvent
        {
            UserId = userId,
            TrainingPlanId = trainingPlan.Id,
            Title = string.IsNullOrWhiteSpace(request.Title) ? trainingPlan.Title : request.Title,
            SportType = trainingPlan.SportType,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Status = "Planned",
            CreatedAt = DateTime.UtcNow
        };

        _context.CalendarEvents.Add(calendarEvent);
        await _context.SaveChangesAsync();

        var result = new CalendarEventDto
        {
            Id = calendarEvent.Id,
            TrainingPlanId = calendarEvent.TrainingPlanId,
            Title = calendarEvent.Title,
            SportType = calendarEvent.SportType.ToString(),
            StartTime = calendarEvent.StartTime,
            EndTime = calendarEvent.EndTime,
            Status = calendarEvent.Status
        };

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var calendarEvent = await _context.CalendarEvents
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

        if (calendarEvent == null)
        {
            return NotFound();
        }

        _context.CalendarEvents.Remove(calendarEvent);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}