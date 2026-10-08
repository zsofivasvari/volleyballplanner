namespace VolleyPlanner.API.DTOs.TeamInvitation;

public class TeamInvitationResponseDto
{
    public int Id { get; set; }

    public int TournamentId { get; set; }

    public string TournamentName { get; set; } = string.Empty;

    public int InviterUserId { get; set; }

    public string InviterName { get; set; } = string.Empty;

    public string InviterPlayerCode { get; set; } = string.Empty;

    public int InvitedUserId { get; set; }

    public string InvitedName { get; set; } = string.Empty;

    public string InvitedPlayerCode { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? RespondedAt { get; set; }
}