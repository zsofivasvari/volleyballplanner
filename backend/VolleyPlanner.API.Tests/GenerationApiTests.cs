using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using VolleyPlanner.API.DTOs.Generation;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VolleyPlanner.API.Data;
using VolleyPlanner.API.Entities;
using VolleyPlanner.API.Enums;

namespace VolleyPlanner.API.Tests;

public class GenerationApiTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public GenerationApiTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PostGeneration_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

        var request = new GenerateTrainingPlanRequestDto
        {
            SportTypes = new List<string>
            {
                "BeachVolleyball"
            },
            DurationMin = 60,
            PlayerCount = 2,
            Intensities = new List<string>
            {
                "Low"
            },
            Difficulties = new List<string>
            {
                "Beginner"
            },
            FocusAreas = new List<string>
            {
                "Reception"
            }
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/Generation",
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }


    [Fact]
    public async Task PostGeneration_AsFreeUser_ReturnsPlanAndConsumesOneQuota()
    {
        int userId;

        var difficulty =
            Enum.GetValues<DifficultyLevel>().First();

        var intensity =
            Enum.GetValues<IntensityLevel>().First();

        // 1. Tesztadatok létrehozása
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
                    Name = "Generation API Free User",
                    Email =
                        $"generation-api-{Guid.NewGuid()}@test.hu",
                    PasswordHash = "test",
                    Role = "User",
                    FreeGenerationsUsed = 0
                };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            userId = user.Id;

            var focusTag =
                new Tag
                {
                    Name = "Fullbody",
                    Type = TagType.Focus
                };

            context.Tags.Add(focusTag);
            await context.SaveChangesAsync();

            var exercise =
                new Exercise
                {
                    Title = "API teszt bemelegítés",
                    Description =
                        "Automatizált API integrációs teszt.",

                    SportType = SportType.Gym,
                    DurationMin = 10,

                    Difficulty = difficulty,
                    Intensity = intensity,

                    MinPlayers = 1,
                    MaxPlayers = 20,

                    Phase = ExercisePhase.Warmup
                };

            context.Exercises.Add(exercise);
            await context.SaveChangesAsync();

            context.ExerciseTags.Add(
                new ExerciseTag
                {
                    ExerciseId = exercise.Id,
                    TagId = focusTag.Id
                });

            await context.SaveChangesAsync();
        }

        // 2. HTTP kliens
        var client =
            _factory.CreateClient();

        // A TestAuthHandler ebből készíti el
        // a NameIdentifier claimet.
        client.DefaultRequestHeaders.Add(
            "X-Test-UserId",
            userId.ToString());

        // 3. Generálási kérés
        var request =
            new GenerateTrainingPlanRequestDto
            {
                SportTypes =
                    new List<string>
                    {
                        SportType.Gym.ToString()
                    },

                Difficulties =
                    new List<string>
                    {
                        difficulty.ToString()
                    },

                Intensities =
                    new List<string>
                    {
                        intensity.ToString()
                    },

                FocusAreas =
                    new List<string>
                    {
                        "Fullbody"
                    },

                DurationMin = 10
            };

        // 4. Valódi HTTP POST
        var response =
            await client.PostAsJsonAsync(
                "/api/Generation",
                request);

        // 5. HTTP válasz ellenőrzése
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<
                    GeneratedTrainingPlanResponseDto>();

        Assert.NotNull(result);

        Assert.Equal(
            SportType.Gym.ToString(),
            result!.SportType);

        Assert.Equal(
            10,
            result.TargetDuration);

        Assert.NotEmpty(
            result.Items);

        Assert.Equal(
            10,
            result.Items.Sum(
                item =>
                    item.PlannedDuration));

        Assert.Contains(
            result.Items,
            item =>
                item.ExerciseTitle ==
                "API teszt bemelegítés");

        // 6. Ellenőrizzük az adatbázisban,
        // hogy fogyott-e egy Free generálás.
        using (
            var scope =
                _factory.Services.CreateScope())
        {
            var context =
                scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

            var updatedUser =
                await context.Users
                    .SingleAsync(
                        user =>
                            user.Id == userId);

            Assert.Equal(
                1,
                updatedUser.FreeGenerationsUsed);
        }
    }

    [Fact]
    public async Task PostGeneration_AsFreeUserWithNoRemainingQuota_ReturnsBadRequest()
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
                    Name = "Quota Exhausted User",
                    Email =
                        $"quota-exhausted-{Guid.NewGuid()}@test.hu",
                    PasswordHash = "test",
                    Role = "User",
                    FreeGenerationsUsed = 5
                };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            userId = user.Id;
        }

        var client =
            _factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-Test-UserId",
            userId.ToString());

        var request =
            new GenerateTrainingPlanRequestDto
            {
                SportTypes = new List<string>
                {
                    SportType.Gym.ToString()
                },

                DurationMin = 10,

                Intensities = new List<string>
                {
                    Enum.GetValues<IntensityLevel>()
                        .First()
                        .ToString()
                },

                Difficulties = new List<string>
                {
                    Enum.GetValues<DifficultyLevel>()
                        .First()
                        .ToString()
                },

                FocusAreas = new List<string>
                {
                    "Fullbody"
                }
            };

        var response =
            await client.PostAsJsonAsync(
                "/api/Generation",
                request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        using (
            var scope =
                _factory.Services.CreateScope())
        {
            var context =
                scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

            var user =
                await context.Users
                    .SingleAsync(
                        u => u.Id == userId);

            Assert.Equal(
                5,
                user.FreeGenerationsUsed);
        }
    }

    [Fact]
    public async Task PostGeneration_AsPremiumUserWithUsedFreeQuota_ReturnsOkAndDoesNotConsumeQuota()
    {
        int userId;

        var difficulty =
            Enum.GetValues<DifficultyLevel>().First();

        var intensity =
            Enum.GetValues<IntensityLevel>().First();

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
                    Name = "Premium Generation User",
                    Email =
                        $"premium-generation-{Guid.NewGuid()}@test.hu",
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
                    Status =
                        SubscriptionStatus.Active,

                    CurrentPeriodStart =
                        DateTime.UtcNow.AddDays(-1),

                    CurrentPeriodEnd =
                        DateTime.UtcNow.AddDays(30),

                    CancelAtPeriodEnd = false
                });

            var tag =
                new Tag
                {
                    Name = "Fullbody",
                    Type = TagType.Focus
                };

            context.Tags.Add(tag);
            await context.SaveChangesAsync();

            var exercise =
                new Exercise
                {
                    Title =
                        $"Premium API Exercise {Guid.NewGuid()}",

                    Description =
                        "Premium API teszt.",

                    SportType = SportType.Gym,
                    DurationMin = 10,
                    Difficulty = difficulty,
                    Intensity = intensity,
                    MinPlayers = 1,
                    MaxPlayers = 20,
                    Phase = ExercisePhase.Warmup
                };

            context.Exercises.Add(exercise);
            await context.SaveChangesAsync();

            context.ExerciseTags.Add(
                new ExerciseTag
                {
                    ExerciseId = exercise.Id,
                    TagId = tag.Id
                });

            await context.SaveChangesAsync();

            userId = user.Id;
        }

        var client =
            _factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-Test-UserId",
            userId.ToString());

        var request =
            new GenerateTrainingPlanRequestDto
            {
                SportTypes = new List<string>
                {
                    SportType.Gym.ToString()
                },

                DurationMin = 10,

                Intensities = new List<string>
                {
                    intensity.ToString()
                },

                Difficulties = new List<string>
                {
                    difficulty.ToString()
                },

                FocusAreas = new List<string>
                {
                    "Fullbody"
                }
            };

        var response =
            await client.PostAsJsonAsync(
                "/api/Generation",
                request);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<
                    GeneratedTrainingPlanResponseDto>();

        Assert.NotNull(result);
        Assert.NotEmpty(result!.Items);

        using (
            var scope =
                _factory.Services.CreateScope())
        {
            var context =
                scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

            var user =
                await context.Users
                    .SingleAsync(
                        u => u.Id == userId);

            // Premiumként NEM fogy a Free kvóta.
            Assert.Equal(
                5,
                user.FreeGenerationsUsed);
        }
    }

    [Fact]
    public async Task PostGeneration_AsPastDueUserWithUsedQuota_ReturnsBadRequest()
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
                    Name = "PastDue API User",
                    Email =
                        $"pastdue-api-{Guid.NewGuid()}@test.hu",
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

                    Status =
                        SubscriptionStatus.PastDue,

                    CurrentPeriodStart =
                        DateTime.UtcNow.AddDays(-5),

                    CurrentPeriodEnd =
                        DateTime.UtcNow.AddDays(25),

                    CancelAtPeriodEnd = false
                });

            await context.SaveChangesAsync();

            userId = user.Id;
        }

        var client =
            _factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-Test-UserId",
            userId.ToString());

        var request =
            new GenerateTrainingPlanRequestDto
            {
                SportTypes = new List<string>
                {
                    SportType.Gym.ToString()
                },

                DurationMin = 10,

                Intensities = new List<string>
                {
                    Enum.GetValues<IntensityLevel>()
                        .First()
                        .ToString()
                },

                Difficulties = new List<string>
                {
                    Enum.GetValues<DifficultyLevel>()
                        .First()
                        .ToString()
                },

                FocusAreas = new List<string>
                {
                    "Fullbody"
                }
            };

        var response =
            await client.PostAsJsonAsync(
                "/api/Generation",
                request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        using (
            var scope =
                _factory.Services.CreateScope())
        {
            var context =
                scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

            var user =
                await context.Users
                    .SingleAsync(
                        u => u.Id == userId);

            Assert.Equal(
                5,
                user.FreeGenerationsUsed);
        }
    }

    [Fact]
    public async Task FailedGeneration_AsFreeUser_DoesNotConsumeQuota()
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
                    Name = "Failed Generation User",
                    Email =
                        $"failed-generation-{Guid.NewGuid()}@test.hu",
                    PasswordHash = "test",
                    Role = "User",
                    FreeGenerationsUsed = 2
                };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            userId = user.Id;
        }

        var client =
            _factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-Test-UserId",
            userId.ToString());

        var request =
            new GenerateTrainingPlanRequestDto
            {
                SportTypes = new List<string>
                {
                    SportType.Gym.ToString()
                },

                // Szándékosan olyan nagy idő,
                // amit a tesztadatbázis nem tud kitölteni.
                DurationMin = 100000,

                Intensities = new List<string>
                {
                    Enum.GetValues<IntensityLevel>()
                        .First()
                        .ToString()
                },

                Difficulties = new List<string>
                {
                    Enum.GetValues<DifficultyLevel>()
                        .First()
                        .ToString()
                },

                FocusAreas = new List<string>
                {
                    "Fullbody"
                }
            };

        var response =
            await client.PostAsJsonAsync(
                "/api/Generation",
                request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        using (
            var scope =
                _factory.Services.CreateScope())
        {
            var context =
                scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

            var user =
                await context.Users
                    .SingleAsync(
                        u => u.Id == userId);

            // Sikertelen generálásért nem fizet kvótával.
            Assert.Equal(
                2,
                user.FreeGenerationsUsed);
        }
    }

    [Fact]
    public async Task PostGeneration_AsCancelledAtPeriodEndPremiumUser_StillReturnsOk()
    {
        int userId;

        var difficulty =
            Enum.GetValues<DifficultyLevel>().First();

        var intensity =
            Enum.GetValues<IntensityLevel>().First();

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
                    Name = "Cancelling Premium User",
                    Email =
                        $"cancel-premium-{Guid.NewGuid()}@test.hu",
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
                        DateTime.UtcNow.AddDays(-10),

                    CurrentPeriodEnd =
                        DateTime.UtcNow.AddDays(20),

                    CancelAtPeriodEnd = true,
                    CancelledAt = DateTime.UtcNow
                });

            var tag =
                new Tag
                {
                    Name = "Fullbody",
                    Type = TagType.Focus
                };

            context.Tags.Add(tag);
            await context.SaveChangesAsync();

            var exercise =
                new Exercise
                {
                    Title =
                        $"Cancel Premium Exercise {Guid.NewGuid()}",

                    Description = "Test exercise.",

                    SportType = SportType.Gym,
                    DurationMin = 10,
                    Difficulty = difficulty,
                    Intensity = intensity,
                    MinPlayers = 1,
                    MaxPlayers = 20,
                    Phase = ExercisePhase.Warmup
                };

            context.Exercises.Add(exercise);
            await context.SaveChangesAsync();

            context.ExerciseTags.Add(
                new ExerciseTag
                {
                    ExerciseId = exercise.Id,
                    TagId = tag.Id
                });

            await context.SaveChangesAsync();

            userId = user.Id;
        }

        var client =
            _factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-Test-UserId",
            userId.ToString());

        var request =
            new GenerateTrainingPlanRequestDto
            {
                SportTypes = new List<string>
                {
                    SportType.Gym.ToString()
                },

                DurationMin = 10,

                Intensities = new List<string>
                {
                    intensity.ToString()
                },

                Difficulties = new List<string>
                {
                    difficulty.ToString()
                },

                FocusAreas = new List<string>
                {
                    "Fullbody"
                }
            };

        var response =
            await client.PostAsJsonAsync(
                "/api/Generation",
                request);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        using (
            var scope =
                _factory.Services.CreateScope())
        {
            var context =
                scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

            var user =
                await context.Users
                    .SingleAsync(
                        u => u.Id == userId);

            Assert.Equal(
                5,
                user.FreeGenerationsUsed);
        }
    }

    [Fact]
    public async Task PostGeneration_AsExpiredPremiumUserWithUsedQuota_ReturnsBadRequest()
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
                    Name = "Expired Premium User",
                    Email =
                        $"expired-premium-{Guid.NewGuid()}@test.hu",
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
                    Status =
                        SubscriptionStatus.Active,

                    CurrentPeriodStart =
                        DateTime.UtcNow.AddDays(-31),

                    CurrentPeriodEnd =
                        DateTime.UtcNow.AddDays(-1),

                    CancelAtPeriodEnd = false
                });

            await context.SaveChangesAsync();

            userId = user.Id;
        }

        var client =
            _factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-Test-UserId",
            userId.ToString());

        var request =
            new GenerateTrainingPlanRequestDto
            {
                SportTypes = new List<string>
                {
                    SportType.Gym.ToString()
                },

                DurationMin = 10,

                Intensities = new List<string>
                {
                    Enum.GetValues<IntensityLevel>()
                        .First()
                        .ToString()
                },

                Difficulties = new List<string>
                {
                    Enum.GetValues<DifficultyLevel>()
                        .First()
                        .ToString()
                },

                FocusAreas = new List<string>
                {
                    "Fullbody"
                }
            };

        var response =
            await client.PostAsJsonAsync(
                "/api/Generation",
                request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        using (
            var scope =
                _factory.Services.CreateScope())
        {
            var context =
                scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

            var user =
                await context.Users
                    .SingleAsync(
                        u => u.Id == userId);

            Assert.Equal(
                5,
                user.FreeGenerationsUsed);
        }
    }

    [Fact]
    public async Task PostGeneration_AsAdminWithUsedQuota_ReturnsOkAndDoesNotConsumeQuota()
    {
        int userId;

        var difficulty =
            Enum.GetValues<DifficultyLevel>().First();

        var intensity =
            Enum.GetValues<IntensityLevel>().First();

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
                    Name = "API Admin",
                    Email =
                        $"generation-admin-{Guid.NewGuid()}@test.hu",
                    PasswordHash = "test",
                    Role = "Admin",
                    FreeGenerationsUsed = 5
                };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var tag =
                new Tag
                {
                    Name = "Fullbody",
                    Type = TagType.Focus
                };

            context.Tags.Add(tag);
            await context.SaveChangesAsync();

            var exercise =
                new Exercise
                {
                    Title =
                        $"Admin Exercise {Guid.NewGuid()}",

                    Description = "Admin API test.",

                    SportType = SportType.Gym,
                    DurationMin = 10,
                    Difficulty = difficulty,
                    Intensity = intensity,
                    MinPlayers = 1,
                    MaxPlayers = 20,
                    Phase = ExercisePhase.Warmup
                };

            context.Exercises.Add(exercise);
            await context.SaveChangesAsync();

            context.ExerciseTags.Add(
                new ExerciseTag
                {
                    ExerciseId = exercise.Id,
                    TagId = tag.Id
                });

            await context.SaveChangesAsync();

            userId = user.Id;
        }

        var client =
            _factory.CreateClient();

        client.DefaultRequestHeaders.Add(
            "X-Test-UserId",
            userId.ToString());

        var request =
            new GenerateTrainingPlanRequestDto
            {
                SportTypes = new List<string>
                {
                    SportType.Gym.ToString()
                },

                DurationMin = 10,

                Intensities = new List<string>
                {
                    intensity.ToString()
                },

                Difficulties = new List<string>
                {
                    difficulty.ToString()
                },

                FocusAreas = new List<string>
                {
                    "Fullbody"
                }
            };

        var response =
            await client.PostAsJsonAsync(
                "/api/Generation",
                request);

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        using (
            var scope =
                _factory.Services.CreateScope())
        {
            var context =
                scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

            var user =
                await context.Users
                    .SingleAsync(
                        u => u.Id == userId);

            Assert.Equal(
                5,
                user.FreeGenerationsUsed);
        }
    }
}