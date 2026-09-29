namespace VolleyPlanner.API.DTOs.TrainingBooking;

public class TrainingParticipantDto
{
    public int UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime RegisteredAt { get; set; }

    public int? WaitlistPosition { get; set; }
}