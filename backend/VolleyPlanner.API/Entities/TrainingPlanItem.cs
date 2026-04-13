namespace VolleyPlanner.API.Entities;

public class TrainingPlanItem
{
    public int Id { get; set; }
    public int TrainingPlanId { get; set; }
    public int ExerciseId { get; set; }
    public int OrderIndex { get; set; }
    public string SectionName { get; set; } = string.Empty;
    public int PlannedDuration { get; set; }

    public TrainingPlan TrainingPlan { get; set; } = null!;
    public Exercise Exercise { get; set; } = null!;
}