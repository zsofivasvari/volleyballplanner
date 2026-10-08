namespace VolleyPlanner.API.DTOs.Tournament;

public class TournamentPoolSlotDto
{
    public int SlotNumber { get; set; }

    public int? EntryId { get; set; }

    public int? TeamId { get; set; }

    public string? TeamName { get; set; }

    public bool IsBye { get; set; }

    public int? FinalRank { get; set; }
}