namespace VolleyPlanner.API.Entities;

public class TrainingBooking
{
    public int Id { get; set; }

    public int TrainingSessionId { get; set; }

    public int UserId { get; set; }

    public string Status { get; set; } = "Confirmed";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public TrainingSession TrainingSession { get; set; } = null!;

    public User User { get; set; } = null!;
}