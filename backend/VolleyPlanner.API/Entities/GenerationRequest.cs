namespace VolleyPlanner.API.Entities;

public class GenerationRequest
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string SportType { get; set; } = string.Empty;
    public int DurationMin { get; set; }
    public int PlayerCount { get; set; }
    public string Intensity { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string PrimaryFocus { get; set; } = string.Empty;
    public string AvailableEquipment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}