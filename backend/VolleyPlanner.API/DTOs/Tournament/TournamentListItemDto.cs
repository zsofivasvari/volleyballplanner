namespace VolleyPlanner.API.DTOs.Tournament;

public class TournamentListItemDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime RegistrationDeadline { get; set; }

    public int MaxTeams { get; set; }

    public int RegisteredTeams { get; set; }

    public string Status { get; set; } = string.Empty;

    public string OrganizerName { get; set; } = string.Empty;
}