using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using VolleyPlanner.API.Data;
using VolleyPlanner.API.DTOs.Subscription;
using VolleyPlanner.API.Entities;
using VolleyPlanner.API.Enums;

namespace VolleyPlanner.API.Tests;

public class SubscriptionApiTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory
        _factory;

    public SubscriptionApiTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetMySubscription_AsFreeUser_ReturnsFreeStatus()
    {
        int userId;

        // 1. Teszt user létrehozása
        using (
            var scope =
                _factory.Services.CreateScope())
        {
            var context =
                scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

            var user =
                new User
                {
                    Name = "API Free User",
                    Email = "api-free@test.hu",
                    PasswordHash = "test",
                    Role = "User",
                    FreeGenerationsUsed = 2
                };

            context.Users.Add(user);

            await context.SaveChangesAsync();

            userId = user.Id;
        }

        // 2. HTTP kliens
        var client =
            _factory.CreateClient();

        // 3. Teszt-hitelesítés
        client.DefaultRequestHeaders.Add(
            "X-Test-UserId",
            userId.ToString());

        // 4. Valódi HTTP kérés
        var response =
            await client.GetAsync(
                "/api/Subscriptions/me");

        // 5. HTTP státusz ellenőrzése
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        // 6. JSON válasz deszerializálása
        var result =
            await response.Content
                .ReadFromJsonAsync<
                    SubscriptionStatusDto>();

        Assert.NotNull(result);

        // 7. Üzleti eredmény ellenőrzése
        Assert.Equal(
            "Free",
            result!.Plan);

        Assert.False(
            result.IsPremium);

        Assert.False(
            result.IsAdmin);

        Assert.Equal(
            2,
            result.FreeGenerationsUsed);

        Assert.Equal(
            5,
            result.FreeGenerationLimit);

        Assert.Equal(
            3,
            result.RemainingFreeGenerations);
    }

    [Fact]
    public async Task GetMySubscription_AsPremiumUser_ReturnsPremiumStatus()
    {
        int userId;

        using (
            var scope =
                _factory.Services.CreateScope())
        {
            var context =
                scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

            var user =
                new User
                {
                    Name = "API Premium User",
                    Email = "api-premium@test.hu",
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

            context.UserSubscriptions.Add(
                subscription);

            await context.SaveChangesAsync();

            userId = user.Id;
        }

        var client =
            _factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-Test-UserId",
            userId.ToString());

        var response =
            await client.GetAsync(
                "/api/Subscriptions/me");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<
                    SubscriptionStatusDto>();

        Assert.NotNull(result);

        Assert.Equal(
            "Premium",
            result!.Plan);

        Assert.True(
            result.IsPremium);

        Assert.False(
            result.IsAdmin);

        Assert.Equal(
            "Active",
            result.SubscriptionStatus);

        Assert.Equal(
            5,
            result.FreeGenerationsUsed);

        Assert.Equal(
            0,
            result.RemainingFreeGenerations);

        Assert.False(
            result.CancelAtPeriodEnd);
    }
}