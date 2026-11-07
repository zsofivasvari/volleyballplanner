using VolleyPlanner.API.Enums;

namespace VolleyPlanner.API.Entities;

public class UserSubscription
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public SubscriptionStatus Status { get; set; }
        = SubscriptionStatus.Inactive;

    // Stripe Customer azonosító
    public string? ProviderCustomerId { get; set; }

    // Stripe Subscription azonosító
    public string? ProviderSubscriptionId { get; set; }

    public DateTime? CurrentPeriodStart { get; set; }

    public DateTime? CurrentPeriodEnd { get; set; }

    // Ha true, a felhasználó lemondta,
    // de a már kifizetett időszak végéig Premium marad.
    public bool CancelAtPeriodEnd { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public DateTime? CancelledAt { get; set; }
}