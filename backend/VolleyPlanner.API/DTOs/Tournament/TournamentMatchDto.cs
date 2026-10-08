namespace VolleyPlanner.API.DTOs.Tournament;

public class TournamentMatchDto
{
    public int Id { get; set; }

    public int MatchNumber { get; set; }

    public string Stage { get; set; } = string.Empty;

    public int? PoolId { get; set; }

    public string? PoolName { get; set; }

    public int? Team1EntryId { get; set; }

    public string? Team1Name { get; set; }

    public int? Team2EntryId { get; set; }

    public string? Team2Name { get; set; }

    public string Team1SourceLabel { get; set; } = string.Empty;

    public string Team2SourceLabel { get; set; } = string.Empty;

    public int? Team1SetsWon { get; set; }

    public int? Team2SetsWon { get; set; }

    public int? WinnerEntryId { get; set; }

    public string? WinnerTeamName { get; set; }

    public bool IsAutomaticResult { get; set; }

    public string Status { get; set; } = string.Empty;

    public List<TournamentMatchSetDto> Sets { get; set; }
        = new();
}