namespace VolleyPlanner.API.DTOs.Exercise;

public class PagedExercisesResponseDto
{
    public List<ExerciseListItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}