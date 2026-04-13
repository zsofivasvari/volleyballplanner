namespace VolleyPlanner.API.DTOs.TrainingPlan;

public class TrainingPlanListItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string SportType { get; set; } = string.Empty;
    public int TargetDuration { get; set; }
    public string PrimaryFocus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}