namespace VolleyPlanner.API.DTOs.TeamInvitation;

public class CreateTeamInvitationRequestDto
{
    public int TournamentId { get; set; }

    public string PlayerCode { get; set; } = string.Empty;
}