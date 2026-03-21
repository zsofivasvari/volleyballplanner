namespace VolleyPlanner.API.DTOs.Exercise;

public class ExerciseDetailsDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string SportType { get; set; } = string.Empty;
    public int DurationMin { get; set; }
    public string Difficulty { get; set; } = string.Empty;
    public string Intensity { get; set; } = string.Empty;
    public int MinPlayers { get; set; }
    public int MaxPlayers { get; set; }
    public string Phase { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
}