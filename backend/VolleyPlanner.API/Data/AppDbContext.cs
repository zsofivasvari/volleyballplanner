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

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasOne(u => u.Profile)
            .WithOne(p => p.User)
            .HasForeignKey<UserProfile>(p => p.UserId);

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

        modelBuilder.Entity<TrainingPlan>()
            .HasOne(tp => tp.User)
            .WithMany(u => u.TrainingPlans)
            .HasForeignKey(tp => tp.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TrainingPlanItem>()
            .HasOne(tpi => tpi.TrainingPlan)
            .WithMany(tp => tp.Items)
            .HasForeignKey(tpi => tpi.TrainingPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TrainingPlanItem>()
            .HasOne(tpi => tpi.Exercise)
            .WithMany()
            .HasForeignKey(tpi => tpi.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<GenerationRequest>()
            .HasOne(gr => gr.User)
            .WithMany(u => u.GenerationRequests)
            .HasForeignKey(gr => gr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CalendarEvent>()
            .HasOne(ce => ce.User)
            .WithMany(u => u.CalendarEvents)
            .HasForeignKey(ce => ce.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<CalendarEvent>()
            .HasOne(ce => ce.TrainingPlan)
            .WithMany(tp => tp.CalendarEvents)
            .HasForeignKey(ce => ce.TrainingPlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}