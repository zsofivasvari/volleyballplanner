namespace VolleyPlanner.API.Entities;

public class UserProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? Level { get; set; }
    public string? Goal { get; set; }
    public int? Age { get; set; }
    public double? Height { get; set; }
    public double? Weight { get; set; }

    public User User { get; set; } = null!;
}