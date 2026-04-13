using VolleyPlanner.API.Enums;

namespace VolleyPlanner.API.Entities;

public class TrainingPlan
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public SportType SportType { get; set; }
    public string PlanType { get; set; } = "Single";
    public int TargetDuration { get; set; }
    public string TargetIntensity { get; set; } = string.Empty;
    public string TargetLevel { get; set; } = string.Empty;
    public string PrimaryFocus { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public ICollection<TrainingPlanItem> Items { get; set; } = new List<TrainingPlanItem>();

    public ICollection<CalendarEvent> CalendarEvents { get; set; } = new List<CalendarEvent>();
}