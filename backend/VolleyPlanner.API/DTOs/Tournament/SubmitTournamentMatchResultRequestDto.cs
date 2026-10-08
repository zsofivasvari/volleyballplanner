namespace VolleyPlanner.API.DTOs.Tournament;

public class SubmitTournamentMatchResultRequestDto
{
    public List<TournamentMatchSetResultDto> Sets { get; set; }
        = new();
}

public class TournamentMatchSetResultDto
{
    public int Team1Points { get; set; }

    public int Team2Points { get; set; }
}