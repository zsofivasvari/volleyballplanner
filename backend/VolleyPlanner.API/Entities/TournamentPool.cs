namespace VolleyPlanner.API.Entities;

public class TournamentPool
{
    public int Id { get; set; }

    public int TournamentId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int PoolNumber { get; set; }

    public Tournament Tournament { get; set; } = null!;

    public ICollection<TournamentPoolSlot> Slots { get; set; }
        = new List<TournamentPoolSlot>();

    public ICollection<TournamentMatch> Matches { get; set; }
        = new List<TournamentMatch>();
}