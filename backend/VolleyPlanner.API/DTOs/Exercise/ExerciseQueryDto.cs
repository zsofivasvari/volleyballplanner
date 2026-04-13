namespace VolleyPlanner.API.DTOs.Exercise;

public class ExerciseQueryDto
{
    public List<string>? SportTypes { get; set; }
    public List<string>? Difficulties { get; set; }
    public List<string>? Intensities { get; set; }
    public List<string>? Phases { get; set; }

    public int? MinPlayers { get; set; }
    public int? MaxPlayers { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 8;
}