using Microsoft.EntityFrameworkCore;
using VolleyPlanner.API.Data;
using VolleyPlanner.API.DTOs.Tournament;
using VolleyPlanner.API.Entities;
using VolleyPlanner.API.Enums;
using VolleyPlanner.API.Interfaces;

namespace VolleyPlanner.API.Services;

public class TournamentService : ITournamentService
{
    private readonly AppDbContext _context;

    public TournamentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TournamentDetailsDto> CreateAsync(
        int organizerUserId,
        CreateTournamentRequestDto request)
    {
        var organizer = await _context.Users
            .Include(u => u.SportRoles)
            .FirstOrDefaultAsync(u =>
                u.Id == organizerUserId);

        if (organizer == null)
        {
            throw new Exception(
                "A felhasználó nem található.");
        }

        var isOrganizerCoach =
            organizer.SportRoles.Any(sr =>
                sr.RoleType ==
                SportRoleType.OrganizerCoach);

        if (!isOrganizerCoach)
        {
            throw new UnauthorizedAccessException(
                "Csak szervező/edző hozhat létre versenyt.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new Exception(
                "A verseny neve kötelező.");
        }

        if (string.IsNullOrWhiteSpace(request.Location))
        {
            throw new Exception(
                "A helyszín megadása kötelező.");
        }

        if (request.StartDate <= DateTime.UtcNow)
        {
            throw new Exception(
                "A verseny kezdési időpontjának a jövőben kell lennie.");
        }

        if (request.EndDate.HasValue &&
            request.EndDate.Value < request.StartDate)
        {
            throw new Exception(
                "A verseny befejezése nem lehet korábban a kezdésnél.");
        }

        if (request.RegistrationDeadline <= DateTime.UtcNow)
        {
            throw new Exception(
                "A nevezési határidőnek a jövőben kell lennie.");
        }

        if (request.RegistrationDeadline >= request.StartDate)
        {
            throw new Exception(
                "A nevezési határidőnek a verseny kezdete előtt kell lennie.");
        }

        if (request.MaxTeams < 2 ||
            request.MaxTeams > 16)
        {
            throw new Exception(
                "A csapatok maximális száma 2 és 16 között lehet.");
        }

        var tournament = new Tournament
        {
            OrganizerUserId = organizerUserId,

            Name = request.Name.Trim(),

            Description =
                request.Description?.Trim()
                ?? string.Empty,

            Location = request.Location.Trim(),

            StartDate = request.StartDate,

            EndDate = request.EndDate,

            RegistrationDeadline =
                request.RegistrationDeadline,

            MaxTeams = request.MaxTeams,

            Format =
                TournamentFormat.ModifiedPoolPlay16,

            Status =
                TournamentStatus.RegistrationOpen
        };

        _context.Tournaments.Add(tournament);

        await _context.SaveChangesAsync();

        return ToDetailsDto(
            tournament,
            organizer.Name,
            0);
    }

    public async Task<List<TournamentListItemDto>>
        GetAllAsync()
    {
        return await _context.Tournaments
            .AsNoTracking()
            .OrderBy(t => t.StartDate)
            .Select(t =>
                new TournamentListItemDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Location = t.Location,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,

                    RegistrationDeadline =
                        t.RegistrationDeadline,

                    MaxTeams = t.MaxTeams,

                    RegisteredTeams =
                        t.Entries.Count,

                    Status =
                        t.Status.ToString(),

                    OrganizerName =
                        t.OrganizerUser.Name
                })
            .ToListAsync();
    }

    public async Task<List<TournamentListItemDto>>
        GetMineAsync(
            int organizerUserId)
    {
        return await _context.Tournaments
            .AsNoTracking()
            .Where(t =>
                t.OrganizerUserId ==
                organizerUserId)
            .OrderBy(t => t.StartDate)
            .Select(t =>
                new TournamentListItemDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Location = t.Location,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,

                    RegistrationDeadline =
                        t.RegistrationDeadline,

                    MaxTeams = t.MaxTeams,

                    RegisteredTeams =
                        t.Entries.Count,

                    Status =
                        t.Status.ToString(),

                    OrganizerName =
                        t.OrganizerUser.Name
                })
            .ToListAsync();
    }

    public async Task<TournamentDetailsDto?>
        GetByIdAsync(
            int tournamentId)
    {
        return await _context.Tournaments
            .AsNoTracking()
            .Where(t =>
                t.Id == tournamentId)
            .Select(t =>
                new TournamentDetailsDto
                {
                    Id = t.Id,

                    OrganizerUserId =
                        t.OrganizerUserId,

                    OrganizerName =
                        t.OrganizerUser.Name,

                    Name = t.Name,

                    Description =
                        t.Description,

                    Location = t.Location,

                    StartDate = t.StartDate,

                    EndDate = t.EndDate,

                    RegistrationDeadline =
                        t.RegistrationDeadline,

                    MaxTeams = t.MaxTeams,

                    RegisteredTeams =
                        t.Entries.Count,

                    Format =
                        t.Format.ToString(),

                    Status =
                        t.Status.ToString(),

                    CreatedAt =
                        t.CreatedAt
                })
            .FirstOrDefaultAsync();
    }

    private static TournamentDetailsDto
        ToDetailsDto(
            Tournament tournament,
            string organizerName,
            int registeredTeams)
    {
        return new TournamentDetailsDto
        {
            Id = tournament.Id,

            OrganizerUserId =
                tournament.OrganizerUserId,

            OrganizerName =
                organizerName,

            Name = tournament.Name,

            Description =
                tournament.Description,

            Location =
                tournament.Location,

            StartDate =
                tournament.StartDate,

            EndDate =
                tournament.EndDate,

            RegistrationDeadline =
                tournament.RegistrationDeadline,

            MaxTeams =
                tournament.MaxTeams,

            RegisteredTeams =
                registeredTeams,

            Format =
                tournament.Format.ToString(),

            Status =
                tournament.Status.ToString(),

            CreatedAt =
                tournament.CreatedAt
        };
    }

    public async Task<List<TournamentEntryListItemDto>>
        GetEntriesAsync(
            int tournamentId)
    {
        var tournamentExists =
            await _context.Tournaments
                .AnyAsync(t => t.Id == tournamentId);

        if (!tournamentExists)
        {
            throw new Exception(
                "A verseny nem található.");
        }

        return await _context.TournamentEntries
            .AsNoTracking()
            .Where(te =>
                te.TournamentId == tournamentId)
            .OrderBy(te =>
                te.RegisteredAt)
            .Select(te =>
                new TournamentEntryListItemDto
                {
                    EntryId = te.Id,

                    TeamId = te.TeamId,

                    TeamName =
                        te.Team.Name,

                    Player1UserId =
                        te.Team.Player1UserId,

                    Player1Name =
                        te.Team.Player1User.Name,

                    Player2UserId =
                        te.Team.Player2UserId,

                    Player2Name =
                        te.Team.Player2User.Name,

                    RegisteredAt =
                        te.RegisteredAt
                })
            .ToListAsync();
    }

    public async Task<TournamentDrawResponseDto>
        GeneratePoolDrawAsync(
            int tournamentId,
            int currentUserId)
    {
        var tournament =
            await _context.Tournaments
                .Include(t => t.Entries)
                    .ThenInclude(e => e.Team)
                .Include(t => t.Pools)
                .Include(t => t.Matches)
                .FirstOrDefaultAsync(t =>
                    t.Id == tournamentId);

        if (tournament == null)
        {
            throw new Exception(
                "A verseny nem található.");
        }

        if (tournament.OrganizerUserId !=
            currentUserId)
        {
            throw new UnauthorizedAccessException(
                "A sorsolást csak a verseny szervezője indíthatja el.");
        }

        if (tournament.Status !=
            TournamentStatus.RegistrationOpen)
        {
            throw new Exception(
                "A sorsolás csak nyitott nevezési állapotból indítható.");
        }

        if (tournament.Pools.Any() ||
            tournament.Matches.Any())
        {
            throw new Exception(
                "Ehhez a versenyhez már készült sorsolás.");
        }

        var entries =
            tournament.Entries.ToList();

        if (entries.Count < 12)
        {
            throw new Exception(
                "A verseny elindításához legalább 12 csapat szükséges.");
        }

        if (entries.Count > 16)
        {
            throw new Exception(
                "Legfeljebb 16 csapat vehet részt.");
        }

        Shuffle(entries);

        var poolNames =
            new[] { "A", "B", "C", "D" };

        var pools =
            new List<TournamentPool>();

        for (var i = 0; i < 4; i++)
        {
            var pool = new TournamentPool
            {
                TournamentId = tournament.Id,
                PoolNumber = i + 1,
                Name = poolNames[i]
            };

            pools.Add(pool);

            _context.TournamentPools.Add(pool);
        }

        await _context.SaveChangesAsync();

        var poolOrder =
            new[] { 0, 1, 2, 3 }
                .OrderBy(_ => Random.Shared.Next())
                .ToArray();

        var entryIndex = 0;

        for (var slotNumber = 1;
            slotNumber <= 4;
            slotNumber++)
        {
            foreach (var poolIndex in poolOrder)
            {
                TournamentEntry? entry = null;

                if (entryIndex < entries.Count)
                {
                    entry = entries[entryIndex];
                    entryIndex++;
                }

                var slot =
                    new TournamentPoolSlot
                    {
                        TournamentPoolId =
                            pools[poolIndex].Id,

                        TournamentEntryId =
                            entry?.Id,

                        SlotNumber =
                            slotNumber
                    };

                _context.TournamentPoolSlots
                    .Add(slot);
            }
        }

        await _context.SaveChangesAsync();

        var globalMatchNumber = 1;

        foreach (var pool in pools)
        {
            var slots =
                await _context.TournamentPoolSlots
                    .Where(s =>
                        s.TournamentPoolId ==
                        pool.Id)
                    .OrderBy(s =>
                        s.SlotNumber)
                    .ToListAsync();

            var slot1 = slots.Single(s =>
                s.SlotNumber == 1);

            var slot2 = slots.Single(s =>
                s.SlotNumber == 2);

            var slot3 = slots.Single(s =>
                s.SlotNumber == 3);

            var slot4 = slots.Single(s =>
                s.SlotNumber == 4);

            var opening1 =
                CreateOpeningMatch(
                    tournament.Id,
                    pool.Id,
                    globalMatchNumber++,
                    slot1.TournamentEntryId,
                    slot4.TournamentEntryId);

            var opening2 =
                CreateOpeningMatch(
                    tournament.Id,
                    pool.Id,
                    globalMatchNumber++,
                    slot2.TournamentEntryId,
                    slot3.TournamentEntryId);

            _context.TournamentMatches.Add(opening1);
            _context.TournamentMatches.Add(opening2);

            await _context.SaveChangesAsync();

            CompleteByeMatchIfNeeded(opening1);
            CompleteByeMatchIfNeeded(opening2);

            await _context.SaveChangesAsync();

            var winnersMatch =
                new TournamentMatch
                {
                    TournamentId =
                        tournament.Id,

                    TournamentPoolId =
                        pool.Id,

                    MatchNumber =
                        globalMatchNumber++,

                    Stage =
                        TournamentMatchStage.PoolWinners,

                    Team1SourceMatchId =
                        opening1.Id,

                    Team1SourceType =
                        MatchParticipantSourceType.MatchWinner,

                    Team2SourceMatchId =
                        opening2.Id,

                    Team2SourceType =
                        MatchParticipantSourceType.MatchWinner,

                    Team1EntryId =
                        opening1.WinnerEntryId,

                    Team2EntryId =
                        opening2.WinnerEntryId,

                    Status =
                        GetDerivedMatchStatus(
                            opening1.WinnerEntryId,
                            opening2.WinnerEntryId)
                };

            var losersMatch =
                new TournamentMatch
                {
                    TournamentId =
                        tournament.Id,

                    TournamentPoolId =
                        pool.Id,

                    MatchNumber =
                        globalMatchNumber++,

                    Stage =
                        TournamentMatchStage.PoolLosers,

                    Team1SourceMatchId =
                        opening1.Id,

                    Team1SourceType =
                        MatchParticipantSourceType.MatchLoser,

                    Team2SourceMatchId =
                        opening2.Id,

                    Team2SourceType =
                        MatchParticipantSourceType.MatchLoser,

                    Team1EntryId =
                        opening1.LoserEntryId,

                    Team2EntryId =
                        opening2.LoserEntryId,

                    Status =
                        GetDerivedMatchStatus(
                            opening1.LoserEntryId,
                            opening2.LoserEntryId)
                };

            _context.TournamentMatches
                .Add(winnersMatch);

            _context.TournamentMatches
                .Add(losersMatch);

            await _context.SaveChangesAsync();
        }

        tournament.Status =
            TournamentStatus.PoolStage;

        await _context.SaveChangesAsync();

        return await BuildDrawResponseAsync(
            tournament.Id);
    }

    public async Task<TournamentMatchResultResponseDto>
        SubmitMatchResultAsync(
            int tournamentId,
            int matchId,
            int currentUserId,
            SubmitTournamentMatchResultRequestDto request)
    {
        var tournament =
            await _context.Tournaments
                .FirstOrDefaultAsync(t =>
                    t.Id == tournamentId);

        if (tournament == null)
        {
            throw new Exception(
                "A verseny nem található.");
        }

        if (tournament.OrganizerUserId !=
            currentUserId)
        {
            throw new UnauthorizedAccessException(
                "Eredményt csak a verseny szervezője rögzíthet.");
        }

        var match =
            await _context.TournamentMatches
                .Include(m => m.Team1Entry)
                    .ThenInclude(e => e!.Team)
                .Include(m => m.Team2Entry)
                    .ThenInclude(e => e!.Team)
                .Include(m => m.Sets)
                .FirstOrDefaultAsync(m =>
                    m.Id == matchId &&
                    m.TournamentId == tournamentId);

        if (match == null)
        {
            throw new Exception(
                "A mérkőzés nem található.");
        }

        if (match.Status ==
            TournamentMatchStatus.Completed)
        {
            throw new Exception(
                "Ehhez a mérkőzéshez már rögzítettek eredményt.");
        }

        if (!match.Team1EntryId.HasValue ||
            !match.Team2EntryId.HasValue)
        {
            throw new Exception(
                "A mérkőzés résztvevői még nem ismertek.");
        }

        if (request.Sets == null ||
            request.Sets.Count < 2 ||
            request.Sets.Count > 3)
        {
            throw new Exception(
                "Egy strandröplabda mérkőzés 2 vagy 3 szettből állhat.");
        }

        var team1SetsWon = 0;
        var team2SetsWon = 0;

        for (var i = 0; i < request.Sets.Count; i++)
        {
            var set =
                request.Sets[i];

            ValidateSetScore(
                i + 1,
                set.Team1Points,
                set.Team2Points);

            if (set.Team1Points >
                set.Team2Points)
            {
                team1SetsWon++;
            }
            else
            {
                team2SetsWon++;
            }
        }

        if (!(
            (team1SetsWon == 2 &&
            team2SetsWon is 0 or 1)
            ||
            (team2SetsWon == 2 &&
            team1SetsWon is 0 or 1)
        ))
        {
            throw new Exception(
                "A mérkőzés végeredményének 2:0 vagy 2:1 arányúnak kell lennie.");
        }

        if (request.Sets.Count == 2 &&
            !(team1SetsWon == 2 ||
            team2SetsWon == 2))
        {
            throw new Exception(
                "Két szett esetén az egyik csapatnak mindkét szettet meg kell nyernie.");
        }

        if (request.Sets.Count == 3 &&
            !(
                team1SetsWon == 2 &&
                team2SetsWon == 1
                ||
                team2SetsWon == 2 &&
                team1SetsWon == 1
            ))
        {
            throw new Exception(
                "Három szett esetén a végeredmény csak 2:1 lehet.");
        }

        foreach (var oldSet in
                match.Sets.ToList())
        {
            _context.TournamentMatchSets
                .Remove(oldSet);
        }

        for (var i = 0;
            i < request.Sets.Count;
            i++)
        {
            var set =
                request.Sets[i];

            _context.TournamentMatchSets.Add(
                new TournamentMatchSet
                {
                    TournamentMatchId =
                        match.Id,

                    SetNumber =
                        i + 1,

                    Team1Points =
                        set.Team1Points,

                    Team2Points =
                        set.Team2Points
                });
        }

        match.Team1SetsWon =
            team1SetsWon;

        match.Team2SetsWon =
            team2SetsWon;

        if (team1SetsWon >
            team2SetsWon)
        {
            match.WinnerEntryId =
                match.Team1EntryId;

            match.LoserEntryId =
                match.Team2EntryId;
        }
        else
        {
            match.WinnerEntryId =
                match.Team2EntryId;

            match.LoserEntryId =
                match.Team1EntryId;
        }

        match.Status =
            TournamentMatchStatus.Completed;

        match.CompletedAt =
            DateTime.UtcNow;

        match.IsAutomaticResult =
            false;

        await _context.SaveChangesAsync();

        await PropagateMatchResultAsync(
            match);

        await UpdatePoolFinalRanksIfNeededAsync(
            match);

        await _context.SaveChangesAsync();

        await TryGenerateKnockoutBracketAsync(
            tournamentId);

        await TryCompleteTournamentAsync(
            tournamentId);

        return await BuildMatchResultResponseAsync(
            match.Id);
    }

    public async Task<List<TournamentMatchDto>>
    GetMatchesAsync(
        int tournamentId)
    {
        var exists =
            await _context.Tournaments
                .AnyAsync(t => t.Id == tournamentId);

        if (!exists)
        {
            throw new Exception(
                "A verseny nem található.");
        }

        return await _context.TournamentMatches
            .AsNoTracking()
            .Where(m =>
                m.TournamentId == tournamentId)
            .OrderBy(m =>
                m.MatchNumber)
            .Select(m =>
                new TournamentMatchDto
                {
                    Id = m.Id,

                    MatchNumber =
                        m.MatchNumber,

                    Stage =
                        m.Stage.ToString(),

                    PoolId =
                        m.TournamentPoolId,

                    PoolName =
                        m.TournamentPool != null
                            ? m.TournamentPool.Name
                            : null,

                    Team1EntryId =
                        m.Team1EntryId,

                    Team1Name =
                        m.Team1Entry != null
                            ? m.Team1Entry.Team.Name
                            : null,

                    Team2EntryId =
                        m.Team2EntryId,

                    Team2Name =
                        m.Team2Entry != null
                            ? m.Team2Entry.Team.Name
                            : null,

                    Team1SourceLabel =
                        BuildSourceLabel(
                            m.Team1SourceType,
                            m.Team1SourceMatchId),

                    Team2SourceLabel =
                        BuildSourceLabel(
                            m.Team2SourceType,
                            m.Team2SourceMatchId),

                    Team1SetsWon =
                        m.Team1SetsWon,

                    Team2SetsWon =
                        m.Team2SetsWon,

                    WinnerEntryId =
                        m.WinnerEntryId,

                    WinnerTeamName =
                        m.WinnerEntry != null
                            ? m.WinnerEntry.Team.Name
                            : null,

                    IsAutomaticResult =
                        m.IsAutomaticResult,

                    Status =
                        m.Status.ToString(),

                    Sets =
                        m.Sets
                            .OrderBy(s =>
                                s.SetNumber)
                            .Select(s =>
                                new TournamentMatchSetDto
                                {
                                    SetNumber =
                                        s.SetNumber,

                                    Team1Points =
                                        s.Team1Points,

                                    Team2Points =
                                        s.Team2Points
                                })
                            .ToList()
                })
            .ToListAsync();
    }

    public async Task<TournamentBracketDto>
        GetBracketAsync(
            int tournamentId)
    {
        var tournament =
            await _context.Tournaments
                .AsNoTracking()
                .FirstOrDefaultAsync(t =>
                    t.Id == tournamentId);

        if (tournament == null)
        {
            throw new Exception(
                "A verseny nem található.");
        }

        var matches =
            await GetMatchesAsync(
                tournamentId);

        return new TournamentBracketDto
        {
            TournamentId =
                tournament.Id,

            TournamentName =
                tournament.Name,

            PoolMatches =
                matches
                    .Where(m =>
                        m.Stage ==
                            TournamentMatchStage.PoolOpening.ToString()
                        ||
                        m.Stage ==
                            TournamentMatchStage.PoolWinners.ToString()
                        ||
                        m.Stage ==
                            TournamentMatchStage.PoolLosers.ToString())
                    .ToList(),

            RoundOf12Matches =
                matches
                    .Where(m =>
                        m.Stage ==
                            TournamentMatchStage.RoundOf12.ToString())
                    .ToList(),

            QuarterfinalMatches =
                matches
                    .Where(m =>
                        m.Stage ==
                            TournamentMatchStage.Quarterfinal.ToString())
                    .ToList(),

            SemifinalMatches =
                matches
                    .Where(m =>
                        m.Stage ==
                            TournamentMatchStage.Semifinal.ToString())
                    .ToList(),

            BronzeMatch =
                matches
                    .FirstOrDefault(m =>
                        m.Stage ==
                            TournamentMatchStage.Bronze.ToString()),

            FinalMatch =
                matches
                    .FirstOrDefault(m =>
                        m.Stage ==
                            TournamentMatchStage.Final.ToString())
        };
    }

    public async Task<TournamentFinalResultDto>
        GetFinalResultAsync(
            int tournamentId)
    {
        var tournament =
            await _context.Tournaments
                .AsNoTracking()
                .FirstOrDefaultAsync(t =>
                    t.Id == tournamentId);

        if (tournament == null)
        {
            throw new Exception(
                "A verseny nem található.");
        }

        var finalMatch =
            await _context.TournamentMatches
                .AsNoTracking()
                .Include(m => m.WinnerEntry)
                    .ThenInclude(e => e!.Team)
                .Include(m => m.LoserEntry)
                    .ThenInclude(e => e!.Team)
                .FirstOrDefaultAsync(m =>
                    m.TournamentId == tournamentId &&
                    m.Stage ==
                        TournamentMatchStage.Final);

        var bronzeMatch =
            await _context.TournamentMatches
                .AsNoTracking()
                .Include(m => m.WinnerEntry)
                    .ThenInclude(e => e!.Team)
                .Include(m => m.LoserEntry)
                    .ThenInclude(e => e!.Team)
                .FirstOrDefaultAsync(m =>
                    m.TournamentId == tournamentId &&
                    m.Stage ==
                        TournamentMatchStage.Bronze);

        if (finalMatch == null ||
            bronzeMatch == null)
        {
            throw new Exception(
                "A kieséses szakasz még nem készült el.");
        }

        if (finalMatch.Status !=
                TournamentMatchStatus.Completed ||
            bronzeMatch.Status !=
                TournamentMatchStatus.Completed)
        {
            throw new Exception(
                "A verseny még nem fejeződött be.");
        }

        if (!finalMatch.WinnerEntryId.HasValue ||
            !finalMatch.LoserEntryId.HasValue ||
            !bronzeMatch.WinnerEntryId.HasValue ||
            !bronzeMatch.LoserEntryId.HasValue)
        {
            throw new Exception(
                "A végeredmény nem állapítható meg.");
        }

        return new TournamentFinalResultDto
        {
            TournamentId =
                tournament.Id,

            TournamentName =
                tournament.Name,

            FirstPlaceEntryId =
                finalMatch.WinnerEntryId.Value,

            FirstPlaceTeamName =
                finalMatch.WinnerEntry!.Team.Name,

            SecondPlaceEntryId =
                finalMatch.LoserEntryId.Value,

            SecondPlaceTeamName =
                finalMatch.LoserEntry!.Team.Name,

            ThirdPlaceEntryId =
                bronzeMatch.WinnerEntryId.Value,

            ThirdPlaceTeamName =
                bronzeMatch.WinnerEntry!.Team.Name,

            FourthPlaceEntryId =
                bronzeMatch.LoserEntryId.Value,

            FourthPlaceTeamName =
                bronzeMatch.LoserEntry!.Team.Name
        };
    }

    private static void Shuffle<T>(
        IList<T> list)
    {
        for (var i = list.Count - 1;
            i > 0;
            i--)
        {
            var j =
                Random.Shared.Next(i + 1);

            (list[i], list[j]) =
                (list[j], list[i]);
        }
    }

    private static TournamentMatch
        CreateOpeningMatch(
            int tournamentId,
            int poolId,
            int matchNumber,
            int? team1EntryId,
            int? team2EntryId)
    {
        return new TournamentMatch
        {
            TournamentId =
                tournamentId,

            TournamentPoolId =
                poolId,

            MatchNumber =
                matchNumber,

            Stage =
                TournamentMatchStage.PoolOpening,

            Team1EntryId =
                team1EntryId,

            Team2EntryId =
                team2EntryId,

            Team1SourceType =
                MatchParticipantSourceType.DirectTeam,

            Team2SourceType =
                MatchParticipantSourceType.DirectTeam,

            Status =
                team1EntryId.HasValue &&
                team2EntryId.HasValue
                    ? TournamentMatchStatus.Ready
                    : TournamentMatchStatus.WaitingForTeams
        };
    }

    private static void CompleteByeMatchIfNeeded(
        TournamentMatch match)
    {
        var team1Exists =
            match.Team1EntryId.HasValue;

        var team2Exists =
            match.Team2EntryId.HasValue;

        if (team1Exists == team2Exists)
        {
            return;
        }

        match.IsAutomaticResult = true;

        match.Status =
            TournamentMatchStatus.Completed;

        match.CompletedAt =
            DateTime.UtcNow;

        if (team1Exists)
        {
            match.WinnerEntryId =
                match.Team1EntryId;

            match.LoserEntryId = null;

            match.Team1SetsWon = 2;
            match.Team2SetsWon = 0;
        }
        else
        {
            match.WinnerEntryId =
                match.Team2EntryId;

            match.LoserEntryId = null;

            match.Team1SetsWon = 0;
            match.Team2SetsWon = 2;
        }
    }

    private static TournamentMatchStatus
            GetDerivedMatchStatus(
                int? team1EntryId,
                int? team2EntryId)
    {
        if (team1EntryId.HasValue &&
            team2EntryId.HasValue)
        {
            return TournamentMatchStatus.Ready;
        }

        return TournamentMatchStatus.WaitingForTeams;
    }

    private static void ValidateSetScore(
        int setNumber,
        int team1Points,
        int team2Points)
    {
        if (team1Points < 0 ||
            team2Points < 0)
        {
            throw new Exception(
                "A szettpontszám nem lehet negatív.");
        }

        var isDecidingSet =
            setNumber == 3;

        var minimumWinningPoints =
            isDecidingSet
                ? 15
                : 21;

        var winnerPoints =
            Math.Max(
                team1Points,
                team2Points);

        var loserPoints =
            Math.Min(
                team1Points,
                team2Points);

        if (winnerPoints <
            minimumWinningPoints)
        {
            throw new Exception(
                $"{setNumber}. szett: a győztes pontszáma túl alacsony.");
        }

        if (winnerPoints -
            loserPoints < 2)
        {
            throw new Exception(
                $"{setNumber}. szett: legalább kétpontos különbség szükséges.");
        }

        if (winnerPoints >
            minimumWinningPoints &&
            winnerPoints -
            loserPoints != 2)
        {
            throw new Exception(
                $"{setNumber}. szett: hosszabbításban pontosan kétpontos különbséggel kell nyerni.");
        }
    }

    private async Task PropagateMatchResultAsync(
        TournamentMatch completedMatch)
    {
        var dependentMatches =
            await _context.TournamentMatches
                .Where(m =>
                    m.Team1SourceMatchId ==
                        completedMatch.Id ||
                    m.Team2SourceMatchId ==
                        completedMatch.Id)
                .ToListAsync();

        foreach (var nextMatch in dependentMatches)
        {
            if (nextMatch.Team1SourceMatchId ==
                completedMatch.Id)
            {
                nextMatch.Team1EntryId =
                    ResolveSourceEntryId(
                        completedMatch,
                        nextMatch.Team1SourceType);
            }

            if (nextMatch.Team2SourceMatchId ==
                completedMatch.Id)
            {
                nextMatch.Team2EntryId =
                    ResolveSourceEntryId(
                        completedMatch,
                        nextMatch.Team2SourceType);
            }

            var team1Known =
                nextMatch.Team1EntryId.HasValue;

            var team2Known =
                nextMatch.Team2EntryId.HasValue;

            if (team1Known &&
                team2Known)
            {
                nextMatch.Status =
                    TournamentMatchStatus.Ready;
            }
            else
            {
                /*
                * Ha az egyik forrásmeccs már lezárult,
                * de abból nincs valódi vesztes/győztes
                * (BYE miatt), akkor a másik valódi csapat
                * automatikusan nyeri ezt a meccset.
                */
                var team1SourceResolved =
                    await IsMatchSourceResolvedAsync(
                        nextMatch.Team1SourceMatchId);

                var team2SourceResolved =
                    await IsMatchSourceResolvedAsync(
                        nextMatch.Team2SourceMatchId);

                if (team1SourceResolved &&
                    team2SourceResolved &&
                    team1Known != team2Known)
                {
                    CompleteByeMatchIfNeeded(
                        nextMatch);
                }
                else
                {
                    nextMatch.Status =
                        TournamentMatchStatus.WaitingForTeams;
                }
            }
        }

        await _context.SaveChangesAsync();

        /*
        * Ha egy következő meccs BYE miatt automatikusan
        * befejeződött, annak eredményét is tovább kell vezetni.
        */
        foreach (var nextMatch in dependentMatches)
        {
            if (nextMatch.Status ==
                    TournamentMatchStatus.Completed &&
                nextMatch.IsAutomaticResult)
            {
                await UpdatePoolFinalRanksIfNeededAsync(
                    nextMatch);

                await _context.SaveChangesAsync();

                await PropagateMatchResultAsync(
                    nextMatch);

                await TryGenerateKnockoutBracketAsync(
                    nextMatch.TournamentId);
            }
        }
    }

    private async Task<bool> IsMatchSourceResolvedAsync(
        int? sourceMatchId)
    {
        if (!sourceMatchId.HasValue)
        {
            return true;
        }

        return await _context.TournamentMatches
            .AnyAsync(m =>
                m.Id == sourceMatchId.Value &&
                m.Status ==
                    TournamentMatchStatus.Completed);
    }

    private static int? ResolveSourceEntryId(
        TournamentMatch sourceMatch,
        MatchParticipantSourceType sourceType)
    {
        return sourceType switch
        {
            MatchParticipantSourceType.MatchWinner =>
                sourceMatch.WinnerEntryId,

            MatchParticipantSourceType.MatchLoser =>
                sourceMatch.LoserEntryId,

            MatchParticipantSourceType.DirectTeam =>
                null,

            _ => null
        };
    }

    private async Task UpdatePoolFinalRanksIfNeededAsync(
        TournamentMatch completedMatch)
    {
        if (!completedMatch.TournamentPoolId
            .HasValue)
        {
            return;
        }

        if (completedMatch.Stage !=
                TournamentMatchStage.PoolWinners &&
            completedMatch.Stage !=
                TournamentMatchStage.PoolLosers)
        {
            return;
        }

        if (!completedMatch.WinnerEntryId
                .HasValue)
        {
            return;
        }

        var poolId =
            completedMatch.TournamentPoolId.Value;

        var slots =
            await _context.TournamentPoolSlots
                .Where(s =>
                    s.TournamentPoolId ==
                        poolId)
                .ToListAsync();

        if (completedMatch.Stage ==
            TournamentMatchStage.PoolWinners)
        {
            SetFinalRank(
                slots,
                completedMatch.WinnerEntryId,
                1);

            SetFinalRank(
                slots,
                completedMatch.LoserEntryId,
                2);
        }

        if (completedMatch.Stage ==
            TournamentMatchStage.PoolLosers)
        {
            SetFinalRank(
                slots,
                completedMatch.WinnerEntryId,
                3);

            SetFinalRank(
                slots,
                completedMatch.LoserEntryId,
                4);
        }
    }

    private static void SetFinalRank(
        List<TournamentPoolSlot> slots,
        int? entryId,
        int finalRank)
    {
        if (!entryId.HasValue)
        {
            return;
        }

        var slot =
            slots.FirstOrDefault(s =>
                s.TournamentEntryId ==
                    entryId.Value);

        if (slot != null)
        {
            slot.FinalRank =
                finalRank;
        }
    }

    private async Task<TournamentMatchResultResponseDto>
    BuildMatchResultResponseAsync(
            int matchId)
    {
        var result =
            await _context.TournamentMatches
                .AsNoTracking()
                .Where(m =>
                    m.Id == matchId)
                .Select(m =>
                    new TournamentMatchResultResponseDto
                    {
                        MatchId =
                            m.Id,

                        MatchNumber =
                            m.MatchNumber,

                        Stage =
                            m.Stage.ToString(),

                        Team1EntryId =
                            m.Team1EntryId!.Value,

                        Team1Name =
                            m.Team1Entry!.Team.Name,

                        Team2EntryId =
                            m.Team2EntryId!.Value,

                        Team2Name =
                            m.Team2Entry!.Team.Name,

                        Team1SetsWon =
                            m.Team1SetsWon!.Value,

                        Team2SetsWon =
                            m.Team2SetsWon!.Value,

                        WinnerEntryId =
                            m.WinnerEntryId!.Value,

                        WinnerTeamName =
                            m.WinnerEntry!.Team.Name,

                        LoserEntryId =
                            m.LoserEntryId!.Value,

                        LoserTeamName =
                            m.LoserEntry!.Team.Name,

                        Status =
                            m.Status.ToString()
                    })
                .FirstOrDefaultAsync();

        if (result == null)
        {
            throw new Exception(
                "A mérkőzés nem található.");
        }

        return result;
    }

    private async Task<TournamentDrawResponseDto>
        BuildDrawResponseAsync(
            int tournamentId)
    {
        var tournament =
            await _context.Tournaments
                .AsNoTracking()
                .FirstAsync(t =>
                    t.Id == tournamentId);

        var pools =
            await _context.TournamentPools
                .AsNoTracking()
                .Where(p =>
                    p.TournamentId ==
                    tournamentId)
                .OrderBy(p =>
                    p.PoolNumber)
                .Select(p =>
                    new TournamentPoolDto
                    {
                        PoolId =
                            p.Id,

                        Name =
                            p.Name,

                        PoolNumber =
                            p.PoolNumber,

                        Slots =
                            p.Slots
                                .OrderBy(s =>
                                    s.SlotNumber)
                                .Select(s =>
                                    new TournamentPoolSlotDto
                                    {
                                        SlotNumber =
                                            s.SlotNumber,

                                        EntryId =
                                            s.TournamentEntryId,

                                        TeamId =
                                            s.TournamentEntry != null
                                                ? s.TournamentEntry.TeamId
                                                : null,

                                        TeamName =
                                            s.TournamentEntry != null
                                                ? s.TournamentEntry.Team.Name
                                                : null,

                                        IsBye =
                                            s.TournamentEntryId == null,

                                        FinalRank =
                                            s.FinalRank
                                    })
                                .ToList()
                    })
                .ToListAsync();

        var matchCount =
            await _context.TournamentMatches
                .CountAsync(m =>
                    m.TournamentId ==
                    tournamentId);

        return new TournamentDrawResponseDto
        {
            TournamentId =
                tournament.Id,

            TournamentName =
                tournament.Name,

            Pools =
                pools,

            CreatedMatches =
                matchCount
        };
    }

    

    private static string BuildSourceLabel(
        MatchParticipantSourceType sourceType,
        int? sourceMatchId)
    {
        if (sourceType ==
                MatchParticipantSourceType.DirectTeam ||
            !sourceMatchId.HasValue)
        {
            return string.Empty;
        }

        return sourceType switch
        {
            MatchParticipantSourceType.MatchWinner =>
                $"Winner Match #{sourceMatchId}",

            MatchParticipantSourceType.MatchLoser =>
                $"Loser Match #{sourceMatchId}",

            _ => string.Empty
        };
    }

    private async Task TryGenerateKnockoutBracketAsync(
        int tournamentId)
    {
        var knockoutAlreadyExists =
            await _context.TournamentMatches
                .AnyAsync(m =>
                    m.TournamentId == tournamentId &&
                    (
                        m.Stage == TournamentMatchStage.RoundOf12 ||
                        m.Stage == TournamentMatchStage.Quarterfinal ||
                        m.Stage == TournamentMatchStage.Semifinal ||
                        m.Stage == TournamentMatchStage.Bronze ||
                        m.Stage == TournamentMatchStage.Final
                    ));

        if (knockoutAlreadyExists)
        {
            return;
        }

        var poolMatchesCompleted =
            await _context.TournamentMatches
                .Where(m =>
                    m.TournamentId == tournamentId &&
                    (
                        m.Stage == TournamentMatchStage.PoolOpening ||
                        m.Stage == TournamentMatchStage.PoolWinners ||
                        m.Stage == TournamentMatchStage.PoolLosers
                    ))
                .AllAsync(m =>
                    m.Status ==
                        TournamentMatchStatus.Completed);

        var poolMatchCount =
            await _context.TournamentMatches
                .CountAsync(m =>
                    m.TournamentId == tournamentId &&
                    (
                        m.Stage == TournamentMatchStage.PoolOpening ||
                        m.Stage == TournamentMatchStage.PoolWinners ||
                        m.Stage == TournamentMatchStage.PoolLosers
                    ));

        if (poolMatchCount != 16 ||
            !poolMatchesCompleted)
        {
            return;
        }

        var poolSlots =
            await _context.TournamentPoolSlots
                .Include(s => s.TournamentPool)
                .Where(s =>
                    s.TournamentPool.TournamentId ==
                        tournamentId &&
                    s.TournamentEntryId.HasValue &&
                    s.FinalRank.HasValue)
                .ToListAsync();

        var firstPlaces =
            poolSlots
                .Where(s =>
                    s.FinalRank == 1)
                .OrderBy(s =>
                    s.TournamentPool.PoolNumber)
                .ToList();

        var secondPlaces =
            poolSlots
                .Where(s =>
                    s.FinalRank == 2)
                .ToList();

        var thirdPlaces =
            poolSlots
                .Where(s =>
                    s.FinalRank == 3)
                .ToList();

        if (firstPlaces.Count != 4 ||
            secondPlaces.Count != 4 ||
            thirdPlaces.Count != 4)
        {
            throw new Exception(
                "A csoporteredmények nem teljesek, ezért a kieséses szakasz nem generálható.");
        }

        Shuffle(secondPlaces);
        Shuffle(thirdPlaces);
        Shuffle(firstPlaces);

        var lastMatchNumber =
            await _context.TournamentMatches
                .Where(m =>
                    m.TournamentId == tournamentId)
                .MaxAsync(m =>
                    m.MatchNumber);

        var nextMatchNumber =
            lastMatchNumber + 1;

        var roundOf12Matches =
            new List<TournamentMatch>();

        for (var i = 0; i < 4; i++)
        {
            var secondEntryId =
                secondPlaces[i].TournamentEntryId!.Value;

            var thirdEntryId =
                thirdPlaces[i].TournamentEntryId!.Value;

            var match =
                new TournamentMatch
                {
                    TournamentId =
                        tournamentId,

                    TournamentPoolId =
                        null,

                    MatchNumber =
                        nextMatchNumber++,

                    Stage =
                        TournamentMatchStage.RoundOf12,

                    Team1EntryId =
                        secondEntryId,

                    Team2EntryId =
                        thirdEntryId,

                    Team1SourceType =
                        MatchParticipantSourceType.DirectTeam,

                    Team2SourceType =
                        MatchParticipantSourceType.DirectTeam,

                    Status =
                        TournamentMatchStatus.Ready
                };

            roundOf12Matches.Add(match);

            _context.TournamentMatches.Add(match);
        }

        await _context.SaveChangesAsync();

        Shuffle(roundOf12Matches);

        var quarterfinalMatches =
            new List<TournamentMatch>();

        for (var i = 0; i < 4; i++)
        {
            var poolWinnerEntryId =
                firstPlaces[i].TournamentEntryId!.Value;

            var roundOf12Match =
                roundOf12Matches[i];

            var quarterfinal =
                new TournamentMatch
                {
                    TournamentId =
                        tournamentId,

                    TournamentPoolId =
                        null,

                    MatchNumber =
                        nextMatchNumber++,

                    Stage =
                        TournamentMatchStage.Quarterfinal,

                    Team1EntryId =
                        poolWinnerEntryId,

                    Team1SourceType =
                        MatchParticipantSourceType.DirectTeam,

                    Team2EntryId =
                        null,

                    Team2SourceMatchId =
                        roundOf12Match.Id,

                    Team2SourceType =
                        MatchParticipantSourceType.MatchWinner,

                    Status =
                        TournamentMatchStatus.WaitingForTeams
                };

            quarterfinalMatches.Add(
                quarterfinal);

            _context.TournamentMatches.Add(
                quarterfinal);
        }

        await _context.SaveChangesAsync();

        var semifinal1 =
            new TournamentMatch
            {
                TournamentId =
                    tournamentId,

                MatchNumber =
                    nextMatchNumber++,

                Stage =
                    TournamentMatchStage.Semifinal,

                Team1SourceMatchId =
                    quarterfinalMatches[0].Id,

                Team1SourceType =
                    MatchParticipantSourceType.MatchWinner,

                Team2SourceMatchId =
                    quarterfinalMatches[1].Id,

                Team2SourceType =
                    MatchParticipantSourceType.MatchWinner,

                Status =
                    TournamentMatchStatus.WaitingForTeams
            };

        var semifinal2 =
            new TournamentMatch
            {
                TournamentId =
                    tournamentId,

                MatchNumber =
                    nextMatchNumber++,

                Stage =
                    TournamentMatchStage.Semifinal,

                Team1SourceMatchId =
                    quarterfinalMatches[2].Id,

                Team1SourceType =
                    MatchParticipantSourceType.MatchWinner,

                Team2SourceMatchId =
                    quarterfinalMatches[3].Id,

                Team2SourceType =
                    MatchParticipantSourceType.MatchWinner,

                Status =
                    TournamentMatchStatus.WaitingForTeams
            };

        _context.TournamentMatches.Add(
            semifinal1);

        _context.TournamentMatches.Add(
            semifinal2);

        await _context.SaveChangesAsync();

        var bronzeMatch =
            new TournamentMatch
            {
                TournamentId =
                    tournamentId,

                MatchNumber =
                    nextMatchNumber++,

                Stage =
                    TournamentMatchStage.Bronze,

                Team1SourceMatchId =
                    semifinal1.Id,

                Team1SourceType =
                    MatchParticipantSourceType.MatchLoser,

                Team2SourceMatchId =
                    semifinal2.Id,

                Team2SourceType =
                    MatchParticipantSourceType.MatchLoser,

                Status =
                    TournamentMatchStatus.WaitingForTeams
            };

        var finalMatch =
            new TournamentMatch
            {
                TournamentId =
                    tournamentId,

                MatchNumber =
                    nextMatchNumber++,

                Stage =
                    TournamentMatchStage.Final,

                Team1SourceMatchId =
                    semifinal1.Id,

                Team1SourceType =
                    MatchParticipantSourceType.MatchWinner,

                Team2SourceMatchId =
                    semifinal2.Id,

                Team2SourceType =
                    MatchParticipantSourceType.MatchWinner,

                Status =
                    TournamentMatchStatus.WaitingForTeams
            };

        _context.TournamentMatches.Add(
            bronzeMatch);

        _context.TournamentMatches.Add(
            finalMatch);

        var tournament =
            await _context.Tournaments
                .FirstAsync(t =>
                    t.Id == tournamentId);

        tournament.Status =
            TournamentStatus.KnockoutStage;

        await _context.SaveChangesAsync();
    }

    private async Task TryCompleteTournamentAsync(
        int tournamentId)
    {
        var finalMatch =
            await _context.TournamentMatches
                .FirstOrDefaultAsync(m =>
                    m.TournamentId == tournamentId &&
                    m.Stage ==
                        TournamentMatchStage.Final);

        var bronzeMatch =
            await _context.TournamentMatches
                .FirstOrDefaultAsync(m =>
                    m.TournamentId == tournamentId &&
                    m.Stage ==
                        TournamentMatchStage.Bronze);

        if (finalMatch == null ||
            bronzeMatch == null)
        {
            return;
        }

        if (finalMatch.Status !=
                TournamentMatchStatus.Completed ||
            bronzeMatch.Status !=
                TournamentMatchStatus.Completed)
        {
            return;
        }

        var tournament =
            await _context.Tournaments
                .FirstOrDefaultAsync(t =>
                    t.Id == tournamentId);

        if (tournament == null)
        {
            throw new Exception(
                "A verseny nem található.");
        }

        tournament.Status =
            TournamentStatus.Completed;

        await _context.SaveChangesAsync();
    }

}