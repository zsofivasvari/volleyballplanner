namespace VolleyPlanner.API.DTOs.Auth;

public class UpdateUserProfileRequestDto
{
    public string? Level { get; set; }

    public string? Goal { get; set; }

    public int? Age { get; set; }

    public double? Height { get; set; }

    public double? Weight { get; set; }
}