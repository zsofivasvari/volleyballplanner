using Microsoft.EntityFrameworkCore;
using VolleyPlanner.API.Entities;

namespace VolleyPlanner.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
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

        // Egy user ugyanazt a sport szerepkört csak egyszer kaphatja meg
        modelBuilder.Entity<UserSportRole>()
            .HasIndex(usr => new { usr.UserId, usr.RoleType })
            .IsUnique();

        // ExerciseTag összetett kulcs
        modelBuilder.Entity<ExerciseTag>()
            .HasKey(et => new { et.ExerciseId, et.TagId });

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
        modelBuilder.Entity<CalendarEvent>()
            .HasOne(ce => ce.TrainingPlan)
            .WithMany(tp => tp.CalendarEvents)
            .HasForeignKey(ce => ce.TrainingPlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}