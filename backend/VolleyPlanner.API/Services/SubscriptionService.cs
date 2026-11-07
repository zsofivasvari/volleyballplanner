using Microsoft.EntityFrameworkCore;
using VolleyPlanner.API.Constants;
using VolleyPlanner.API.Data;
using VolleyPlanner.API.DTOs.Subscription;
using VolleyPlanner.API.Enums;
using VolleyPlanner.API.Interfaces;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using VolleyPlanner.API.Models;
using AppSubscriptionStatus =
    VolleyPlanner.API.Enums.SubscriptionStatus;

namespace VolleyPlanner.API.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly AppDbContext _context;
    private readonly StripeSettings _stripeSettings;

    public SubscriptionService(
        AppDbContext context,
        IOptions<StripeSettings> stripeSettings)
    {
        _context = context;
        _stripeSettings = stripeSettings.Value;
    }

    public async Task<SubscriptionStatusDto> GetStatusAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Subscription)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new Exception("A felhasználó nem található.");
        }

        var isAdmin = string.Equals(
            user.Role,
            "Admin",
            StringComparison.OrdinalIgnoreCase);

        var subscription = user.Subscription;

        var isPremium =
            subscription != null &&
            subscription.Status == SubscriptionStatus.Active &&
            (
                !subscription.CurrentPeriodEnd.HasValue ||
                subscription.CurrentPeriodEnd.Value > DateTime.UtcNow
            );

        var remainingFreeGenerations = Math.Max(
            0,
            PremiumPlanConstants.FreeGenerationLimit -
            user.FreeGenerationsUsed);

        return new SubscriptionStatusDto
        {
            Plan = isAdmin
                ? "Admin"
                : isPremium
                    ? "Premium"
                    : "Free",

            IsPremium = isPremium,

            IsAdmin = isAdmin,

            FreeGenerationsUsed =
                user.FreeGenerationsUsed,

            FreeGenerationLimit =
                PremiumPlanConstants.FreeGenerationLimit,

            RemainingFreeGenerations =
                remainingFreeGenerations,

            SubscriptionStatus =
                subscription?.Status.ToString(),

            CurrentPeriodEnd =
                subscription?.CurrentPeriodEnd,

            CancelAtPeriodEnd =
                subscription?.CancelAtPeriodEnd ?? false
        };
    }

    public async Task<CreateCheckoutSessionResponseDto>
        CreateCheckoutSessionAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Subscription)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new Exception(
                "A felhasználó nem található.");
        }

        var isAdmin = string.Equals(
            user.Role,
            "Admin",
            StringComparison.OrdinalIgnoreCase);

        if (isAdmin)
        {
            throw new Exception(
                "Admin felhasználónak nincs szüksége Premium előfizetésre.");
        }

        var hasActiveSubscription =
            user.Subscription != null &&
            user.Subscription.Status ==
                SubscriptionStatus.Active &&
            (
                !user.Subscription.CurrentPeriodEnd.HasValue ||
                user.Subscription.CurrentPeriodEnd.Value >
                    DateTime.UtcNow
            );

        if (hasActiveSubscription)
        {
            throw new Exception(
                "Már rendelkezel aktív Premium előfizetéssel.");
        }

        if (string.IsNullOrWhiteSpace(
            _stripeSettings.SecretKey))
        {
            throw new Exception(
                "A Stripe SecretKey nincs konfigurálva.");
        }

        if (string.IsNullOrWhiteSpace(
            _stripeSettings.PremiumPriceId))
        {
            throw new Exception(
                "A Stripe PremiumPriceId nincs konfigurálva.");
        }

        StripeConfiguration.ApiKey =
            _stripeSettings.SecretKey;

        var options =
            new SessionCreateOptions
            {
                Mode = "subscription",

                SuccessUrl =
                    "http://localhost:5173/profile?subscription=success",

                CancelUrl =
                    "http://localhost:5173/profile?subscription=cancelled",

                CustomerEmail = user.Email,

                LineItems = new List<SessionLineItemOptions>
                {
                    new()
                    {
                        Price =
                            _stripeSettings.PremiumPriceId,

                        Quantity = 1
                    }
                },

                Metadata = new Dictionary<string, string>
                {
                    {
                        "userId",
                        user.Id.ToString()
                    }
                },

                SubscriptionData =
                    new SessionSubscriptionDataOptions
                    {
                        Metadata =
                            new Dictionary<string, string>
                            {
                                {
                                    "userId",
                                    user.Id.ToString()
                                }
                            }
                    }
            };

        var service = new SessionService();

        var session =
            await service.CreateAsync(options);

        if (string.IsNullOrWhiteSpace(session.Url))
        {
            throw new Exception(
                "A Stripe Checkout URL nem jött létre.");
        }

        return new CreateCheckoutSessionResponseDto
        {
            CheckoutUrl = session.Url
        };
    }

    public async Task CancelSubscriptionAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Subscription)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new Exception(
                "A felhasználó nem található.");
        }

        var subscription = user.Subscription;

        if (subscription == null ||
            subscription.Status != SubscriptionStatus.Active)
        {
            throw new Exception(
                "Nincs aktív Premium előfizetésed.");
        }

        if (string.IsNullOrWhiteSpace(
            subscription.ProviderSubscriptionId))
        {
            throw new Exception(
                "A Stripe előfizetés azonosítója nem található.");
        }

        if (subscription.CancelAtPeriodEnd)
        {
            throw new Exception(
                "Az előfizetés lemondása már be van állítva.");
        }

        if (string.IsNullOrWhiteSpace(
            _stripeSettings.SecretKey))
        {
            throw new Exception(
                "A Stripe SecretKey nincs konfigurálva.");
        }

        StripeConfiguration.ApiKey =
            _stripeSettings.SecretKey;

        var stripeSubscriptionService =
            new Stripe.SubscriptionService();

        var options =
            new SubscriptionUpdateOptions
            {
                CancelAtPeriodEnd = true
            };

        await stripeSubscriptionService.UpdateAsync(
            subscription.ProviderSubscriptionId,
            options);
    }

    public async Task ReactivateSubscriptionAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Subscription)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new Exception(
                "A felhasználó nem található.");
        }

        var subscription = user.Subscription;

        if (subscription == null ||
            subscription.Status != SubscriptionStatus.Active)
        {
            throw new Exception(
                "Nincs aktív Premium előfizetésed.");
        }

        if (!subscription.CancelAtPeriodEnd)
        {
            throw new Exception(
                "Az előfizetés automatikus megújítása már aktív.");
        }

        if (string.IsNullOrWhiteSpace(
            subscription.ProviderSubscriptionId))
        {
            throw new Exception(
                "A Stripe előfizetés azonosítója nem található.");
        }

        if (string.IsNullOrWhiteSpace(
            _stripeSettings.SecretKey))
        {
            throw new Exception(
                "A Stripe SecretKey nincs konfigurálva.");
        }

        StripeConfiguration.ApiKey =
            _stripeSettings.SecretKey;

        var stripeSubscriptionService =
            new Stripe.SubscriptionService();

        var options =
            new SubscriptionUpdateOptions
            {
                CancelAtPeriodEnd = false
            };

        await stripeSubscriptionService.UpdateAsync(
            subscription.ProviderSubscriptionId,
            options);
    }

    public async Task<string> CreatePortalSessionAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Subscription)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new Exception(
                "A felhasználó nem található.");
        }

        var subscription = user.Subscription;

        if (subscription == null ||
            string.IsNullOrWhiteSpace(
                subscription.ProviderCustomerId))
        {
            throw new Exception(
                "Ehhez a felhasználóhoz nem tartozik Stripe ügyfél.");
        }

        if (string.IsNullOrWhiteSpace(
            _stripeSettings.SecretKey))
        {
            throw new Exception(
                "A Stripe SecretKey nincs konfigurálva.");
        }

        StripeConfiguration.ApiKey =
            _stripeSettings.SecretKey;

        var options =
            new Stripe.BillingPortal.SessionCreateOptions
            {
                Customer =
                    subscription.ProviderCustomerId,

                ReturnUrl =
                    "http://localhost:5173/profile"
            };

        var service =
            new Stripe.BillingPortal.SessionService();

        var session =
            await service.CreateAsync(options);

        if (string.IsNullOrWhiteSpace(session.Url))
        {
            throw new Exception(
                "A Stripe Customer Portal URL nem jött létre.");
        }

        return session.Url;
    }

    public async Task HandleWebhookAsync(
        string json,
        string stripeSignature)
    {
        if (string.IsNullOrWhiteSpace(
            _stripeSettings.WebhookSecret))
        {
            throw new Exception(
                "A Stripe WebhookSecret nincs konfigurálva.");
        }

        StripeConfiguration.ApiKey =
            _stripeSettings.SecretKey;

        Event stripeEvent;

        try
        {
            stripeEvent =
                EventUtility.ConstructEvent(
                    json,
                    stripeSignature,
                    _stripeSettings.WebhookSecret);
        }
        catch (StripeException)
        {
            throw new Exception(
                "Érvénytelen Stripe webhook aláírás.");
        }

        switch (stripeEvent.Type)
        {
            case "checkout.session.completed":
            {
                var session =
                    stripeEvent.Data.Object
                        as Session;

                if (session != null)
                {
                    await HandleCheckoutCompletedAsync(
                        session);
                }

                break;
            }

            case "customer.subscription.updated":
            {
                var stripeSubscription =
                    stripeEvent.Data.Object
                        as Stripe.Subscription;

                if (stripeSubscription != null)
                {
                    await HandleSubscriptionUpdatedAsync(
                        stripeSubscription);
                }

                break;
            }

           case "invoice.payment_failed":
            {
                var invoice =
                    stripeEvent.Data.Object
                        as Invoice;

                if (invoice != null)
                {
                    await HandleInvoicePaymentFailedAsync(
                        invoice);
                }

                break;
            }

            case "customer.subscription.deleted":
            {
                var stripeSubscription =
                    stripeEvent.Data.Object
                        as Stripe.Subscription;

                if (stripeSubscription != null)
                {
                    await HandleSubscriptionDeletedAsync(
                        stripeSubscription);
                }

                break;
            }
        }
    }

    private async Task HandleCheckoutCompletedAsync(
        Session session)
    {
        if (session.Metadata == null ||
            !session.Metadata.TryGetValue(
                "userId",
                out var userIdValue) ||
            !int.TryParse(
                userIdValue,
                out var userId))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(
            session.SubscriptionId))
        {
            return;
        }

        var stripeSubscriptionService =
            new Stripe.SubscriptionService();

        var stripeSubscription =
            await stripeSubscriptionService.GetAsync(
                session.SubscriptionId);

        var user = await _context.Users
            .Include(u => u.Subscription)
            .FirstOrDefaultAsync(
                u => u.Id == userId);

        if (user == null)
        {
            return;
        }

        var subscription =
            user.Subscription;

        if (subscription == null)
        {
            subscription =
                new Entities.UserSubscription
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

            _context.UserSubscriptions.Add(
                subscription);
        }

        subscription.ProviderCustomerId =
            session.CustomerId;

        subscription.ProviderSubscriptionId =
            stripeSubscription.Id;

        subscription.Status =
            AppSubscriptionStatus.Active;

        subscription.CancelAtPeriodEnd =
            stripeSubscription.CancelAtPeriodEnd ||
            stripeSubscription.CancelAt.HasValue;

        var firstItem =
            stripeSubscription.Items?.Data?
                .FirstOrDefault();

        if (firstItem != null)
        {
            subscription.CurrentPeriodStart =
                firstItem.CurrentPeriodStart;

            subscription.CurrentPeriodEnd =
                firstItem.CurrentPeriodEnd;
        }

        subscription.CancelledAt = null;
        subscription.UpdatedAt =
            DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    private async Task HandleSubscriptionUpdatedAsync(
        Stripe.Subscription stripeSubscription)
    {
        var subscription =
            await _context.UserSubscriptions
                .FirstOrDefaultAsync(
                    s =>
                        s.ProviderSubscriptionId ==
                        stripeSubscription.Id);

        if (subscription == null)
        {
            return;
        }

        subscription.CancelAtPeriodEnd =
            stripeSubscription.CancelAtPeriodEnd ||
            stripeSubscription.CancelAt.HasValue;

        var firstItem =
            stripeSubscription.Items?.Data?
                .FirstOrDefault();

        if (firstItem != null)
        {
            subscription.CurrentPeriodStart =
                firstItem.CurrentPeriodStart;

            subscription.CurrentPeriodEnd =
                firstItem.CurrentPeriodEnd;
        }

        subscription.Status =
            stripeSubscription.Status switch
            {
                "active" =>
                    AppSubscriptionStatus.Active,

                "trialing" =>
                    AppSubscriptionStatus.Active,

                "past_due" =>
                    AppSubscriptionStatus.PastDue,

                "canceled" =>
                    AppSubscriptionStatus.Cancelled,

                _ =>
                    AppSubscriptionStatus.Inactive
            };

        if (subscription.Status ==
            AppSubscriptionStatus.Cancelled)
        {
            subscription.CancelledAt =
                DateTime.UtcNow;
        }

        subscription.UpdatedAt =
            DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    private async Task HandleInvoicePaymentFailedAsync(
        Invoice invoice)
    {
        if (string.IsNullOrWhiteSpace(
            invoice.CustomerId))
        {
            return;
        }

        var subscription =
            await _context.UserSubscriptions
                .FirstOrDefaultAsync(
                    s =>
                        s.ProviderCustomerId ==
                        invoice.CustomerId);

        if (subscription == null)
        {
            return;
        }

        subscription.Status =
            AppSubscriptionStatus.PastDue;

        subscription.UpdatedAt =
            DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    private async Task HandleSubscriptionDeletedAsync(
        Stripe.Subscription stripeSubscription)
    {
        var subscription =
            await _context.UserSubscriptions
                .FirstOrDefaultAsync(
                    s =>
                        s.ProviderSubscriptionId ==
                        stripeSubscription.Id);

        if (subscription == null)
        {
            return;
        }

        subscription.Status =
            AppSubscriptionStatus.Cancelled;

        subscription.CancelAtPeriodEnd =
            false;

        subscription.CancelledAt =
            DateTime.UtcNow;

        subscription.UpdatedAt =
            DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }
}