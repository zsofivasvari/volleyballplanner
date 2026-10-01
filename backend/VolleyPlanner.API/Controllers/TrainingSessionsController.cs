using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolleyPlanner.API.Data;
using VolleyPlanner.API.DTOs.TrainingBooking;
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

                ParticipantCount = ts.Bookings.Count(
                    tb => tb.Status == "Confirmed"
                ),

                WaitlistCount = ts.Bookings.Count(
                    tb => tb.Status == "Waitlisted"
                ),

                IsFull = ts.Bookings.Count(
                    tb => tb.Status == "Confirmed"
                ) >= ts.MaxParticipants,

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
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var session = await GetTrainingSessionDetailsAsync(
            id,
            userId.Value
        );

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

        var isOrganizerCoach =
            await _context.UserSportRoles
                .AnyAsync(usr =>
                    usr.UserId == userId.Value &&
                    usr.RoleType ==
                    SportRoleType.OrganizerCoach);

        if (!isOrganizerCoach)
        {
            return Forbid();
        }

        var sessions = await _context.TrainingSessions
            .Where(ts =>
                ts.OrganizerUserId == userId.Value)
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

                ParticipantCount = ts.Bookings.Count(
                    tb => tb.Status == "Confirmed"
                ),

                WaitlistCount = ts.Bookings.Count(
                    tb => tb.Status == "Waitlisted"
                ),

                IsFull = ts.Bookings.Count(
                    tb => tb.Status == "Confirmed"
                ) >= ts.MaxParticipants,

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

        var isOrganizerCoach =
            await _context.UserSportRoles
                .AnyAsync(usr =>
                    usr.UserId == userId.Value &&
                    usr.RoleType ==
                    SportRoleType.OrganizerCoach);

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

        if (request.StartTime <= DateTime.UtcNow)
        {
            return BadRequest(new
            {
                message =
                    "Az edzés kezdési időpontjának a jövőben kell lennie."
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

        if (request.MaxParticipants <= 0)
        {
            return BadRequest(new
            {
                message =
                    "A maximális létszámnak pozitív értéknek kell lennie."
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
            trainingPlan =
                await _context.TrainingPlans
                    .FirstOrDefaultAsync(tp =>
                        tp.Id ==
                        request.TrainingPlanId.Value &&
                        tp.UserId == userId.Value);

            if (trainingPlan == null)
            {
                return BadRequest(new
                {
                    message =
                        "A kiválasztott edzésterv nem található vagy nem a bejelentkezett felhasználóhoz tartozik."
                });
            }

            if (trainingPlan.SportType != sportType)
            {
                return BadRequest(new
                {
                    message =
                        "Az edzés sportágának meg kell egyeznie a kiválasztott edzésterv sportágával."
                });
            }

            var sessionDuration =
                (int)(request.EndTime - request.StartTime)
                    .TotalMinutes;

            if (trainingPlan.TargetDuration != sessionDuration)
            {
                return BadRequest(new
                {
                    message =
                        $"Az edzésterv időtartama ({trainingPlan.TargetDuration} perc) nem egyezik az edzés időtartamával ({sessionDuration} perc)."
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

        var result = await GetTrainingSessionDetailsAsync(
            trainingSession.Id,
            userId.Value
        );

        if (result == null)
        {
            return StatusCode(500, new
            {
                message =
                    "Az edzés létrejött, de az adatok visszaolvasása nem sikerült."
            });
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result
        );

        
    }

    // =========================================================
    // EDZÉS SZERKESZTÉSE
    // =========================================================

    [HttpPut("{id}")]
    public async Task<ActionResult<TrainingSessionDetailsDto>> Update(
        int id,
        UpdateTrainingSessionRequestDto request)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var isOrganizerCoach =
            await _context.UserSportRoles
                .AnyAsync(usr =>
                    usr.UserId == userId.Value &&
                    usr.RoleType == SportRoleType.OrganizerCoach);

        if (!isOrganizerCoach)
        {
            return Forbid();
        }

        var trainingSession =
            await _context.TrainingSessions
                .Include(ts => ts.TrainingPlan)
                .FirstOrDefaultAsync(ts => ts.Id == id);

        if (trainingSession == null)
        {
            return NotFound(new
            {
                message = "Az edzés nem található."
            });
        }

        if (trainingSession.OrganizerUserId != userId.Value)
        {
            return Forbid();
        }

        if (trainingSession.StartTime <= DateTime.UtcNow)
        {
            return BadRequest(new
            {
                message =
                    "Már megkezdődött vagy lezajlott edzés nem szerkeszthető."
            });
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

        if (request.StartTime <= DateTime.UtcNow)
        {
            return BadRequest(new
            {
                message =
                    "Az edzés kezdési időpontjának a jövőben kell lennie."
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

        if (request.MaxParticipants <= 0)
        {
            return BadRequest(new
            {
                message =
                    "A maximális létszámnak pozitív értéknek kell lennie."
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

        var confirmedParticipantCount =
            await _context.TrainingBookings
                .CountAsync(tb =>
                    tb.TrainingSessionId == id &&
                    tb.Status == "Confirmed");

        if (request.MaxParticipants < confirmedParticipantCount)
        {
            return BadRequest(new
            {
                message =
                    $"A maximális létszám nem lehet kisebb a már elfogadott résztvevők számánál ({confirmedParticipantCount})."
            });
        }

        /*
        * Ha van az edzéshez edzésterv rendelve, akkor az új
        * sportágnak és időtartamnak továbbra is kompatibilisnek
        * kell lennie vele.
        */
        if (trainingSession.TrainingPlan != null)
        {
            if (trainingSession.TrainingPlan.SportType != sportType)
            {
                return BadRequest(new
                {
                    message =
                        "A sportág nem módosítható erre az értékre, mert a hozzárendelt edzésterv más sportághoz tartozik. Előbb válaszd le az edzéstervet."
                });
            }

            var newDuration =
                (int)(request.EndTime - request.StartTime)
                    .TotalMinutes;

            if (trainingSession.TrainingPlan.TargetDuration !=
                newDuration)
            {
                return BadRequest(new
                {
                    message =
                        $"Az új időtartam ({newDuration} perc) nem egyezik a hozzárendelt edzésterv időtartamával ({trainingSession.TrainingPlan.TargetDuration} perc). Előbb válaszd le vagy cseréld le az edzéstervet."
                });
            }
        }

        trainingSession.Title =
            request.Title.Trim();

        trainingSession.Description =
            request.Description?.Trim() ?? string.Empty;

        trainingSession.SportType =
            sportType;

        trainingSession.StartTime =
            request.StartTime;

        trainingSession.EndTime =
            request.EndTime;

        trainingSession.Location =
            request.Location.Trim();

        trainingSession.MaxParticipants =
            request.MaxParticipants;

        trainingSession.TargetLevel =
            request.TargetLevel?.Trim() ?? string.Empty;

        /*
        * Ha az edzés már bekerült felhasználók heti
        * tervezőjébe, az ottani eseményt is frissítjük.
        */
        var relatedCalendarEvents =
            await _context.CalendarEvents
                .Where(ce =>
                    ce.TrainingSessionId == id)
                .ToListAsync();

        foreach (var calendarEvent in relatedCalendarEvents)
        {
            calendarEvent.Title =
                trainingSession.Title;

            calendarEvent.SportType =
                trainingSession.SportType;

            calendarEvent.StartTime =
                trainingSession.StartTime;

            calendarEvent.EndTime =
                trainingSession.EndTime;
        }

        await _context.SaveChangesAsync();

        var result =
            await GetTrainingSessionDetailsAsync(
                id,
                userId.Value);

        if (result == null)
        {
            return StatusCode(500, new
            {
                message =
                    "Az edzés módosult, de az adatok visszaolvasása nem sikerült."
            });
        }

        return Ok(result);
    }

    // =========================================================
    // EDZÉS TÖRLÉSE
    // =========================================================

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(
        int id)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var isOrganizerCoach =
            await _context.UserSportRoles
                .AnyAsync(usr =>
                    usr.UserId == userId.Value &&
                    usr.RoleType == SportRoleType.OrganizerCoach);

        if (!isOrganizerCoach)
        {
            return Forbid();
        }

        var trainingSession =
            await _context.TrainingSessions
                .FirstOrDefaultAsync(ts =>
                    ts.Id == id);

        if (trainingSession == null)
        {
            return NotFound(new
            {
                message = "Az edzés nem található."
            });
        }

        if (trainingSession.OrganizerUserId !=
            userId.Value)
        {
            return Forbid();
        }

        if (trainingSession.StartTime <=
            DateTime.UtcNow)
        {
            return BadRequest(new
            {
                message =
                    "Már megkezdődött vagy lezajlott edzés nem törölhető."
            });
        }

        /*
        * Az edzéshez tartozó naptáreseményeket eltávolítjuk.
        *
        * Ha egy meghirdetett edzés megszűnik, annak nincs
        * értelme továbbra is szerepelnie a résztvevők
        * heti tervezőjében.
        */
        var relatedCalendarEvents =
            await _context.CalendarEvents
                .Where(ce =>
                    ce.TrainingSessionId == id)
                .ToListAsync();

        if (relatedCalendarEvents.Count > 0)
        {
            _context.CalendarEvents.RemoveRange(
                relatedCalendarEvents);
        }

        /*
        * Jelentkezések és várólistás bejegyzések törlése.
        */
        var relatedBookings =
            await _context.TrainingBookings
                .Where(tb =>
                    tb.TrainingSessionId == id)
                .ToListAsync();

        if (relatedBookings.Count > 0)
        {
            _context.TrainingBookings.RemoveRange(
                relatedBookings);
        }

        _context.TrainingSessions.Remove(
            trainingSession);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // =========================================================
    // EDZÉSTERV HOZZÁRENDELÉSE
    // =========================================================

    [HttpPut("{id}/training-plan")]
    public async Task<ActionResult<TrainingSessionDetailsDto>> AssignTrainingPlan(
        int id,
        AssignTrainingPlanRequestDto request)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var trainingSession =
            await _context.TrainingSessions
                .FirstOrDefaultAsync(ts =>
                    ts.Id == id);

        if (trainingSession == null)
        {
            return NotFound(new
            {
                message = "Az edzés nem található."
            });
        }

        // Csak az adott edzés létrehozója módosíthatja.
        if (trainingSession.OrganizerUserId !=
            userId.Value)
        {
            return Forbid();
        }

        // Megkezdődött vagy lezajlott edzéshez
        // már nem módosítjuk az edzéstervet.
        if (trainingSession.StartTime <=
            DateTime.UtcNow)
        {
            return BadRequest(new
            {
                message =
                    "Már megkezdődött vagy lezajlott edzés edzésterve nem módosítható."
            });
        }

        var trainingPlan =
            await _context.TrainingPlans
                .FirstOrDefaultAsync(tp =>
                    tp.Id ==
                        request.TrainingPlanId &&
                    tp.UserId ==
                        userId.Value);

        if (trainingPlan == null)
        {
            return BadRequest(new
            {
                message =
                    "A kiválasztott edzésterv nem található vagy nem a te edzésterved."
            });
        }

        if (trainingPlan.SportType !=
            trainingSession.SportType)
        {
            return BadRequest(new
            {
                message =
                    "Az edzésterv sportágának meg kell egyeznie az edzés sportágával."
            });
        }

        var sessionDuration =
            (int)(
                trainingSession.EndTime -
                trainingSession.StartTime
            ).TotalMinutes;

        if (trainingPlan.TargetDuration !=
            sessionDuration)
        {
            return BadRequest(new
            {
                message =
                    $"Az edzésterv időtartama ({trainingPlan.TargetDuration} perc) nem egyezik az edzés időtartamával ({sessionDuration} perc)."
            });
        }

        trainingSession.TrainingPlanId =
            trainingPlan.Id;

        // Ha valaki már korábban betette ezt az edzést
        // a saját naptárába, ott is frissítjük a tervet.
        var relatedCalendarEvents =
            await _context.CalendarEvents
                .Where(ce =>
                    ce.TrainingSessionId == id)
                .ToListAsync();

        foreach (var calendarEvent in
                 relatedCalendarEvents)
        {
            calendarEvent.TrainingPlanId =
                trainingPlan.Id;
        }

        await _context.SaveChangesAsync();

        var result =
            await GetTrainingSessionDetailsAsync(
                id,
                userId.Value
            );

        return Ok(result);
    }

    // =========================================================
    // EDZÉSTERV LEVÁLASZTÁSA
    // =========================================================

    [HttpDelete("{id}/training-plan")]
    public async Task<IActionResult> RemoveTrainingPlan(
        int id)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var trainingSession =
            await _context.TrainingSessions
                .FirstOrDefaultAsync(ts =>
                    ts.Id == id);

        if (trainingSession == null)
        {
            return NotFound(new
            {
                message = "Az edzés nem található."
            });
        }

        if (trainingSession.OrganizerUserId !=
            userId.Value)
        {
            return Forbid();
        }

        if (trainingSession.StartTime <=
            DateTime.UtcNow)
        {
            return BadRequest(new
            {
                message =
                    "Már megkezdődött vagy lezajlott edzés edzésterve nem módosítható."
            });
        }

        if (!trainingSession.TrainingPlanId.HasValue)
        {
            return BadRequest(new
            {
                message =
                    "Ehhez az edzéshez jelenleg nincs edzésterv hozzárendelve."
            });
        }

        trainingSession.TrainingPlanId = null;

        var relatedCalendarEvents =
            await _context.CalendarEvents
                .Where(ce =>
                    ce.TrainingSessionId == id)
                .ToListAsync();

        foreach (var calendarEvent in
                 relatedCalendarEvents)
        {
            calendarEvent.TrainingPlanId = null;
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // =========================================================
    // JELENTKEZÉS / VÁRÓLISTA
    // =========================================================

    [HttpPost("{id}/book")]
    public async Task<ActionResult<TrainingBookingDto>> Book(
        int id)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var isPlayer =
            await _context.UserSportRoles
                .AnyAsync(usr =>
                    usr.UserId == userId.Value &&
                    usr.RoleType ==
                    SportRoleType.Player);

        if (!isPlayer)
        {
            return Forbid();
        }

        var trainingSession =
            await _context.TrainingSessions
                .FirstOrDefaultAsync(ts =>
                    ts.Id == id);

        if (trainingSession == null)
        {
            return NotFound(new
            {
                message = "Az edzés nem található."
            });
        }

        if (trainingSession.Status != "Open")
        {
            return BadRequest(new
            {
                message =
                    "Erre az edzésre jelenleg nem lehet jelentkezni."
            });
        }

        if (trainingSession.StartTime <=
            DateTime.UtcNow)
        {
            return BadRequest(new
            {
                message =
                    "A már megkezdődött vagy lezajlott edzésre nem lehet jelentkezni."
            });
        }

        var existingBooking =
            await _context.TrainingBookings
                .FirstOrDefaultAsync(tb =>
                    tb.TrainingSessionId == id &&
                    tb.UserId ==
                        userId.Value);

        if (existingBooking?.Status ==
            "Confirmed")
        {
            return BadRequest(new
            {
                message =
                    "Erre az edzésre már jelentkeztél."
            });
        }

        if (existingBooking?.Status ==
            "Waitlisted")
        {
            var position =
                await GetWaitlistPositionAsync(
                    id,
                    existingBooking
                );

            return BadRequest(new
            {
                message =
                    $"Már rajta vagy a várólistán. Jelenlegi helyezésed: {position}."
            });
        }

        var participantCount =
            await _context.TrainingBookings
                .CountAsync(tb =>
                    tb.TrainingSessionId == id &&
                    tb.Status == "Confirmed");

        var newStatus =
            participantCount <
            trainingSession.MaxParticipants
                ? "Confirmed"
                : "Waitlisted";

        TrainingBooking booking;

        if (existingBooking != null &&
            existingBooking.Status ==
            "Cancelled")
        {
            existingBooking.Status =
                newStatus;

            existingBooking.CreatedAt =
                DateTime.UtcNow;

            booking = existingBooking;
        }
        else
        {
            booking = new TrainingBooking
            {
                TrainingSessionId =
                    trainingSession.Id,

                UserId = userId.Value,

                Status = newStatus,

                CreatedAt =
                    DateTime.UtcNow
            };

            _context.TrainingBookings.Add(
                booking
            );
        }

        await _context.SaveChangesAsync();

        int? waitlistPosition = null;

        if (booking.Status == "Waitlisted")
        {
            waitlistPosition =
                await GetWaitlistPositionAsync(
                    id,
                    booking
                );
        }

        var result =
            await _context.TrainingBookings
                .Where(tb =>
                    tb.Id == booking.Id)
                .Select(tb =>
                    new TrainingBookingDto
                    {
                        Id = tb.Id,

                        TrainingSessionId =
                            tb.TrainingSessionId,

                        TrainingSessionTitle =
                            tb.TrainingSession.Title,

                        UserId = tb.UserId,

                        UserName = tb.User.Name,

                        Status = tb.Status,

                        CreatedAt =
                            tb.CreatedAt
                    })
                .FirstAsync();

        result.WaitlistPosition =
            waitlistPosition;

        return Ok(result);
    }

    [HttpDelete("{id}/book")]
    public async Task<IActionResult> CancelBooking(
        int id)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var trainingSession =
            await _context.TrainingSessions
                .FirstOrDefaultAsync(ts =>
                    ts.Id == id);

        if (trainingSession == null)
        {
            return NotFound(new
            {
                message =
                    "Az edzés nem található."
            });
        }

        if (trainingSession.StartTime <=
            DateTime.UtcNow)
        {
            return BadRequest(new
            {
                message =
                    "A már megkezdődött vagy lezajlott edzésről nem lehet lejelentkezni."
            });
        }

        var booking =
            await _context.TrainingBookings
                .FirstOrDefaultAsync(tb =>
                    tb.TrainingSessionId == id &&
                    tb.UserId ==
                        userId.Value);

        if (booking == null)
        {
            return NotFound(new
            {
                message =
                    "Nincs jelentkezésed erre az edzésre."
            });
        }

        if (booking.Status ==
            "Cancelled")
        {
            return BadRequest(new
            {
                message =
                    "A jelentkezést már korábban lemondtad."
            });
        }

        var wasConfirmed =
            booking.Status ==
            "Confirmed";

        booking.Status =
            "Cancelled";

        if (wasConfirmed)
        {
            var nextWaitlisted =
                await _context.TrainingBookings
                    .Where(tb =>
                        tb.TrainingSessionId ==
                            id &&
                        tb.Status ==
                            "Waitlisted")
                    .OrderBy(tb =>
                        tb.CreatedAt)
                    .ThenBy(tb =>
                        tb.Id)
                    .FirstOrDefaultAsync();

            if (nextWaitlisted != null)
            {
                nextWaitlisted.Status =
                    "Confirmed";
            }
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // =========================================================
    // JELENTKEZŐK
    // =========================================================

    [HttpGet("{id}/participants")]
    public async Task<ActionResult<IEnumerable<TrainingParticipantDto>>> GetParticipants(
        int id)
    {
        var userId =
            GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var trainingSession =
            await _context.TrainingSessions
                .FirstOrDefaultAsync(ts =>
                    ts.Id == id);

        if (trainingSession == null)
        {
            return NotFound(new
            {
                message =
                    "Az edzés nem található."
            });
        }

        if (trainingSession.OrganizerUserId !=
            userId.Value)
        {
            return Forbid();
        }

        var participants =
            await _context.TrainingBookings
                .Where(tb =>
                    tb.TrainingSessionId ==
                        id &&
                    (
                        tb.Status ==
                            "Confirmed" ||
                        tb.Status ==
                            "Waitlisted"
                    ))
                .OrderBy(tb =>
                    tb.Status ==
                    "Confirmed"
                        ? 0
                        : 1)
                .ThenBy(tb =>
                    tb.CreatedAt)
                .ThenBy(tb =>
                    tb.Id)
                .Select(tb =>
                    new TrainingParticipantDto
                    {
                        UserId = tb.UserId,

                        UserName =
                            tb.User.Name,

                        Status =
                            tb.Status,

                        RegisteredAt =
                            tb.CreatedAt
                    })
                .ToListAsync();

        var waitlistPosition = 1;

        foreach (var participant in
                 participants)
        {
            if (participant.Status ==
                "Waitlisted")
            {
                participant.WaitlistPosition =
                    waitlistPosition;

                waitlistPosition++;
            }
        }

        return Ok(participants);
    }

    // =========================================================
    // SEGÉDFÜGGVÉNYEK
    // =========================================================

    private async Task<TrainingSessionDetailsDto?>
        GetTrainingSessionDetailsAsync(
            int id,
            int currentUserId)
    {
        var session =
            await _context.TrainingSessions
                .Where(ts =>
                    ts.Id == id)
                .Select(ts =>
                    new TrainingSessionDetailsDto
                    {
                        Id = ts.Id,

                        OrganizerUserId =
                            ts.OrganizerUserId,

                        OrganizerName =
                            ts.OrganizerUser.Name,

                        TrainingPlanId =
                            ts.OrganizerUserId == currentUserId
                                ? ts.TrainingPlanId
                                : null,

                        TrainingPlanTitle =
                            ts.OrganizerUserId == currentUserId &&
                            ts.TrainingPlan != null
                                ? ts.TrainingPlan.Title
                                : null,

                        Title = ts.Title,

                        Description =
                            ts.Description,

                        SportType =
                            ts.SportType.ToString(),

                        StartTime =
                            ts.StartTime,

                        EndTime =
                            ts.EndTime,

                        Location =
                            ts.Location,

                        MaxParticipants =
                            ts.MaxParticipants,

                        ParticipantCount =
                            ts.Bookings.Count(
                                tb =>
                                    tb.Status ==
                                    "Confirmed"
                            ),

                        WaitlistCount =
                            ts.Bookings.Count(
                                tb =>
                                    tb.Status ==
                                    "Waitlisted"
                            ),

                        IsFull =
                            ts.Bookings.Count(
                                tb =>
                                    tb.Status ==
                                    "Confirmed"
                            ) >=
                            ts.MaxParticipants,

                        TargetLevel =
                            ts.TargetLevel,

                        Status =
                            ts.Status,

                        CreatedAt =
                            ts.CreatedAt,

                        IsInCurrentUserCalendar =
                            ts.CalendarEvents.Any(
                                ce =>
                                    ce.UserId ==
                                    currentUserId
                            )
                    })
                .FirstOrDefaultAsync();

        if (session == null)
        {
            return null;
        }

        var currentBooking =
            await _context.TrainingBookings
                .FirstOrDefaultAsync(tb =>
                    tb.TrainingSessionId ==
                        id &&
                    tb.UserId ==
                        currentUserId &&
                    tb.Status !=
                        "Cancelled");

        if (currentBooking != null)
        {
            session.CurrentUserBookingStatus =
                currentBooking.Status;

            if (currentBooking.Status ==
                "Waitlisted")
            {
                session.WaitlistPosition =
                    await GetWaitlistPositionAsync(
                        id,
                        currentBooking
                    );
            }
        }

        return session;
    }

    private async Task<int>
        GetWaitlistPositionAsync(
            int trainingSessionId,
            TrainingBooking booking)
    {
        var usersBefore =
            await _context.TrainingBookings
                .CountAsync(tb =>
                    tb.TrainingSessionId ==
                        trainingSessionId &&
                    tb.Status ==
                        "Waitlisted" &&
                    (
                        tb.CreatedAt <
                            booking.CreatedAt ||
                        (
                            tb.CreatedAt ==
                                booking.CreatedAt &&
                            tb.Id <
                                booking.Id
                        )
                    ));

        return usersBefore + 1;
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim =
            User.FindFirst(
                ClaimTypes.NameIdentifier
            )?.Value;

        if (string.IsNullOrWhiteSpace(
                userIdClaim))
        {
            return null;
        }

        if (!int.TryParse(
                userIdClaim,
                out var userId))
        {
            return null;
        }

        return userId;
    }
}