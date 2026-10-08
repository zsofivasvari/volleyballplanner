using VolleyPlanner.API.DTOs.Tournament;

namespace VolleyPlanner.API.Interfaces;

public interface ITournamentService
{
    Task<TournamentDetailsDto> CreateAsync(
        int organizerUserId,
        CreateTournamentRequestDto request);

    Task<List<TournamentListItemDto>> GetAllAsync();

    Task<List<TournamentListItemDto>> GetMineAsync(
        int organizerUserId);

    Task<TournamentDetailsDto?> GetByIdAsync(
        int tournamentId);

    Task<List<TournamentEntryListItemDto>> GetEntriesAsync(
        int tournamentId);

    Task<TournamentDrawResponseDto> GeneratePoolDrawAsync(
        int tournamentId,
        int currentUserId);

    Task<TournamentMatchResultResponseDto> SubmitMatchResultAsync(
        int tournamentId,
        int matchId,
        int currentUserId,
        SubmitTournamentMatchResultRequestDto request);

    Task<List<TournamentMatchDto>> GetMatchesAsync(
        int tournamentId);

    Task<TournamentBracketDto> GetBracketAsync(
        int tournamentId);

    Task<TournamentFinalResultDto> GetFinalResultAsync(
        int tournamentId);
}