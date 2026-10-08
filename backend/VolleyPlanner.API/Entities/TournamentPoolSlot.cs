namespace VolleyPlanner.API.Entities;

public class TournamentPoolSlot
{
    public int Id { get; set; }

    public int TournamentPoolId { get; set; }

    public int? TournamentEntryId { get; set; }

    // 1-4 közötti random sorsolási pozíció
    public int SlotNumber { get; set; }

    // A csoport végeredménye:
    // 1, 2, 3 vagy 4.
    // A csoport elején még null.
    public int? FinalRank { get; set; }

    public TournamentPool TournamentPool { get; set; } = null!;

    public TournamentEntry? TournamentEntry { get; set; }
}