using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace VolleyPlanner.API.Tests;

public class ApiAuthorizationTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ApiAuthorizationTests(
        CustomWebApplicationFactory factory)
    {
        _client =
            factory.CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false
                });
    }

    [Theory]
    [InlineData("/api/TrainingSessions")]
    [InlineData("/api/CalendarEvents")]
    public async Task ProtectedEndpoint_WithoutToken_ReturnsUnauthorized(
        string url)
    {
        var response =
            await _client.GetAsync(url);

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithInvalidToken_ReturnsUnauthorized()
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                "invalid-test-token");

        var response =
            await _client.GetAsync(
                "/api/TrainingSessions");

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }
}