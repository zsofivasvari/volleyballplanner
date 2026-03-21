namespace VolleyPlanner.API.DTOs.Exercise;

public class ExerciseQueryDto
{
    public string? SportType { get; set; }
    public string? Difficulty { get; set; }
    public string? Intensity { get; set; }
    public string? Phase { get; set; }
    public int? MinPlayers { get; set; }
    public int? MaxPlayers { get; set; }
}