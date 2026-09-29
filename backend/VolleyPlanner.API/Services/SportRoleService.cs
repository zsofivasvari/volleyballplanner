using Microsoft.EntityFrameworkCore;
using VolleyPlanner.API.Data;
using VolleyPlanner.API.DTOs.SportRole;
using VolleyPlanner.API.Entities;
using VolleyPlanner.API.Enums;
using VolleyPlanner.API.Interfaces;

namespace VolleyPlanner.API.Services;

public class SportRoleService : ISportRoleService
{
    private readonly AppDbContext _context;

    public SportRoleService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SportRolesResponseDto> GetUserSportRolesAsync(int userId)
    {
        var userExists = await _context.Users
            .AnyAsync(u => u.Id == userId);

        if (!userExists)
        {
            throw new Exception("A felhasználó nem található.");
        }

        var roles = await _context.UserSportRoles
            .Where(usr => usr.UserId == userId)
            .Select(usr => usr.RoleType)
            .ToListAsync();

        return new SportRolesResponseDto
        {
            IsPlayer = roles.Contains(SportRoleType.Player),
            IsOrganizerCoach = roles.Contains(SportRoleType.OrganizerCoach)
        };
    }

    public async Task<SportRolesResponseDto> UpdateUserSportRolesAsync(
        int userId,
        UpdateSportRolesRequestDto request)
    {
        var userExists = await _context.Users
            .AnyAsync(u => u.Id == userId);

        if (!userExists)
        {
            throw new Exception("A felhasználó nem található.");
        }

        // Pontosan egy szerepkör választható.
        // false/false és true/true sem engedélyezett.
        if (request.IsPlayer == request.IsOrganizerCoach)
        {
            throw new Exception(
                "Pontosan egy sportbeli szerepkört kell kiválasztani.");
        }

        var existingRoles = await _context.UserSportRoles
            .Where(usr => usr.UserId == userId)
            .ToListAsync();

        _context.UserSportRoles.RemoveRange(existingRoles);

        if (request.IsPlayer)
        {
            _context.UserSportRoles.Add(
                new UserSportRole
                {
                    UserId = userId,
                    RoleType = SportRoleType.Player
                });
        }
        else
        {
            _context.UserSportRoles.Add(
                new UserSportRole
                {
                    UserId = userId,
                    RoleType = SportRoleType.OrganizerCoach
                });
        }

        await _context.SaveChangesAsync();

        return new SportRolesResponseDto
        {
            IsPlayer = request.IsPlayer,
            IsOrganizerCoach = request.IsOrganizerCoach
        };
    }
}