namespace VolleyPlanner.API.Entities;

public class TournamentEntry
{
    public int Id { get; set; }

    public int TournamentId { get; set; }

    public int TeamId { get; set; }

    public DateTime RegisteredAt { get; set; }
        = DateTime.UtcNow;

    public Tournament Tournament { get; set; } = null!;

    public Team Team { get; set; } = null!;

    public ICollection<TournamentPoolSlot> PoolSlots { get; set; }
        = new List<TournamentPoolSlot>();

    public ICollection<TournamentMatch> MatchesAsTeam1 { get; set; }
        = new List<TournamentMatch>();

    public ICollection<TournamentMatch> MatchesAsTeam2 { get; set; }
        = new List<TournamentMatch>();
}