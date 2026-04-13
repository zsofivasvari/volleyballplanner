namespace VolleyPlanner.API.DTOs.Generation;

public class GeneratedTrainingPlanItemDto
{
    public int ExerciseId { get; set; }
    public string ExerciseTitle { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public int PlannedDuration { get; set; }
}