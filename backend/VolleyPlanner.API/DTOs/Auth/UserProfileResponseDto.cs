namespace VolleyPlanner.API.DTOs.Auth;

public class UserProfileResponseDto
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Level { get; set; }
    public string? Goal { get; set; }
    public int? Age { get; set; }
    public double? Height { get; set; }
    public double? Weight { get; set; }
}