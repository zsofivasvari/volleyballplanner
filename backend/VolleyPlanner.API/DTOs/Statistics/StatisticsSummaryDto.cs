namespace VolleyPlanner.API.DTOs.Statistics;

public class StatisticsSummaryDto
{
    public int WeeklyCount { get; set; }
    public int MonthlyCount { get; set; }
    public int TotalEvents { get; set; }
    public int TotalDurationMinutes { get; set; }

    public List<PlanUsageStatDto> TopPlans { get; set; } = new();
    public List<DailyTrainingStatDto> WeeklyDailyStats { get; set; } = new();
    public List<FocusUsageStatDto> FocusUsageStats { get; set; } = new();
}