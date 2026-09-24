using VolleyPlanner.API.DTOs.SportRole;

namespace VolleyPlanner.API.Interfaces;

public interface ISportRoleService
{
    Task<SportRolesResponseDto> GetUserSportRolesAsync(int userId);

    Task<SportRolesResponseDto> UpdateUserSportRolesAsync(
        int userId,
        UpdateSportRolesRequestDto request);
}