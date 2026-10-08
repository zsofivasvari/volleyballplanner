using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolleyPlanner.API.Data;
using VolleyPlanner.API.Entities;
using VolleyPlanner.API.Enums;

namespace VolleyPlanner.API.Controllers;

[ApiController]
[Route("api/dev")]
public class DevTestDataController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public DevTestDataController(
        AppDbContext context,
        IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [HttpPost("tournaments/{tournamentId:int}/seed-12-teams")]
    public async Task<IActionResult> SeedTournament(
        int tournamentId)
    {
        // Ez az endpoint csak Development környezetben működhet.
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        var tournament =
            await _context.Tournaments
                .Include(t => t.Entries)
                .FirstOrDefaultAsync(t =>
                    t.Id == tournamentId);

        if (tournament == null)
        {
            return NotFound(new
            {
                message = "A verseny nem található."
            });
        }

        if (tournament.Status !=
            TournamentStatus.RegistrationOpen)
        {
            return BadRequest(new
            {
                message =
                    "Csak nyitott nevezési állapotú verseny tölthető fel tesztadatokkal."
            });
        }

        if (tournament.Entries.Any())
        {
            return BadRequest(new
            {
                message =
                    "A tesztadat-generátor csak üres versenyen használható."
            });
        }

        if (tournament.MaxTeams < 12)
        {
            return BadRequest(new
            {
                message =
                    "A verseny MaxTeams értékének legalább 12-nek kell lennie."
            });
        }

        var testRunId =
            Guid.NewGuid()
                .ToString("N")[..6]
                .ToUpperInvariant();

        var createdUsers =
            new List<User>();

        var createdTeams =
            new List<Team>();

        for (var teamNumber = 1;
             teamNumber <= 12;
             teamNumber++)
        {
            var player1 =
                new User
                {
                    Name =
                        $"Test Player {teamNumber:00}A",

                    Email =
                        $"test-{testRunId}-{teamNumber:00}a@volleymind.local",

                    PasswordHash =
                        BCrypt.Net.BCrypt.HashPassword(
                            "Test123!"),

                    IsEmailConfirmed =
                        true,

                    PlayerCode =
                        $"T{testRunId}{teamNumber:00}A",

                    Role =
                        "User"
                };

            var player2 =
                new User
                {
                    Name =
                        $"Test Player {teamNumber:00}B",

                    Email =
                        $"test-{testRunId}-{teamNumber:00}b@volleymind.local",

                    PasswordHash =
                        BCrypt.Net.BCrypt.HashPassword(
                            "Test123!"),

                    IsEmailConfirmed =
                        true,

                    PlayerCode =
                        $"T{testRunId}{teamNumber:00}B",

                    Role =
                        "User"
                };

            _context.Users.Add(player1);
            _context.Users.Add(player2);

            await _context.SaveChangesAsync();

            _context.UserSportRoles.Add(
                new UserSportRole
                {
                    UserId = player1.Id,
                    RoleType =
                        SportRoleType.Player
                });

            _context.UserSportRoles.Add(
                new UserSportRole
                {
                    UserId = player2.Id,
                    RoleType =
                        SportRoleType.Player
                });

            await _context.SaveChangesAsync();

            var team =
                new Team
                {
                    Player1UserId =
                        player1.Id,

                    Player2UserId =
                        player2.Id,

                    Name =
                        $"Test Team {teamNumber:00}"
                };

            _context.Teams.Add(team);

            await _context.SaveChangesAsync();

            var entry =
                new TournamentEntry
                {
                    TournamentId =
                        tournament.Id,

                    TeamId =
                        team.Id
                };

            _context.TournamentEntries.Add(
                entry);

            await _context.SaveChangesAsync();

            createdUsers.Add(player1);
            createdUsers.Add(player2);
            createdTeams.Add(team);
        }

        return Ok(new
        {
            message =
                "12 tesztcsapat sikeresen létrehozva és benevezve.",

            tournamentId =
                tournament.Id,

            createdPlayers =
                createdUsers.Count,

            createdTeams =
                createdTeams.Count,

            teams =
                createdTeams.Select(t =>
                    new
                    {
                        t.Id,
                        t.Name,
                        t.Player1UserId,
                        t.Player2UserId
                    })
        });
    }
}