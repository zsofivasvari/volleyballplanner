using VolleyPlanner.API.Enums;

namespace VolleyPlanner.API.Entities;

public class CalendarEvent
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int TrainingPlanId { get; set; }
    public string Title { get; set; } = string.Empty;
    public SportType SportType { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = "Planned";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public TrainingPlan TrainingPlan { get; set; } = null!;
}