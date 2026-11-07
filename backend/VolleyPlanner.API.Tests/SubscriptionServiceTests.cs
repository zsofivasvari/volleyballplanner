using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VolleyPlanner.API.Data;
using VolleyPlanner.API.Entities;
using VolleyPlanner.API.Enums;
using VolleyPlanner.API.Models;
using VolleyPlanner.API.Services;
using System.Reflection;

namespace VolleyPlanner.API.Tests;

public class SubscriptionServiceTests
{
    private static AppDbContext CreateContext(
        SqliteConnection connection)
    {
        var options =
            new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

        var context =
            new AppDbContext(options);

        context.Database.EnsureCreated();

        return context;
    }

    private static SubscriptionService CreateService(
        AppDbContext context)
    {
        var stripeSettings =
            Options.Create(
                new StripeSettings
                {
                    SecretKey = "test",
                    PremiumPriceId = "test",
                    WebhookSecret = "test"
                });

        return new SubscriptionService(
            context,
            stripeSettings);
    }

    [Fact]
    public async Task FreeUser_ReturnsFreePlan()
    {
        await using var connection =
            new SqliteConnection("DataSource=:memory:");

        await connection.OpenAsync();

        await using var context =
            CreateContext(connection);

        var user = new User
        {
            Name = "Free User",
            Email = "free@test.hu",
            PasswordHash = "test",
            Role = "User",
            FreeGenerationsUsed = 2
        };

        context.Users.Add(user);

        await context.SaveChangesAsync();

        var service =
            CreateService(context);

        var result =
            await service.GetStatusAsync(user.Id);

        Assert.Equal("Free", result.Plan);
        Assert.False(result.IsPremium);
        Assert.False(result.IsAdmin);
        Assert.Equal(2, result.FreeGenerationsUsed);
        Assert.Equal(3, result.RemainingFreeGenerations);
    }

    [Fact]
    public async Task ActiveSubscription_ReturnsPremiumPlan()
    {
        await using var connection =
            new SqliteConnection("DataSource=:memory:");

        await connection.OpenAsync();

        await using var context =
            CreateContext(connection);

        var user = new User
        {
            Name = "Premium User",
            Email = "premium@test.hu",
            PasswordHash = "test",
            Role = "User",
            FreeGenerationsUsed = 5
        };

        context.Users.Add(user);

        await context.SaveChangesAsync();

        var subscription =
            new UserSubscription
            {
                UserId = user.Id,
                Status = SubscriptionStatus.Active,
                CurrentPeriodStart =
                    DateTime.UtcNow.AddDays(-1),
                CurrentPeriodEnd =
                    DateTime.UtcNow.AddDays(30),
                CancelAtPeriodEnd = false
            };

        context.UserSubscriptions.Add(subscription);

        await context.SaveChangesAsync();

        var service =
            CreateService(context);

        var result =
            await service.GetStatusAsync(user.Id);

        Assert.Equal("Premium", result.Plan);
        Assert.True(result.IsPremium);
        Assert.False(result.IsAdmin);

        // Az 5 felhasznált ingyenes alkalom
        // Premium státusz mellett sem számít.
        Assert.Equal(5, result.FreeGenerationsUsed);
        Assert.Equal(0, result.RemainingFreeGenerations);
    }

    [Fact]
    public async Task PastDueSubscription_IsNotPremium()
    {
        await using var connection =
            new SqliteConnection("DataSource=:memory:");

        await connection.OpenAsync();

        await using var context =
            CreateContext(connection);

        var user = new User
        {
            Name = "PastDue User",
            Email = "pastdue@test.hu",
            PasswordHash = "test",
            Role = "User",
            FreeGenerationsUsed = 5
        };

        context.Users.Add(user);

        await context.SaveChangesAsync();

        var subscription =
            new UserSubscription
            {
                UserId = user.Id,
                Status = SubscriptionStatus.PastDue,
                CurrentPeriodStart =
                    DateTime.UtcNow.AddDays(-30),
                CurrentPeriodEnd =
                    DateTime.UtcNow.AddDays(1)
            };

        context.UserSubscriptions.Add(subscription);

        await context.SaveChangesAsync();

        var service =
            CreateService(context);

        var result =
            await service.GetStatusAsync(user.Id);

        Assert.Equal("Free", result.Plan);
        Assert.False(result.IsPremium);
        Assert.Equal(
            "PastDue",
            result.SubscriptionStatus);
    }

    [Fact]
    public async Task Admin_ReturnsAdminPlan()
    {
        await using var connection =
            new SqliteConnection("DataSource=:memory:");

        await connection.OpenAsync();

        await using var context =
            CreateContext(connection);

        var user = new User
        {
            Name = "Admin User",
            Email = "admin@test.hu",
            PasswordHash = "test",
            Role = "Admin",
            FreeGenerationsUsed = 5
        };

        context.Users.Add(user);

        await context.SaveChangesAsync();

        var service =
            CreateService(context);

        var result =
            await service.GetStatusAsync(user.Id);

        Assert.Equal("Admin", result.Plan);
        Assert.True(result.IsAdmin);
        Assert.False(result.IsPremium);
    }

    [Fact]
    public async Task CancelledAtPeriodEnd_RemainsPremiumUntilPeriodEnd()
    {
        await using var connection =
            new SqliteConnection("DataSource=:memory:");

        await connection.OpenAsync();

        await using var context =
            CreateContext(connection);

        var user = new User
        {
            Name = "Cancelled Premium User",
            Email = "cancelled@test.hu",
            PasswordHash = "test",
            Role = "User",
            FreeGenerationsUsed = 5
        };

        context.Users.Add(user);

        await context.SaveChangesAsync();

        context.UserSubscriptions.Add(
            new UserSubscription
            {
                UserId = user.Id,
                Status = SubscriptionStatus.Active,
                CurrentPeriodStart =
                    DateTime.UtcNow.AddDays(-5),
                CurrentPeriodEnd =
                    DateTime.UtcNow.AddDays(25),
                CancelAtPeriodEnd = true
            });

        await context.SaveChangesAsync();

        var service =
            CreateService(context);

        var result =
            await service.GetStatusAsync(user.Id);

        Assert.Equal("Premium", result.Plan);
        Assert.True(result.IsPremium);
        Assert.True(result.CancelAtPeriodEnd);
    }

    [Fact]
    public async Task ExpiredActiveSubscription_IsNotPremium()
    {
        await using var connection =
            new SqliteConnection("DataSource=:memory:");

        await connection.OpenAsync();

        await using var context =
            CreateContext(connection);

        var user = new User
        {
            Name = "Expired User",
            Email = "expired@test.hu",
            PasswordHash = "test",
            Role = "User",
            FreeGenerationsUsed = 5
        };

        context.Users.Add(user);

        await context.SaveChangesAsync();

        context.UserSubscriptions.Add(
            new UserSubscription
            {
                UserId = user.Id,
                Status = SubscriptionStatus.Active,
                CurrentPeriodStart =
                    DateTime.UtcNow.AddDays(-40),
                CurrentPeriodEnd =
                    DateTime.UtcNow.AddDays(-1),
                CancelAtPeriodEnd = false
            });

        await context.SaveChangesAsync();

        var service =
            CreateService(context);

        var result =
            await service.GetStatusAsync(user.Id);

        Assert.Equal("Free", result.Plan);
        Assert.False(result.IsPremium);
    }

    [Fact]
    public async Task FreeGenerationRemaining_NeverBecomesNegative()
    {
        await using var connection =
            new SqliteConnection("DataSource=:memory:");

        await connection.OpenAsync();

        await using var context =
            CreateContext(connection);

        var user = new User
        {
            Name = "Quota Test User",
            Email = "quota@test.hu",
            PasswordHash = "test",
            Role = "User",

            // Direkt hibás/extrém érték.
            FreeGenerationsUsed = 8
        };

        context.Users.Add(user);

        await context.SaveChangesAsync();

        var service =
            CreateService(context);

        var result =
            await service.GetStatusAsync(user.Id);

        Assert.Equal(0, result.RemainingFreeGenerations);
    }

    [Fact]
    public async Task SubscriptionUpdated_WithCancelAt_RemainsActiveButIsMarkedForCancellation()
    {
        await using var connection =
            new SqliteConnection("DataSource=:memory:");

        await connection.OpenAsync();

        await using var context =
            CreateContext(connection);

        var user = new User
        {
            Name = "Portal Cancel User",
            Email = "portal-cancel@test.hu",
            PasswordHash = "test",
            Role = "User"
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var localSubscription =
            new UserSubscription
            {
                UserId = user.Id,
                ProviderSubscriptionId = "sub_cancel_test",
                ProviderCustomerId = "cus_cancel_test",
                Status = SubscriptionStatus.Active,
                CancelAtPeriodEnd = false
            };

        context.UserSubscriptions.Add(
            localSubscription);

        await context.SaveChangesAsync();

        var service =
            CreateService(context);

        var stripeSubscription =
            new Stripe.Subscription
            {
                Id = "sub_cancel_test",
                Status = "active",

                CancelAtPeriodEnd = false,

                // A Portal ezt is használhatja.
                CancelAt =
                    DateTime.UtcNow.AddDays(20)
            };

        await InvokePrivateAsync(
            service,
            "HandleSubscriptionUpdatedAsync",
            stripeSubscription);

        await context.Entry(
            localSubscription).ReloadAsync();

        Assert.Equal(
            SubscriptionStatus.Active,
            localSubscription.Status);

        Assert.True(
            localSubscription.CancelAtPeriodEnd);
    }

    [Fact]
    public async Task SubscriptionUpdated_AfterReactivation_RemovesScheduledCancellation()
    {
        await using var connection =
            new SqliteConnection("DataSource=:memory:");

        await connection.OpenAsync();

        await using var context =
            CreateContext(connection);

        var user = new User
        {
            Name = "Reactivate User",
            Email = "reactivate@test.hu",
            PasswordHash = "test",
            Role = "User"
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var localSubscription =
            new UserSubscription
            {
                UserId = user.Id,
                ProviderSubscriptionId =
                    "sub_reactivate_test",

                Status =
                    SubscriptionStatus.Active,

                CancelAtPeriodEnd = true
            };

        context.UserSubscriptions.Add(
            localSubscription);

        await context.SaveChangesAsync();

        var service =
            CreateService(context);

        var stripeSubscription =
            new Stripe.Subscription
            {
                Id = "sub_reactivate_test",
                Status = "active",
                CancelAtPeriodEnd = false,
                CancelAt = null
            };

        await InvokePrivateAsync(
            service,
            "HandleSubscriptionUpdatedAsync",
            stripeSubscription);

        await context.Entry(
            localSubscription).ReloadAsync();

        Assert.Equal(
            SubscriptionStatus.Active,
            localSubscription.Status);

        Assert.False(
            localSubscription.CancelAtPeriodEnd);
    }

    [Fact]
    public async Task SubscriptionUpdated_PastDue_SetsSubscriptionToPastDue()
    {
        await using var connection =
            new SqliteConnection("DataSource=:memory:");

        await connection.OpenAsync();

        await using var context =
            CreateContext(connection);

        var user = new User
        {
            Name = "Past Due Webhook User",
            Email = "webhook-pastdue@test.hu",
            PasswordHash = "test",
            Role = "User"
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var localSubscription =
            new UserSubscription
            {
                UserId = user.Id,

                ProviderSubscriptionId =
                    "sub_pastdue_test",

                Status =
                    SubscriptionStatus.Active
            };

        context.UserSubscriptions.Add(
            localSubscription);

        await context.SaveChangesAsync();

        var service =
            CreateService(context);

        var stripeSubscription =
            new Stripe.Subscription
            {
                Id = "sub_pastdue_test",
                Status = "past_due"
            };

        await InvokePrivateAsync(
            service,
            "HandleSubscriptionUpdatedAsync",
            stripeSubscription);

        await context.Entry(
            localSubscription).ReloadAsync();

        Assert.Equal(
            SubscriptionStatus.PastDue,
            localSubscription.Status);

        var status =
            await service.GetStatusAsync(
                user.Id);

        Assert.False(status.IsPremium);
    }

    [Fact]
    public async Task InvoicePaymentFailed_SetsSubscriptionToPastDue()
    {
        await using var connection =
            new SqliteConnection("DataSource=:memory:");

        await connection.OpenAsync();

        await using var context =
            CreateContext(connection);

        var user = new User
        {
            Name = "Failed Payment User",
            Email = "failed-payment@test.hu",
            PasswordHash = "test",
            Role = "User"
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var localSubscription =
            new UserSubscription
            {
                UserId = user.Id,

                ProviderCustomerId =
                    "cus_failed_test",

                ProviderSubscriptionId =
                    "sub_failed_test",

                Status =
                    SubscriptionStatus.Active
            };

        context.UserSubscriptions.Add(
            localSubscription);

        await context.SaveChangesAsync();

        var service =
            CreateService(context);

        var invoice =
            new Stripe.Invoice
            {
                CustomerId =
                    "cus_failed_test"
            };

        await InvokePrivateAsync(
            service,
            "HandleInvoicePaymentFailedAsync",
            invoice);

        await context.Entry(
            localSubscription).ReloadAsync();

        Assert.Equal(
            SubscriptionStatus.PastDue,
            localSubscription.Status);

        var status =
            await service.GetStatusAsync(
                user.Id);

        Assert.False(status.IsPremium);
    }

    [Fact]
    public async Task SubscriptionDeleted_SetsSubscriptionToCancelled()
    {
        await using var connection =
            new SqliteConnection("DataSource=:memory:");

        await connection.OpenAsync();

        await using var context =
            CreateContext(connection);

        var user = new User
        {
            Name = "Deleted Subscription User",
            Email = "deleted@test.hu",
            PasswordHash = "test",
            Role = "User"
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var localSubscription =
            new UserSubscription
            {
                UserId = user.Id,

                ProviderSubscriptionId =
                    "sub_deleted_test",

                Status =
                    SubscriptionStatus.Active,

                CancelAtPeriodEnd = true
            };

        context.UserSubscriptions.Add(
            localSubscription);

        await context.SaveChangesAsync();

        var service =
            CreateService(context);

        var stripeSubscription =
            new Stripe.Subscription
            {
                Id = "sub_deleted_test",
                Status = "canceled"
            };

        await InvokePrivateAsync(
            service,
            "HandleSubscriptionDeletedAsync",
            stripeSubscription);

        await context.Entry(
            localSubscription).ReloadAsync();

        Assert.Equal(
            SubscriptionStatus.Cancelled,
            localSubscription.Status);

        Assert.False(
            localSubscription.CancelAtPeriodEnd);

        Assert.NotNull(
            localSubscription.CancelledAt);

        var status =
            await service.GetStatusAsync(
                user.Id);

        Assert.False(status.IsPremium);
    }

    private static async Task InvokePrivateAsync(
        SubscriptionService service,
        string methodName,
        object argument)
    {
        var method =
            typeof(SubscriptionService).GetMethod(
                methodName,
                BindingFlags.Instance |
                BindingFlags.NonPublic);

        Assert.NotNull(method);

        var task =
            method!.Invoke(
                service,
                new[] { argument }) as Task;

        Assert.NotNull(task);

        await task!;
    }
}