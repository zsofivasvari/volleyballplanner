namespace VolleyPlanner.API.DTOs.Tournament;

public class CreateTournamentRequestDto
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime RegistrationDeadline { get; set; }

    public int MaxTeams { get; set; } = 16;
}