namespace VolleyPlanner.API.DTOs.Subscription;

public class SubscriptionStatusDto
{
    public string Plan { get; set; } = "Free";

    public bool IsPremium { get; set; }
    
    public bool IsAdmin { get; set; }

    public int FreeGenerationsUsed { get; set; }

    public int FreeGenerationLimit { get; set; }

    public int RemainingFreeGenerations { get; set; }

    public string? SubscriptionStatus { get; set; }

    public DateTime? CurrentPeriodEnd { get; set; }

    public bool CancelAtPeriodEnd { get; set; }
}