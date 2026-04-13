namespace VolleyPlanner.API.DTOs.TrainingPlan;

public class TrainingPlanDetailsItemDto
{
    public int ExerciseId { get; set; }
    public string ExerciseTitle { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public string SectionName { get; set; } = string.Empty;
    public int PlannedDuration { get; set; }
}