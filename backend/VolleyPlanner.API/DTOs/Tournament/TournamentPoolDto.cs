namespace VolleyPlanner.API.DTOs.Tournament;

public class TournamentPoolDto
{
    public int PoolId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int PoolNumber { get; set; }

    public List<TournamentPoolSlotDto> Slots { get; set; }
        = new();
}