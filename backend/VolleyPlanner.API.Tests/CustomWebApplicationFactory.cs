using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VolleyPlanner.API.Data;

namespace VolleyPlanner.API.Tests;

public class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection;

    public CustomWebApplicationFactory()
    {
        _connection =
            new SqliteConnection(
                "DataSource=:memory:");

        _connection.Open();
    }

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(
            services =>
            {
                // A valódi SQL Server konfiguráció eltávolítása.
                services.RemoveAll<
                    DbContextOptions<AppDbContext>>();


                services.RemoveAll<AppDbContext>();

                // Tesztekhez SQLite memória-adatbázis.
                services.AddDbContext<AppDbContext>(
                    options =>
                    {
                        options.UseSqlite(
                            _connection);
                    });

                // A valódi JWT helyett teszt-authentication.
                services
                    .AddAuthentication(
                        options =>
                        {
                            options.DefaultAuthenticateScheme =
                                TestAuthHandler.SchemeName;

                            options.DefaultChallengeScheme =
                                TestAuthHandler.SchemeName;

                            options.DefaultScheme =
                                TestAuthHandler.SchemeName;
                        })
                    .AddScheme<
                        AuthenticationSchemeOptions,
                        TestAuthHandler>(
                        TestAuthHandler.SchemeName,
                        _ =>
                        {
                        });

                // Létrehozzuk az SQLite adatbázist.
                var serviceProvider =
                    services.BuildServiceProvider();

                using var scope =
                    serviceProvider.CreateScope();

                var context =
                    scope.ServiceProvider
                        .GetRequiredService<AppDbContext>();

                context.Database.EnsureCreated();
            });
    }

    protected override void Dispose(
        bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection.Dispose();
        }
    }
}