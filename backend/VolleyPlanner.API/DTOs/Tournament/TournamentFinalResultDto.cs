namespace VolleyPlanner.API.DTOs.Tournament;

public class TournamentFinalResultDto
{
    public int TournamentId { get; set; }

    public string TournamentName { get; set; } = string.Empty;

    public int FirstPlaceEntryId { get; set; }
    public string FirstPlaceTeamName { get; set; } = string.Empty;

    public int SecondPlaceEntryId { get; set; }
    public string SecondPlaceTeamName { get; set; } = string.Empty;

    public int ThirdPlaceEntryId { get; set; }
    public string ThirdPlaceTeamName { get; set; } = string.Empty;

    public int FourthPlaceEntryId { get; set; }
    public string FourthPlaceTeamName { get; set; } = string.Empty;
}