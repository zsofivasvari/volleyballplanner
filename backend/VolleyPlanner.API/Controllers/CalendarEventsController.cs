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

    public CalendarEventsController(
        AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CalendarEventDto>>> GetAll()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var events = await _context.CalendarEvents
            .Where(e => e.UserId == userId.Value)
            .OrderBy(e => e.StartTime)
            .Select(e => new CalendarEventDto
            {
                Id = e.Id,
                TrainingPlanId = e.TrainingPlanId,
                TrainingSessionId = e.TrainingSessionId,
                Title = e.Title,
                SportType = e.SportType.ToString(),
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                Status = e.Status
            })
            .ToListAsync();

        return Ok(events);
    }

    // Meglévő, manuális edzésterv-tervezés.
    [HttpPost]
    public async Task<ActionResult<CalendarEventDto>> Create(
        CreateCalendarEventRequestDto request)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var trainingPlan =
            await _context.TrainingPlans
                .FirstOrDefaultAsync(tp =>
                    tp.Id == request.TrainingPlanId &&
                    tp.UserId == userId.Value);

        if (trainingPlan == null)
        {
            return BadRequest(new
            {
                message =
                    "A kiválasztott edzésterv nem található."
            });
        }

        if (request.EndTime <= request.StartTime)
        {
            return BadRequest(new
            {
                message =
                    "A befejezési időnek későbbinek kell lennie, mint a kezdési idő."
            });
        }

        var startTimeOfDay =
            request.StartTime.TimeOfDay;

        var endTimeOfDay =
            request.EndTime.TimeOfDay;

        if (
            startTimeOfDay <
                TimeSpan.FromHours(CalendarStartHour) ||
            startTimeOfDay >=
                TimeSpan.FromHours(CalendarEndHour))
        {
            return BadRequest(new
            {
                message =
                    "A kezdési idő csak 06:00 és 21:30 között lehet."
            });
        }

        if (
            endTimeOfDay <=
                TimeSpan.FromHours(CalendarStartHour) ||
            endTimeOfDay >
                TimeSpan.FromHours(CalendarEndHour))
        {
            return BadRequest(new
            {
                message =
                    "A befejezési idő csak 06:30 és 22:00 között lehet."
            });
        }

        var actualDurationMinutes =
            (int)(request.EndTime -
                  request.StartTime)
                .TotalMinutes;

        if (
            actualDurationMinutes !=
            trainingPlan.TargetDuration)
        {
            return BadRequest(new
            {
                message =
                    "Az esemény időtartamának meg kell egyeznie a kiválasztott edzésterv időtartamával."
            });
        }

        var hasConflict =
            await _context.CalendarEvents
                .AnyAsync(e =>
                    e.UserId == userId.Value &&
                    e.StartTime < request.EndTime &&
                    e.EndTime > request.StartTime);

        if (hasConflict)
        {
            return BadRequest(new
            {
                message =
                    "Ebben az időpontban már van egy másik eseményed."
            });
        }

        var calendarEvent =
            new CalendarEvent
            {
                UserId = userId.Value,
                TrainingPlanId = trainingPlan.Id,
                TrainingSessionId = null,

                Title =
                    string.IsNullOrWhiteSpace(
                        request.Title)
                        ? trainingPlan.Title
                        : request.Title,

                SportType =
                    trainingPlan.SportType,

                StartTime = request.StartTime,
                EndTime = request.EndTime,

                Status = "Planned",
                CreatedAt = DateTime.UtcNow
            };

        _context.CalendarEvents.Add(
            calendarEvent);

        await _context.SaveChangesAsync();

        return Ok(ToDto(calendarEvent));
    }

    // Meghirdetett edzés hozzáadása a saját naptárhoz.
    [HttpPost(
        "from-training-session/{trainingSessionId}")]
    public async Task<ActionResult<CalendarEventDto>>
        CreateFromTrainingSession(
            int trainingSessionId)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var session =
            await _context.TrainingSessions
                .FirstOrDefaultAsync(ts =>
                    ts.Id == trainingSessionId);

        if (session == null)
        {
            return NotFound(new
            {
                message =
                    "Az edzés nem található."
            });
        }

        if (session.StartTime <= DateTime.UtcNow)
        {
            return BadRequest(new
            {
                message =
                    "Már megkezdődött vagy lezajlott edzést nem lehet hozzáadni a tervezőhöz."
            });
        }

        // A saját szervező hozzáadhatja.
        var isOrganizer =
            session.OrganizerUserId ==
            userId.Value;

        // Játékos csak Confirmed jelentkezéssel.
        var hasConfirmedBooking =
            await _context.TrainingBookings
                .AnyAsync(tb =>
                    tb.TrainingSessionId ==
                        trainingSessionId &&
                    tb.UserId == userId.Value &&
                    tb.Status == "Confirmed");

        if (!isOrganizer &&
            !hasConfirmedBooking)
        {
            return BadRequest(new
            {
                message =
                    "Az edzést csak a szervező vagy biztos jelentkezéssel rendelkező játékos adhatja a tervezőjéhez."
            });
        }

        var alreadyExists =
            await _context.CalendarEvents
                .AnyAsync(e =>
                    e.UserId == userId.Value &&
                    e.TrainingSessionId ==
                        trainingSessionId);

        if (alreadyExists)
        {
            return BadRequest(new
            {
                message =
                    "Ez az edzés már szerepel a terveződben."
            });
        }

        // A heti tervező jelenleg 06:00–22:00 között működik.
        var startTimeOfDay =
            session.StartTime.TimeOfDay;

        var endTimeOfDay =
            session.EndTime.TimeOfDay;

        if (
            startTimeOfDay <
                TimeSpan.FromHours(CalendarStartHour) ||
            startTimeOfDay >=
                TimeSpan.FromHours(CalendarEndHour) ||
            endTimeOfDay <=
                TimeSpan.FromHours(CalendarStartHour) ||
            endTimeOfDay >
                TimeSpan.FromHours(CalendarEndHour))
        {
            return BadRequest(new
            {
                message =
                    "Az edzés időpontja kívül esik a tervező 06:00–22:00 közötti időtartományán."
            });
        }

        var hasConflict =
            await _context.CalendarEvents
                .AnyAsync(e =>
                    e.UserId == userId.Value &&
                    e.StartTime <
                        session.EndTime &&
                    e.EndTime >
                        session.StartTime);

        if (hasConflict)
        {
            return BadRequest(new
            {
                message =
                    "Ebben az időpontban már van egy másik eseményed a tervezőben."
            });
        }

        var calendarEvent =
            new CalendarEvent
            {
                UserId = userId.Value,

                TrainingPlanId =
                    session.TrainingPlanId,

                TrainingSessionId =
                    session.Id,

                Title = session.Title,

                SportType =
                    session.SportType,

                StartTime =
                    session.StartTime,

                EndTime =
                    session.EndTime,

                Status = "Planned",

                CreatedAt =
                    DateTime.UtcNow
            };

        _context.CalendarEvents.Add(
            calendarEvent);

        await _context.SaveChangesAsync();

        return Ok(
            ToDto(calendarEvent));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(
        int id)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var calendarEvent =
            await _context.CalendarEvents
                .FirstOrDefaultAsync(e =>
                    e.Id == id &&
                    e.UserId == userId.Value);

        if (calendarEvent == null)
        {
            return NotFound();
        }

        _context.CalendarEvents.Remove(
            calendarEvent);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static CalendarEventDto ToDto(
        CalendarEvent calendarEvent)
    {
        return new CalendarEventDto
        {
            Id = calendarEvent.Id,

            TrainingPlanId =
                calendarEvent.TrainingPlanId,

            TrainingSessionId =
                calendarEvent.TrainingSessionId,

            Title = calendarEvent.Title,

            SportType =
                calendarEvent.SportType.ToString(),

            StartTime =
                calendarEvent.StartTime,

            EndTime =
                calendarEvent.EndTime,

            Status = calendarEvent.Status
        };
    }

    private int? GetCurrentUserId()
    {
        var claim =
            User.FindFirst(
                ClaimTypes.NameIdentifier)
                ?.Value;

        if (!int.TryParse(
                claim,
                out var userId))
        {
            return null;
        }

        return userId;
    }
}