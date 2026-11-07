using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace VolleyPlanner.API.Tests;

public class TestAuthHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult>
        HandleAuthenticateAsync()
    {
        // A tesztből headerben adjuk meg,
        // hogy melyik user legyen "bejelentkezve".
        if (!Request.Headers.TryGetValue(
                "X-Test-UserId",
                out var userIdValue))
        {
            return Task.FromResult(
                AuthenticateResult.NoResult());
        }

        var claims =
            new List<Claim>
            {
                new(
                    ClaimTypes.NameIdentifier,
                    userIdValue.ToString()),

                new(
                    ClaimTypes.Name,
                    "Test User"),

                new(
                    ClaimTypes.Email,
                    "test-api@test.hu"),

                new(
                    ClaimTypes.Role,
                    "User")
            };

        var identity =
            new ClaimsIdentity(
                claims,
                SchemeName);

        var principal =
            new ClaimsPrincipal(identity);

        var ticket =
            new AuthenticationTicket(
                principal,
                SchemeName);

        return Task.FromResult(
            AuthenticateResult.Success(ticket));
    }
}