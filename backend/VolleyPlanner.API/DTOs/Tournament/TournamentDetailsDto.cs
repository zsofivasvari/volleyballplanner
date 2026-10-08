namespace VolleyPlanner.API.DTOs.Tournament;

public class TournamentDetailsDto
{
    public int Id { get; set; }

    public int OrganizerUserId { get; set; }

    public string OrganizerName { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime RegistrationDeadline { get; set; }

    public int MaxTeams { get; set; }

    public int RegisteredTeams { get; set; }

    public string Format { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}