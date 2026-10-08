using Microsoft.EntityFrameworkCore;
using VolleyPlanner.API.Entities;

namespace VolleyPlanner.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(
        DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<UserSportRole> UserSportRoles => Set<UserSportRole>();

    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<ExerciseTag> ExerciseTags => Set<ExerciseTag>();

    public DbSet<TrainingPlan> TrainingPlans => Set<TrainingPlan>();
    public DbSet<TrainingPlanItem> TrainingPlanItems => Set<TrainingPlanItem>();
    public DbSet<GenerationRequest> GenerationRequests => Set<GenerationRequest>();
    public DbSet<CalendarEvent> CalendarEvents => Set<CalendarEvent>();

    public DbSet<TrainingSession> TrainingSessions => Set<TrainingSession>();
    public DbSet<TrainingBooking> TrainingBookings => Set<TrainingBooking>();
    public DbSet<Tournament> Tournaments => Set<Tournament>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamInvitation> TeamInvitations => Set<TeamInvitation>();
    public DbSet<TournamentEntry> TournamentEntries => Set<TournamentEntry>();
    public DbSet<TournamentPool> TournamentPools => Set<TournamentPool>();
    public DbSet<TournamentPoolSlot> TournamentPoolSlots => Set<TournamentPoolSlot>();
    public DbSet<TournamentMatch> TournamentMatches => Set<TournamentMatch>();
    public DbSet<TournamentMatchSet> TournamentMatchSets => Set<TournamentMatchSet>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User email legyen egyedi
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.PlayerCode)
            .IsUnique()
            .HasFilter("[PlayerCode] IS NOT NULL");

        // User - UserProfile 1:1 kapcsolat
        modelBuilder.Entity<User>()
            .HasOne(u => u.Profile)
            .WithOne(p => p.User)
            .HasForeignKey<UserProfile>(p => p.UserId);

        // User - UserSportRole 1:N kapcsolat
        modelBuilder.Entity<UserSportRole>()
            .HasOne(usr => usr.User)
            .WithMany(u => u.SportRoles)
            .HasForeignKey(usr => usr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<UserSportRole>()
            .HasIndex(usr => new
            {
                usr.UserId,
                usr.RoleType
            })
            .IsUnique();

        // ExerciseTag összetett kulcs
        modelBuilder.Entity<ExerciseTag>()
            .HasKey(et => new
            {
                et.ExerciseId,
                et.TagId
            });

        modelBuilder.Entity<ExerciseTag>()
            .HasOne(et => et.Exercise)
            .WithMany(e => e.ExerciseTags)
            .HasForeignKey(et => et.ExerciseId);

        modelBuilder.Entity<ExerciseTag>()
            .HasOne(et => et.Tag)
            .WithMany(t => t.ExerciseTags)
            .HasForeignKey(et => et.TagId);

        // TrainingPlan - User
        modelBuilder.Entity<TrainingPlan>()
            .HasOne(tp => tp.User)
            .WithMany(u => u.TrainingPlans)
            .HasForeignKey(tp => tp.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // TrainingPlanItem - TrainingPlan
        modelBuilder.Entity<TrainingPlanItem>()
            .HasOne(tpi => tpi.TrainingPlan)
            .WithMany(tp => tp.Items)
            .HasForeignKey(tpi => tpi.TrainingPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        // TrainingPlanItem - Exercise
        modelBuilder.Entity<TrainingPlanItem>()
            .HasOne(tpi => tpi.Exercise)
            .WithMany()
            .HasForeignKey(tpi => tpi.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);

        // GenerationRequest - User
        modelBuilder.Entity<GenerationRequest>()
            .HasOne(gr => gr.User)
            .WithMany(u => u.GenerationRequests)
            .HasForeignKey(gr => gr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // CalendarEvent - User
        modelBuilder.Entity<CalendarEvent>()
            .HasOne(ce => ce.User)
            .WithMany(u => u.CalendarEvents)
            .HasForeignKey(ce => ce.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        // CalendarEvent - TrainingPlan
        // Már opcionális kapcsolat.
        modelBuilder.Entity<CalendarEvent>()
            .HasOne(ce => ce.TrainingPlan)
            .WithMany(tp => tp.CalendarEvents)
            .HasForeignKey(ce => ce.TrainingPlanId)
            .OnDelete(DeleteBehavior.SetNull);

        // CalendarEvent - TrainingSession
        modelBuilder.Entity<CalendarEvent>()
            .HasOne(ce => ce.TrainingSession)
            .WithMany(ts => ts.CalendarEvents)
            .HasForeignKey(ce => ce.TrainingSessionId)
            .OnDelete(DeleteBehavior.SetNull);

        // Ugyanazt a meghirdetett edzést ugyanaz a user
        // csak egyszer tehesse be a naptárába.
        modelBuilder.Entity<CalendarEvent>()
            .HasIndex(ce => new
            {
                ce.UserId,
                ce.TrainingSessionId
            })
            .IsUnique()
            .HasFilter("[TrainingSessionId] IS NOT NULL");

        // TrainingSession - Organizer User
        modelBuilder.Entity<TrainingSession>()
            .HasOne(ts => ts.OrganizerUser)
            .WithMany(u => u.OrganizedTrainingSessions)
            .HasForeignKey(ts => ts.OrganizerUserId)
            .OnDelete(DeleteBehavior.NoAction);

        // TrainingSession - TrainingPlan
        modelBuilder.Entity<TrainingSession>()
            .HasOne(ts => ts.TrainingPlan)
            .WithMany(tp => tp.TrainingSessions)
            .HasForeignKey(ts => ts.TrainingPlanId)
            .OnDelete(DeleteBehavior.SetNull);

        // TrainingBooking - TrainingSession
        modelBuilder.Entity<TrainingBooking>()
            .HasOne(tb => tb.TrainingSession)
            .WithMany(ts => ts.Bookings)
            .HasForeignKey(tb => tb.TrainingSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        // TrainingBooking - User
        modelBuilder.Entity<TrainingBooking>()
            .HasOne(tb => tb.User)
            .WithMany(u => u.TrainingBookings)
            .HasForeignKey(tb => tb.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<TrainingBooking>()
            .HasIndex(tb => new
            {
                tb.TrainingSessionId,
                tb.UserId
            })
            .IsUnique();

        // ===============================
        // TOURNAMENT MODULE
        // ===============================

        // Tournament - Organizer
        modelBuilder.Entity<Tournament>()
            .HasOne(t => t.OrganizerUser)
            .WithMany(u => u.OrganizedTournaments)
            .HasForeignKey(t => t.OrganizerUserId)
            .OnDelete(DeleteBehavior.NoAction);

        // Team - Player1
        modelBuilder.Entity<Team>()
            .HasOne(t => t.Player1User)
            .WithMany(u => u.TeamsAsPlayer1)
            .HasForeignKey(t => t.Player1UserId)
            .OnDelete(DeleteBehavior.NoAction);

        // Team - Player2
        modelBuilder.Entity<Team>()
            .HasOne(t => t.Player2User)
            .WithMany(u => u.TeamsAsPlayer2)
            .HasForeignKey(t => t.Player2UserId)
            .OnDelete(DeleteBehavior.NoAction);

        // Ugyanaz a játékos nem lehet saját maga csapattársa.
        modelBuilder.Entity<Team>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_Team_DifferentPlayers",
                "[Player1UserId] <> [Player2UserId]"
            ));

        // TeamInvitation - Tournament
        modelBuilder.Entity<TeamInvitation>()
            .HasOne(ti => ti.Tournament)
            .WithMany(t => t.TeamInvitations)
            .HasForeignKey(ti => ti.TournamentId)
            .OnDelete(DeleteBehavior.Cascade);

        // TeamInvitation - Inviter
        modelBuilder.Entity<TeamInvitation>()
            .HasOne(ti => ti.InviterUser)
            .WithMany(u => u.SentTeamInvitations)
            .HasForeignKey(ti => ti.InviterUserId)
            .OnDelete(DeleteBehavior.NoAction);

        // TeamInvitation - Invited
        modelBuilder.Entity<TeamInvitation>()
            .HasOne(ti => ti.InvitedUser)
            .WithMany(u => u.ReceivedTeamInvitations)
            .HasForeignKey(ti => ti.InvitedUserId)
            .OnDelete(DeleteBehavior.NoAction);

        // TournamentEntry - Tournament
        modelBuilder.Entity<TournamentEntry>()
            .HasOne(te => te.Tournament)
            .WithMany(t => t.Entries)
            .HasForeignKey(te => te.TournamentId)
            .OnDelete(DeleteBehavior.Cascade);

        // TournamentEntry - Team
        modelBuilder.Entity<TournamentEntry>()
            .HasOne(te => te.Team)
            .WithMany(t => t.TournamentEntries)
            .HasForeignKey(te => te.TeamId)
            .OnDelete(DeleteBehavior.NoAction);

        // Ugyanaz a Team ugyanarra a versenyre csak egyszer nevezhet.
        modelBuilder.Entity<TournamentEntry>()
            .HasIndex(te => new
            {
                te.TournamentId,
                te.TeamId
            })
            .IsUnique();

        // TournamentPool - Tournament
        modelBuilder.Entity<TournamentPool>()
            .HasOne(tp => tp.Tournament)
            .WithMany(t => t.Pools)
            .HasForeignKey(tp => tp.TournamentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Egy versenyen belül ne lehessen két A / B / stb. pool.
        modelBuilder.Entity<TournamentPool>()
            .HasIndex(tp => new
            {
                tp.TournamentId,
                tp.PoolNumber
            })
            .IsUnique();

        // TournamentPoolSlot - Pool
        modelBuilder.Entity<TournamentPoolSlot>()
            .HasOne(ps => ps.TournamentPool)
            .WithMany(p => p.Slots)
            .HasForeignKey(ps => ps.TournamentPoolId)
            .OnDelete(DeleteBehavior.Cascade);

        // TournamentPoolSlot - Entry
        modelBuilder.Entity<TournamentPoolSlot>()
            .HasOne(ps => ps.TournamentEntry)
            .WithMany(e => e.PoolSlots)
            .HasForeignKey(ps => ps.TournamentEntryId)
            .OnDelete(DeleteBehavior.NoAction);

        // Egy csoportban minden slot csak egyszer szerepelhet.
        modelBuilder.Entity<TournamentPoolSlot>()
            .HasIndex(ps => new
            {
                ps.TournamentPoolId,
                ps.SlotNumber
            })
            .IsUnique();

        // TournamentMatch - Tournament
        modelBuilder.Entity<TournamentMatch>()
            .HasOne(tm => tm.Tournament)
            .WithMany(t => t.Matches)
            .HasForeignKey(tm => tm.TournamentId)
            .OnDelete(DeleteBehavior.Cascade);

        // TournamentMatch - Pool
        modelBuilder.Entity<TournamentMatch>()
            .HasOne(tm => tm.TournamentPool)
            .WithMany(p => p.Matches)
            .HasForeignKey(tm => tm.TournamentPoolId)
            .OnDelete(DeleteBehavior.NoAction);

        // Team1
        modelBuilder.Entity<TournamentMatch>()
            .HasOne(tm => tm.Team1Entry)
            .WithMany(e => e.MatchesAsTeam1)
            .HasForeignKey(tm => tm.Team1EntryId)
            .OnDelete(DeleteBehavior.NoAction);

        // Team2
        modelBuilder.Entity<TournamentMatch>()
            .HasOne(tm => tm.Team2Entry)
            .WithMany(e => e.MatchesAsTeam2)
            .HasForeignKey(tm => tm.Team2EntryId)
            .OnDelete(DeleteBehavior.NoAction);

        // Winner
        modelBuilder.Entity<TournamentMatch>()
            .HasOne(tm => tm.WinnerEntry)
            .WithMany()
            .HasForeignKey(tm => tm.WinnerEntryId)
            .OnDelete(DeleteBehavior.NoAction);

        // Loser
        modelBuilder.Entity<TournamentMatch>()
            .HasOne(tm => tm.LoserEntry)
            .WithMany()
            .HasForeignKey(tm => tm.LoserEntryId)
            .OnDelete(DeleteBehavior.NoAction);

        // Team1 source match
        modelBuilder.Entity<TournamentMatch>()
            .HasOne(tm => tm.Team1SourceMatch)
            .WithMany()
            .HasForeignKey(tm => tm.Team1SourceMatchId)
            .OnDelete(DeleteBehavior.NoAction);

        // Team2 source match
        modelBuilder.Entity<TournamentMatch>()
            .HasOne(tm => tm.Team2SourceMatch)
            .WithMany()
            .HasForeignKey(tm => tm.Team2SourceMatchId)
            .OnDelete(DeleteBehavior.NoAction);

        // Egy Tournamentben minden match number legyen egyedi.
        modelBuilder.Entity<TournamentMatch>()
            .HasIndex(tm => new
            {
                tm.TournamentId,
                tm.MatchNumber
            })
            .IsUnique();

        // TournamentMatchSet - Match
        modelBuilder.Entity<TournamentMatchSet>()
            .HasOne(ms => ms.TournamentMatch)
            .WithMany(tm => tm.Sets)
            .HasForeignKey(ms => ms.TournamentMatchId)
            .OnDelete(DeleteBehavior.Cascade);

        // Egy meccsen belül egy adott szettszám csak egyszer lehet.
        modelBuilder.Entity<TournamentMatchSet>()
            .HasIndex(ms => new
            {
                ms.TournamentMatchId,
                ms.SetNumber
            })
            .IsUnique();
    }
}