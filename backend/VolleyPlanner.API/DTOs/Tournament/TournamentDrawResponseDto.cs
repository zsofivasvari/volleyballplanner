namespace VolleyPlanner.API.DTOs.Tournament;

public class TournamentDrawResponseDto
{
    public int TournamentId { get; set; }

    public string TournamentName { get; set; } = string.Empty;

    public List<TournamentPoolDto> Pools { get; set; }
        = new();

    public int CreatedMatches { get; set; }
}