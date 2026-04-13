namespace VolleyPlanner.API.DTOs.Statistics;

public class PlanUsageStatDto
{
    public int TrainingPlanId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Count { get; set; }
}