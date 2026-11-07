using VolleyPlanner.API.DTOs.Subscription;

namespace VolleyPlanner.API.Interfaces;

public interface ISubscriptionService
{
    Task<SubscriptionStatusDto> GetStatusAsync(int userId);

    Task<CreateCheckoutSessionResponseDto> CreateCheckoutSessionAsync(
        int userId);

    Task HandleWebhookAsync(
    string json,
    string stripeSignature);

    Task CancelSubscriptionAsync(int userId);

    Task ReactivateSubscriptionAsync(int userId);

    Task<string> CreatePortalSessionAsync(int userId);

    Task SyncSubscriptionAsync(
        int userId,
        string? checkoutSessionId = null);
}