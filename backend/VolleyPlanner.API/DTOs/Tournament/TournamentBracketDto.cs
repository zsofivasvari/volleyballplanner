namespace VolleyPlanner.API.DTOs.Tournament;

public class TournamentBracketDto
{
    public int TournamentId { get; set; }

    public string TournamentName { get; set; } = string.Empty;

    public List<TournamentMatchDto> PoolMatches { get; set; }
        = new();

    public List<TournamentMatchDto> RoundOf12Matches { get; set; }
        = new();

    public List<TournamentMatchDto> QuarterfinalMatches { get; set; }
        = new();

    public List<TournamentMatchDto> SemifinalMatches { get; set; }
        = new();

    public TournamentMatchDto? BronzeMatch { get; set; }

    public TournamentMatchDto? FinalMatch { get; set; }
}