using Microsoft.EntityFrameworkCore;
using VolleyPlanner.API.Data;
using VolleyPlanner.API.DTOs.Player;
using VolleyPlanner.API.Enums;
using VolleyPlanner.API.Interfaces;

namespace VolleyPlanner.API.Services;

public class PlayerService : IPlayerService
{
    private readonly AppDbContext _context;

    public PlayerService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PlayerSearchResponseDto?> GetPlayerByCodeAsync(
        string playerCode,
        int currentUserId)
    {
        if (string.IsNullOrWhiteSpace(playerCode))
        {
            throw new Exception(
                "A játékoskód megadása kötelező.");
        }

        var normalizedCode =
            playerCode.Trim().ToUpperInvariant();

        var player = await _context.Users
            .Where(u =>
                u.Id != currentUserId &&
                u.PlayerCode == normalizedCode &&
                u.SportRoles.Any(sr =>
                    sr.RoleType == SportRoleType.Player))
            .Select(u => new PlayerSearchResponseDto
            {
                UserId = u.Id,
                Name = u.Name,
                PlayerCode = u.PlayerCode!,
                Level = u.Profile != null
                    ? u.Profile.Level
                    : null
            })
            .FirstOrDefaultAsync();

        return player;
    }
}