namespace VolleyPlanner.API.Entities;

public class TournamentMatchSet
{
    public int Id { get; set; }

    public int TournamentMatchId { get; set; }

    public int SetNumber { get; set; }

    public int Team1Points { get; set; }

    public int Team2Points { get; set; }

    public TournamentMatch TournamentMatch { get; set; } = null!;
}