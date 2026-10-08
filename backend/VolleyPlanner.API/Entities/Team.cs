namespace VolleyPlanner.API.Entities;

public class Team
{
    public int Id { get; set; }

    public int Player1UserId { get; set; }

    public int Player2UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
        = DateTime.UtcNow;

    public User Player1User { get; set; } = null!;

    public User Player2User { get; set; } = null!;

    public ICollection<TournamentEntry> TournamentEntries { get; set; }
        = new List<TournamentEntry>();
}