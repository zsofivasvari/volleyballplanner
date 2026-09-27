namespace VolleyPlanner.API.DTOs.TrainingSession;

public class CreateTrainingSessionRequestDto
{
    public int? TrainingPlanId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string SportType { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public string Location { get; set; } = string.Empty;

    public int MaxParticipants { get; set; }

    public string TargetLevel { get; set; } = string.Empty;
}