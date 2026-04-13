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

        Console.WriteLine(
            $"[CalendarEvents] Új esemény kérés | UserId={userId} | Start={request.StartTime:yyyy-MM-dd HH:mm:ss} | End={request.EndTime:yyyy-MM-dd HH:mm:ss}");

        var existingEvents = await _context.CalendarEvents
            .Where(e => e.UserId == userId)
            .OrderBy(e => e.StartTime)
            .ToListAsync();

        foreach (var existing in existingEvents)
        {
            var overlaps =
                existing.StartTime < request.EndTime &&
                existing.EndTime > request.StartTime;

            Console.WriteLine(
                $"[CalendarEvents] Létező esemény | Id={existing.Id} | Start={existing.StartTime:yyyy-MM-dd HH:mm:ss} | End={existing.EndTime:yyyy-MM-dd HH:mm:ss} | Overlaps={overlaps}");
        }

        var conflictingEvent = existingEvents.FirstOrDefault(e =>
            e.StartTime < request.EndTime &&
            e.EndTime > request.StartTime);

        if (conflictingEvent != null)
        {
            Console.WriteLine(
                $"[CalendarEvents] ÜTKÖZÉS | ExistingId={conflictingEvent.Id} | ExistingStart={conflictingEvent.StartTime:yyyy-MM-dd HH:mm:ss} | ExistingEnd={conflictingEvent.EndTime:yyyy-MM-dd HH:mm:ss}");

            return BadRequest(new
            {
                message = "Ebben az időpontban már van egy másik eseményed."
            });
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

        Console.WriteLine(
            $"[CalendarEvents] Sikeres létrehozás | Id={calendarEvent.Id} | Start={calendarEvent.StartTime:yyyy-MM-dd HH:mm:ss} | End={calendarEvent.EndTime:yyyy-MM-dd HH:mm:ss}");

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

        Console.WriteLine(
            $"[CalendarEvents] Törölve | Id={calendarEvent.Id} | Start={calendarEvent.StartTime:yyyy-MM-dd HH:mm:ss} | End={calendarEvent.EndTime:yyyy-MM-dd HH:mm:ss}");

        return NoContent();
    }
}