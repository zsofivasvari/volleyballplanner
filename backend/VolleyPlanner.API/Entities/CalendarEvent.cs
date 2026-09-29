using VolleyPlanner.API.Enums;

namespace VolleyPlanner.API.Entities;

public class CalendarEvent
{
    public int Id { get; set; }

    public int UserId { get; set; }

    // Saját, manuálisan betervezett edzéstervnél kötelező,
    // TrainingSessionből létrehozott eseménynél opcionális.
    public int? TrainingPlanId { get; set; }

    // Ha egy meghirdetett edzésből került a naptárba,
    // akkor ez az eredeti TrainingSession azonosítója.
    public int? TrainingSessionId { get; set; }

    public string Title { get; set; } = string.Empty;

    public SportType SportType { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public string Status { get; set; } = "Planned";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;

    public TrainingPlan? TrainingPlan { get; set; }

    public TrainingSession? TrainingSession { get; set; }
}