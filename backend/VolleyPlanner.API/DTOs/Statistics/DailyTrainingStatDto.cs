namespace VolleyPlanner.API.DTOs.Statistics;

public class DailyTrainingStatDto
{
    public string DayLabel { get; set; } = string.Empty;
    public int Count { get; set; }
    public int DurationMinutes { get; set; }
}