namespace VolleyPlanner.API.DTOs.Generation;

public class GeneratedTrainingPlanResponseDto
{
    public string SportType { get; set; } = string.Empty;
    public int TargetDuration { get; set; }
    public string Difficulty { get; set; } = string.Empty;
    public string Intensity { get; set; } = string.Empty;
    public List<string> FocusAreas { get; set; } = new();
    public List<GeneratedTrainingPlanItemDto> Items { get; set; } = new();
}