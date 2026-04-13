namespace VolleyPlanner.API.Entities;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    
    public bool IsEmailConfirmed { get; set; } = false;
    public string? EmailConfirmationToken { get; set; }
    public DateTime? EmailConfirmationTokenExpiresAt { get; set; }

    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiresAt { get; set; }
        
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public UserProfile? Profile { get; set; }

    public ICollection<TrainingPlan> TrainingPlans { get; set; } = new List<TrainingPlan>();
    public ICollection<GenerationRequest> GenerationRequests { get; set; } = new List<GenerationRequest>();
    public ICollection<CalendarEvent> CalendarEvents { get; set; } = new List<CalendarEvent>();

    public string Role { get; set; } = "User";
}