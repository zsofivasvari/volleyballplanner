namespace VolleyPlanner.API.DTOs.Tournament;

public class TournamentMatchResultResponseDto
{
    public int MatchId { get; set; }

    public int MatchNumber { get; set; }

    public string Stage { get; set; } = string.Empty;

    public int Team1EntryId { get; set; }

    public string Team1Name { get; set; } = string.Empty;

    public int Team2EntryId { get; set; }

    public string Team2Name { get; set; } = string.Empty;

    public int Team1SetsWon { get; set; }

    public int Team2SetsWon { get; set; }

    public int WinnerEntryId { get; set; }

    public string WinnerTeamName { get; set; } = string.Empty;

    public int LoserEntryId { get; set; }

    public string LoserTeamName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}