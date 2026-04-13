namespace VolleyPlanner.API.DTOs.Planner;

public class CreateCalendarEventRequestDto
{
    public int TrainingPlanId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}