using VolleyPlanner.API.DTOs.Player;

namespace VolleyPlanner.API.Interfaces;

public interface IPlayerService
{
    Task<PlayerSearchResponseDto?> GetPlayerByCodeAsync(
        string playerCode,
        int currentUserId);
}