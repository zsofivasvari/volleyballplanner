namespace VolleyPlanner.API.DTOs.Tournament;

public class TournamentEntryResponseDto
{
    public int EntryId { get; set; }

    public int TournamentId { get; set; }

    public string TournamentName { get; set; } = string.Empty;

    public int TeamId { get; set; }

    public string TeamName { get; set; } = string.Empty;

    public int Player1UserId { get; set; }

    public string Player1Name { get; set; } = string.Empty;

    public int Player2UserId { get; set; }

    public string Player2Name { get; set; } = string.Empty;

    public DateTime RegisteredAt { get; set; }
}