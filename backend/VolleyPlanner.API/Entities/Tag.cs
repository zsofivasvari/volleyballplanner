using VolleyPlanner.API.Enums;

namespace VolleyPlanner.API.Entities;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public TagType Type { get; set; }

    public ICollection<ExerciseTag> ExerciseTags { get; set; } = new List<ExerciseTag>();
}