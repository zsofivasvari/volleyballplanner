namespace VolleyPlanner.API.DTOs.Auth;

public class RegisterResponseDto
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}