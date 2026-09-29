using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolleyPlanner.API.Data;
using VolleyPlanner.API.DTOs.TrainingBooking;

namespace VolleyPlanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TrainingBookingsController : ControllerBase
{
    private readonly AppDbContext _context;

    public TrainingBookingsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("mine")]
    public async Task<ActionResult<IEnumerable<TrainingBookingDto>>> GetMine()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var bookings = await _context.TrainingBookings
            .Where(tb =>
                tb.UserId == userId.Value &&
                tb.Status == "Confirmed")
            .OrderBy(tb => tb.TrainingSession.StartTime)
            .Select(tb => new TrainingBookingDto
            {
                Id = tb.Id,
                TrainingSessionId = tb.TrainingSessionId,
                TrainingSessionTitle = tb.TrainingSession.Title,
                UserId = tb.UserId,
                UserName = tb.User.Name,
                Status = tb.Status,
                CreatedAt = tb.CreatedAt
            })
            .ToListAsync();

        return Ok(bookings);
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