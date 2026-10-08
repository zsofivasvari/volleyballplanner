using VolleyPlanner.API.Enums;

namespace VolleyPlanner.API.Entities;

public class Tournament
{
    public int Id { get; set; }

    public int OrganizerUserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime RegistrationDeadline { get; set; }

    public int MaxTeams { get; set; } = 16;

    public TournamentFormat Format { get; set; }
        = TournamentFormat.ModifiedPoolPlay16;

    public TournamentStatus Status { get; set; }
        = TournamentStatus.RegistrationOpen;

    public DateTime CreatedAt { get; set; }
        = DateTime.UtcNow;

    public User OrganizerUser { get; set; } = null!;

    public ICollection<TournamentEntry> Entries { get; set; }
        = new List<TournamentEntry>();

    public ICollection<TournamentPool> Pools { get; set; }
        = new List<TournamentPool>();

    public ICollection<TournamentMatch> Matches { get; set; }
        = new List<TournamentMatch>();

    public ICollection<TeamInvitation> TeamInvitations { get; set; }
        = new List<TeamInvitation>();
}