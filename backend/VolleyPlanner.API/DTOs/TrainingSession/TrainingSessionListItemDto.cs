namespace VolleyPlanner.API.DTOs.TrainingSession;

public class TrainingSessionListItemDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string SportType { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public string Location { get; set; } = string.Empty;

    public int MaxParticipants { get; set; }

    public string TargetLevel { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public int OrganizerUserId { get; set; }

    public string OrganizerName { get; set; } = string.Empty;
}