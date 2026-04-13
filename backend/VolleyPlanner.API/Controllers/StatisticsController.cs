using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolleyPlanner.API.Data;
using VolleyPlanner.API.DTOs.Statistics;
using VolleyPlanner.API.Enums;

namespace VolleyPlanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StatisticsController : ControllerBase
{
    private readonly AppDbContext _context;

    public StatisticsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<StatisticsSummaryDto>> GetMyStatistics([FromQuery] string range = "30days")
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var now = DateTime.UtcNow;
        var today = now.Date;

        var rangeNormalized = range.Trim().ToLowerInvariant();

        DateTime? rangeStart = rangeNormalized switch
        {
            "7days" => today.AddDays(-6),
            "30days" => today.AddDays(-29),
            "month" => new DateTime(today.Year, today.Month, 1),
            "all" => null,
            _ => today.AddDays(-29)
        };

        var weeklyStart = today.AddDays(
            today.DayOfWeek == DayOfWeek.Sunday ? -6 : 1 - (int)today.DayOfWeek
        );

        var monthStart = new DateTime(today.Year, today.Month, 1);

        var events = await _context.CalendarEvents
            .Include(e => e.TrainingPlan)
                .ThenInclude(tp => tp.Items)
                    .ThenInclude(i => i.Exercise)
                        .ThenInclude(ex => ex.ExerciseTags)
                            .ThenInclude(et => et.Tag)
            .Where(e => e.UserId == userId)
            .ToListAsync();

        var filteredEvents = rangeStart.HasValue
            ? events.Where(e => e.StartTime.Date >= rangeStart.Value && e.StartTime.Date <= today).ToList()
            : events;

        var weeklyCount = filteredEvents.Count(e =>
            e.StartTime.Date >= weeklyStart &&
            e.StartTime.Date <= today);

        var monthlyCount = filteredEvents.Count(e =>
            e.StartTime.Date >= monthStart &&
            e.StartTime.Date <= today);

        var totalDurationMinutes = filteredEvents.Sum(e =>
            Math.Max(0, (int)(e.EndTime - e.StartTime).TotalMinutes));

        var topPlans = filteredEvents
            .GroupBy(e => new
            {
                e.TrainingPlanId,
                Title = e.TrainingPlan != null ? e.TrainingPlan.Title : e.Title
            })
            .Select(g => new PlanUsageStatDto
            {
                TrainingPlanId = g.Key.TrainingPlanId,
                Title = g.Key.Title ?? "Ismeretlen terv",
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.Title)
            .Take(5)
            .ToList();

        var weeklyDailyStats = Enumerable.Range(0, 7)
            .Select(offset =>
            {
                var day = weeklyStart.AddDays(offset);
                var dayEvents = filteredEvents
                    .Where(e => e.StartTime.Date == day.Date)
                    .ToList();

                return new DailyTrainingStatDto
                {
                    DayLabel = day.ToString("ddd"),
                    Count = dayEvents.Count,
                    DurationMinutes = dayEvents.Sum(e =>
                        Math.Max(0, (int)(e.EndTime - e.StartTime).TotalMinutes))
                };
            })
            .ToList();

        var focusUsageStats = filteredEvents
            .Where(e => e.TrainingPlan != null)
            .SelectMany(e => e.TrainingPlan!.Items)
            .Where(item => item.Exercise != null)
            .SelectMany(item => item.Exercise.ExerciseTags)
            .Where(et => et.Tag.Type == TagType.Focus)
            .GroupBy(et => et.Tag.Name)
            .Select(g => new FocusUsageStatDto
            {
                FocusName = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ThenBy(x => x.FocusName)
            .ToList();

        var result = new StatisticsSummaryDto
        {
            WeeklyCount = weeklyCount,
            MonthlyCount = monthlyCount,
            TotalEvents = filteredEvents.Count,
            TotalDurationMinutes = totalDurationMinutes,
            TopPlans = topPlans,
            WeeklyDailyStats = weeklyDailyStats,
            FocusUsageStats = focusUsageStats
        };

        return Ok(result);
    }
}