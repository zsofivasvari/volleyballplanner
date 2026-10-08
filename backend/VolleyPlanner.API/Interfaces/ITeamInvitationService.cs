using VolleyPlanner.API.DTOs.TeamInvitation;
using VolleyPlanner.API.DTOs.Tournament;

namespace VolleyPlanner.API.Interfaces;

public interface ITeamInvitationService
{
    Task<TeamInvitationResponseDto> CreateAsync(
        int currentUserId,
        CreateTeamInvitationRequestDto request);

    Task<List<TeamInvitationResponseDto>> GetReceivedAsync(
        int currentUserId);

    Task<List<TeamInvitationResponseDto>> GetSentAsync(
        int currentUserId);

    Task<TournamentEntryResponseDto> AcceptAsync(
        int invitationId,
        int currentUserId);

    Task<TeamInvitationResponseDto> RejectAsync(
        int invitationId,
        int currentUserId);
}