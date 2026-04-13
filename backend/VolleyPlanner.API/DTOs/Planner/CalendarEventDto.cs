namespace VolleyPlanner.API.DTOs.Planner;

public class CalendarEventDto
{
    public int Id { get; set; }
    public int TrainingPlanId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string SportType { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
}