namespace VolleyPlanner.API.DTOs.TrainingSession;

public class TrainingSessionDetailsDto
{
    public int Id { get; set; }

    public int OrganizerUserId { get; set; }

    public string OrganizerName { get; set; } = string.Empty;

    public int? TrainingPlanId { get; set; }

    public string? TrainingPlanTitle { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string SportType { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public string Location { get; set; } = string.Empty;

    public int MaxParticipants { get; set; }

    public int ParticipantCount { get; set; }

    public int WaitlistCount { get; set; }

    public bool IsFull { get; set; }

    public string TargetLevel { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public string? CurrentUserBookingStatus { get; set; }

    public int? WaitlistPosition { get; set; }

    public bool IsInCurrentUserCalendar { get; set; }
}