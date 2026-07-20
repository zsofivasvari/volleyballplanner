namespace VolleyPlanner.API.DTOs.Generation;

public class GenerateTrainingPlanRequestDto
{
    public List<string> SportTypes { get; set; } = new();
    public int DurationMin { get; set; }
    public int? PlayerCount { get; set; }
    public List<string> Intensities { get; set; } = new();
    public List<string> Difficulties { get; set; } = new();
    public List<string> FocusAreas { get; set; } = new();
}