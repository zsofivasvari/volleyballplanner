namespace VolleyPlanner.API.DTOs.Player;

public class PlayerSearchResponseDto
{
    public int UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string PlayerCode { get; set; } = string.Empty;

    public string? Level { get; set; }
}