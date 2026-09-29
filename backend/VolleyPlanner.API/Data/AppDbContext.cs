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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User email legyen egyedi
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

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
    }
}