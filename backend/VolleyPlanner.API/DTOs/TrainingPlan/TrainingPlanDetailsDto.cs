namespace VolleyPlanner.API.DTOs.TrainingPlan;

public class TrainingPlanDetailsDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string SportType { get; set; } = string.Empty;
    public string PlanType { get; set; } = string.Empty;
    public int TargetDuration { get; set; }
    public string TargetIntensity { get; set; } = string.Empty;
    public string TargetLevel { get; set; } = string.Empty;
    public string PrimaryFocus { get; set; } = string.Empty;
    public int? PlayerCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<TrainingPlanDetailsItemDto> Items { get; set; } = new();
}