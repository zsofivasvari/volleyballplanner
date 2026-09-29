namespace VolleyPlanner.API.DTOs.TrainingBooking;

public class TrainingBookingDto
{
    public int Id { get; set; }

    public int TrainingSessionId { get; set; }

    public string TrainingSessionTitle { get; set; } = string.Empty;

    public int UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public int? WaitlistPosition { get; set; }
}