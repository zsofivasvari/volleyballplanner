using Microsoft.EntityFrameworkCore;
using VolleyPlanner.API.Data;
using VolleyPlanner.API.DTOs.TeamInvitation;
using VolleyPlanner.API.DTOs.Tournament;
using VolleyPlanner.API.Entities;
using VolleyPlanner.API.Enums;
using VolleyPlanner.API.Interfaces;

namespace VolleyPlanner.API.Services;

public class TeamInvitationService : ITeamInvitationService
{
    private readonly AppDbContext _context;

    public TeamInvitationService(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task<TeamInvitationResponseDto> CreateAsync(
        int currentUserId,
        CreateTeamInvitationRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.PlayerCode))
        {
            throw new Exception(
                "A csapattárs játékoskódjának megadása kötelező.");
        }

        var currentUser = await _context.Users
            .Include(u => u.SportRoles)
            .FirstOrDefaultAsync(u =>
                u.Id == currentUserId);

        if (currentUser == null)
        {
            throw new Exception(
                "A felhasználó nem található.");
        }

        if (!currentUser.SportRoles.Any(sr =>
                sr.RoleType == SportRoleType.Player))
        {
            throw new UnauthorizedAccessException(
                "Csak játékos küldhet csapattárs-meghívást.");
        }

        var tournament = await _context.Tournaments
            .Include(t => t.Entries)
            .FirstOrDefaultAsync(t =>
                t.Id == request.TournamentId);

        if (tournament == null)
        {
            throw new Exception(
                "A verseny nem található.");
        }

        if (tournament.Status !=
            TournamentStatus.RegistrationOpen)
        {
            throw new Exception(
                "Erre a versenyre jelenleg nem lehet nevezni.");
        }

        if (tournament.RegistrationDeadline <=
            DateTime.UtcNow)
        {
            throw new Exception(
                "A nevezési határidő lejárt.");
        }

        if (tournament.Entries.Count >=
            tournament.MaxTeams)
        {
            throw new Exception(
                "A verseny létszáma betelt.");
        }

        var normalizedCode =
            request.PlayerCode
                .Trim()
                .ToUpperInvariant();

        var invitedUser = await _context.Users
            .Include(u => u.SportRoles)
            .FirstOrDefaultAsync(u =>
                u.PlayerCode == normalizedCode);

        if (invitedUser == null)
        {
            throw new Exception(
                "Nem található ilyen játékoskóddal felhasználó.");
        }

        if (invitedUser.Id == currentUserId)
        {
            throw new Exception(
                "Saját magadat nem hívhatod meg csapattársnak.");
        }

        if (!invitedUser.SportRoles.Any(sr =>
                sr.RoleType == SportRoleType.Player))
        {
            throw new Exception(
                "A kiválasztott felhasználó nem játékos.");
        }

        var currentUserAlreadyEntered =
            await IsUserAlreadyEnteredAsync(
                request.TournamentId,
                currentUserId);

        if (currentUserAlreadyEntered)
        {
            throw new Exception(
                "Te már neveztél erre a versenyre egy csapattal.");
        }

        var invitedUserAlreadyEntered =
            await IsUserAlreadyEnteredAsync(
                request.TournamentId,
                invitedUser.Id);

        if (invitedUserAlreadyEntered)
        {
            throw new Exception(
                "A kiválasztott játékos már nevezett erre a versenyre.");
        }

        var duplicateInvitation =
            await _context.TeamInvitations
                .AnyAsync(ti =>
                    ti.TournamentId ==
                        request.TournamentId &&
                    ti.Status ==
                        TeamInvitationStatus.Pending &&
                    (
                        (
                            ti.InviterUserId ==
                                currentUserId &&
                            ti.InvitedUserId ==
                                invitedUser.Id
                        )
                        ||
                        (
                            ti.InviterUserId ==
                                invitedUser.Id &&
                            ti.InvitedUserId ==
                                currentUserId
                        )
                    ));

        if (duplicateInvitation)
        {
            throw new Exception(
                "A két játékos között már van függőben lévő meghívás erre a versenyre.");
        }

        var invitation = new TeamInvitation
        {
            TournamentId =
                request.TournamentId,

            InviterUserId =
                currentUserId,

            InvitedUserId =
                invitedUser.Id,

            Status =
                TeamInvitationStatus.Pending
        };

        _context.TeamInvitations.Add(invitation);

        await _context.SaveChangesAsync();

        return await GetInvitationDtoAsync(
            invitation.Id);
    }

    public async Task<List<TeamInvitationResponseDto>>
        GetReceivedAsync(
            int currentUserId)
    {
        return await _context.TeamInvitations
            .AsNoTracking()
            .Where(ti =>
                ti.InvitedUserId ==
                currentUserId)
            .OrderByDescending(ti =>
                ti.CreatedAt)
            .Select(ti =>
                new TeamInvitationResponseDto
                {
                    Id = ti.Id,

                    TournamentId =
                        ti.TournamentId,

                    TournamentName =
                        ti.Tournament.Name,

                    InviterUserId =
                        ti.InviterUserId,

                    InviterName =
                        ti.InviterUser.Name,

                    InviterPlayerCode =
                        ti.InviterUser.PlayerCode
                        ?? string.Empty,

                    InvitedUserId =
                        ti.InvitedUserId,

                    InvitedName =
                        ti.InvitedUser.Name,

                    InvitedPlayerCode =
                        ti.InvitedUser.PlayerCode
                        ?? string.Empty,

                    Status =
                        ti.Status.ToString(),

                    CreatedAt =
                        ti.CreatedAt,

                    RespondedAt =
                        ti.RespondedAt
                })
            .ToListAsync();
    }

    public async Task<List<TeamInvitationResponseDto>>
        GetSentAsync(
            int currentUserId)
    {
        return await _context.TeamInvitations
            .AsNoTracking()
            .Where(ti =>
                ti.InviterUserId ==
                currentUserId)
            .OrderByDescending(ti =>
                ti.CreatedAt)
            .Select(ti =>
                new TeamInvitationResponseDto
                {
                    Id = ti.Id,

                    TournamentId =
                        ti.TournamentId,

                    TournamentName =
                        ti.Tournament.Name,

                    InviterUserId =
                        ti.InviterUserId,

                    InviterName =
                        ti.InviterUser.Name,

                    InviterPlayerCode =
                        ti.InviterUser.PlayerCode
                        ?? string.Empty,

                    InvitedUserId =
                        ti.InvitedUserId,

                    InvitedName =
                        ti.InvitedUser.Name,

                    InvitedPlayerCode =
                        ti.InvitedUser.PlayerCode
                        ?? string.Empty,

                    Status =
                        ti.Status.ToString(),

                    CreatedAt =
                        ti.CreatedAt,

                    RespondedAt =
                        ti.RespondedAt
                })
            .ToListAsync();
    }

    public async Task<TournamentEntryResponseDto>
        AcceptAsync(
            int invitationId,
            int currentUserId)
    {
        var invitation =
            await _context.TeamInvitations
                .Include(ti => ti.Tournament)
                    .ThenInclude(t => t.Entries)
                .Include(ti => ti.InviterUser)
                .Include(ti => ti.InvitedUser)
                .FirstOrDefaultAsync(ti =>
                    ti.Id == invitationId);

        if (invitation == null)
        {
            throw new Exception(
                "A meghívás nem található.");
        }

        if (invitation.InvitedUserId !=
            currentUserId)
        {
            throw new UnauthorizedAccessException(
                "Ezt a meghívást csak a meghívott játékos fogadhatja el.");
        }

        if (invitation.Status !=
            TeamInvitationStatus.Pending)
        {
            throw new Exception(
                "Ez a meghívás már nem aktív.");
        }

        var tournament =
            invitation.Tournament;

        if (tournament.Status !=
            TournamentStatus.RegistrationOpen)
        {
            throw new Exception(
                "A verseny nevezése már lezárult.");
        }

        if (tournament.RegistrationDeadline <=
            DateTime.UtcNow)
        {
            throw new Exception(
                "A nevezési határidő lejárt.");
        }

        if (tournament.Entries.Count >=
            tournament.MaxTeams)
        {
            throw new Exception(
                "A verseny létszáma betelt.");
        }

        var inviterAlreadyEntered =
            await IsUserAlreadyEnteredAsync(
                tournament.Id,
                invitation.InviterUserId);

        if (inviterAlreadyEntered)
        {
            throw new Exception(
                "A meghívó játékos időközben már benevezett egy másik csapattal.");
        }

        var invitedAlreadyEntered =
            await IsUserAlreadyEnteredAsync(
                tournament.Id,
                invitation.InvitedUserId);

        if (invitedAlreadyEntered)
        {
            throw new Exception(
                "Már neveztél erre a versenyre egy másik csapattal.");
        }

        var player1Id =
            invitation.InviterUserId;

        var player2Id =
            invitation.InvitedUserId;

        var team = await _context.Teams
            .FirstOrDefaultAsync(t =>
                (
                    t.Player1UserId == player1Id &&
                    t.Player2UserId == player2Id
                )
                ||
                (
                    t.Player1UserId == player2Id &&
                    t.Player2UserId == player1Id
                ));

        if (team == null)
        {
            team = new Team
            {
                Player1UserId =
                    player1Id,

                Player2UserId =
                    player2Id,

                Name =
                    $"{invitation.InviterUser.Name} / {invitation.InvitedUser.Name}"
            };

            _context.Teams.Add(team);

            await _context.SaveChangesAsync();
        }

        var entry = new TournamentEntry
        {
            TournamentId =
                tournament.Id,

            TeamId =
                team.Id
        };

        _context.TournamentEntries.Add(entry);

        invitation.Status =
            TeamInvitationStatus.Accepted;

        invitation.RespondedAt =
            DateTime.UtcNow;

        // A két játékos többi függőben lévő meghívása
        // ezen a versenyen már nem használható.
        var otherPendingInvitations =
            await _context.TeamInvitations
                .Where(ti =>
                    ti.TournamentId ==
                        tournament.Id &&
                    ti.Id != invitation.Id &&
                    ti.Status ==
                        TeamInvitationStatus.Pending &&
                    (
                        ti.InviterUserId ==
                            player1Id ||
                        ti.InvitedUserId ==
                            player1Id ||
                        ti.InviterUserId ==
                            player2Id ||
                        ti.InvitedUserId ==
                            player2Id
                    ))
                .ToListAsync();

        foreach (var pending in
                 otherPendingInvitations)
        {
            pending.Status =
                TeamInvitationStatus.Cancelled;

            pending.RespondedAt =
                DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return new TournamentEntryResponseDto
        {
            EntryId =
                entry.Id,

            TournamentId =
                tournament.Id,

            TournamentName =
                tournament.Name,

            TeamId =
                team.Id,

            TeamName =
                team.Name,

            Player1UserId =
                invitation.InviterUser.Id,

            Player1Name =
                invitation.InviterUser.Name,

            Player2UserId =
                invitation.InvitedUser.Id,

            Player2Name =
                invitation.InvitedUser.Name,

            RegisteredAt =
                entry.RegisteredAt
        };
    }

    public async Task<TeamInvitationResponseDto>
        RejectAsync(
            int invitationId,
            int currentUserId)
    {
        var invitation =
            await _context.TeamInvitations
                .FirstOrDefaultAsync(ti =>
                    ti.Id == invitationId);

        if (invitation == null)
        {
            throw new Exception(
                "A meghívás nem található.");
        }

        if (invitation.InvitedUserId !=
            currentUserId)
        {
            throw new UnauthorizedAccessException(
                "Ezt a meghívást csak a meghívott játékos utasíthatja el.");
        }

        if (invitation.Status !=
            TeamInvitationStatus.Pending)
        {
            throw new Exception(
                "Ez a meghívás már nem aktív.");
        }

        invitation.Status =
            TeamInvitationStatus.Rejected;

        invitation.RespondedAt =
            DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetInvitationDtoAsync(
            invitation.Id);
    }

    private async Task<bool>
        IsUserAlreadyEnteredAsync(
            int tournamentId,
            int userId)
    {
        return await _context.TournamentEntries
            .AnyAsync(te =>
                te.TournamentId ==
                    tournamentId &&
                (
                    te.Team.Player1UserId ==
                        userId ||
                    te.Team.Player2UserId ==
                        userId
                ));
    }

    private async Task<TeamInvitationResponseDto>
        GetInvitationDtoAsync(
            int invitationId)
    {
        var invitation =
            await _context.TeamInvitations
                .AsNoTracking()
                .Where(ti =>
                    ti.Id == invitationId)
                .Select(ti =>
                    new TeamInvitationResponseDto
                    {
                        Id = ti.Id,

                        TournamentId =
                            ti.TournamentId,

                        TournamentName =
                            ti.Tournament.Name,

                        InviterUserId =
                            ti.InviterUserId,

                        InviterName =
                            ti.InviterUser.Name,

                        InviterPlayerCode =
                            ti.InviterUser.PlayerCode
                            ?? string.Empty,

                        InvitedUserId =
                            ti.InvitedUserId,

                        InvitedName =
                            ti.InvitedUser.Name,

                        InvitedPlayerCode =
                            ti.InvitedUser.PlayerCode
                            ?? string.Empty,

                        Status =
                            ti.Status.ToString(),

                        CreatedAt =
                            ti.CreatedAt,

                        RespondedAt =
                            ti.RespondedAt
                    })
                .FirstOrDefaultAsync();

        if (invitation == null)
        {
            throw new Exception(
                "A meghívás nem található.");
        }

        return invitation;
    }
}