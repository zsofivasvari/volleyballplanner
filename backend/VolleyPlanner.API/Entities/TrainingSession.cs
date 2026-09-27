using VolleyPlanner.API.Enums;

namespace VolleyPlanner.API.Entities;

public class TrainingSession
{
    public int Id { get; set; }

    public int OrganizerUserId { get; set; }

    public int? TrainingPlanId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public SportType SportType { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public string Location { get; set; } = string.Empty;

    public int MaxParticipants { get; set; }

    public string TargetLevel { get; set; } = string.Empty;

    public string Status { get; set; } = "Open";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User OrganizerUser { get; set; } = null!;

    public TrainingPlan? TrainingPlan { get; set; }
}