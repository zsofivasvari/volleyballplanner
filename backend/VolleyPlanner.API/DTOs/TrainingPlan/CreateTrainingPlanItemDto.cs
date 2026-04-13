namespace VolleyPlanner.API.DTOs.TrainingPlan;

public class CreateTrainingPlanItemDto
{
    public int ExerciseId { get; set; }
    public int OrderIndex { get; set; }
    public string SectionName { get; set; } = string.Empty;
    public int PlannedDuration { get; set; }
}