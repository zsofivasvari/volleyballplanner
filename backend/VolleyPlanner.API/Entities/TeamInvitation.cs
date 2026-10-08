using VolleyPlanner.API.Enums;

namespace VolleyPlanner.API.Entities;

public class TeamInvitation
{
    public int Id { get; set; }

    public int TournamentId { get; set; }

    public int InviterUserId { get; set; }

    public int InvitedUserId { get; set; }

    public TeamInvitationStatus Status { get; set; }
        = TeamInvitationStatus.Pending;

    public DateTime CreatedAt { get; set; }
        = DateTime.UtcNow;

    public DateTime? RespondedAt { get; set; }

    public Tournament Tournament { get; set; } = null!;

    public User InviterUser { get; set; } = null!;

    public User InvitedUser { get; set; } = null!;
}