using VolleyPlanner.API.Enums;

namespace VolleyPlanner.API.Entities;

public class Exercise
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SportType SportType { get; set; }
    public int DurationMin { get; set; }
    public DifficultyLevel Difficulty { get; set; }
    public IntensityLevel Intensity { get; set; }
    public int MinPlayers { get; set; }
    public int MaxPlayers { get; set; }
    public ExercisePhase Phase { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ExerciseTag> ExerciseTags { get; set; } = new List<ExerciseTag>();
}