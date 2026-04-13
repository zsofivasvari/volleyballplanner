namespace VolleyPlanner.API.DTOs.TrainingPlan;

public class CreateTrainingPlanRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string SportType { get; set; } = string.Empty;
    public string PlanType { get; set; } = "Single";
    public int TargetDuration { get; set; }
    public string TargetIntensity { get; set; } = string.Empty;
    public string TargetLevel { get; set; } = string.Empty;
    public string PrimaryFocus { get; set; } = string.Empty;
    public List<CreateTrainingPlanItemDto> Items { get; set; } = new();
}