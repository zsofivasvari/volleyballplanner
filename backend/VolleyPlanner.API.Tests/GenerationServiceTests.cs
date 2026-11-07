using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using VolleyPlanner.API.Data;
using VolleyPlanner.API.DTOs.Generation;
using VolleyPlanner.API.Entities;
using VolleyPlanner.API.Enums;
using VolleyPlanner.API.Services;
using System.Reflection;

namespace VolleyPlanner.API.Tests;

public class GenerationServiceTests
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

    [Fact]
    public async Task FreeUser_WithFiveUsedGenerations_CannotGenerate()
    {
        await using var connection =
            new SqliteConnection("DataSource=:memory:");

        await connection.OpenAsync();

        await using var context =
            CreateContext(connection);

        var user = new User
        {
            Name = "Teszt felhasználó",
            Email = "test@example.com",
            PasswordHash = "test",
            Role = "User",
            FreeGenerationsUsed = 5
        };

        context.Users.Add(user);

        await context.SaveChangesAsync();

        var service =
            new GenerationService(context);

        var request =
            new GenerateTrainingPlanRequestDto();

        var exception =
            await Assert.ThrowsAsync<Exception>(
                () =>
                    service.GenerateAsync(
                        user.Id,
                        request));

        Assert.Contains(
            "Felhasználtad mind az 5 ingyenes",
            exception.Message);
    }

    [Fact]
    public async Task PremiumUser_WithFiveUsedGenerations_IsNotBlockedByQuota()
    {
        await using var connection =
            new SqliteConnection("DataSource=:memory:");

        await connection.OpenAsync();

        await using var context =
            CreateContext(connection);

        var user = new User
        {
            Name = "Premium User",
            Email = "premium-generation@test.hu",
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
                    DateTime.UtcNow.AddDays(-1),
                CurrentPeriodEnd =
                    DateTime.UtcNow.AddDays(30)
            });

        await context.SaveChangesAsync();

        var service =
            new GenerationService(context);

        var request =
            new GenerateTrainingPlanRequestDto();

        var exception =
            await Assert.ThrowsAsync<Exception>(
                () =>
                    service.GenerateAsync(
                        user.Id,
                        request));

        Assert.DoesNotContain(
            "Felhasználtad mind az 5 ingyenes",
            exception.Message);

        Assert.Contains(
            "sportág",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Admin_WithFiveUsedGenerations_IsNotBlockedByQuota()
    {
        await using var connection =
            new SqliteConnection("DataSource=:memory:");

        await connection.OpenAsync();

        await using var context =
            CreateContext(connection);

        var user = new User
        {
            Name = "Admin User",
            Email = "admin-generation@test.hu",
            PasswordHash = "test",
            Role = "Admin",
            FreeGenerationsUsed = 5
        };

        context.Users.Add(user);

        await context.SaveChangesAsync();

        var service =
            new GenerationService(context);

        var request =
            new GenerateTrainingPlanRequestDto();

        var exception =
            await Assert.ThrowsAsync<Exception>(
                () =>
                    service.GenerateAsync(
                        user.Id,
                        request));

        Assert.DoesNotContain(
            "Felhasználtad mind az 5 ingyenes",
            exception.Message);

        Assert.Contains(
            "sportág",
            exception.Message,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SuccessfulFreeGeneration_IncreasesFreeGenerationCount()
    {
        await using var connection =
            new SqliteConnection("DataSource=:memory:");

        await connection.OpenAsync();

        await using var context =
            CreateContext(connection);

        var user = new User
        {
            Name = "Free Generator",
            Email = "free-generator@test.hu",
            PasswordHash = "test",
            Role = "User",
            FreeGenerationsUsed = 0
        };

        context.Users.Add(user);

        await context.SaveChangesAsync();

        var service =
            new GenerationService(context);

        await InvokeCompleteGenerationAsync(
            service,
            user,
            isAdmin: false,
            isPremium: false);

        Assert.Equal(
            1,
            user.FreeGenerationsUsed);
    }

    [Fact]
    public async Task SuccessfulPremiumGeneration_DoesNotConsumeFreeQuota()
    {
        await using var connection =
            new SqliteConnection("DataSource=:memory:");

        await connection.OpenAsync();

        await using var context =
            CreateContext(connection);

        var user = new User
        {
            Name = "Premium Generator",
            Email = "premium-generator@test.hu",
            PasswordHash = "test",
            Role = "User",
            FreeGenerationsUsed = 5
        };

        context.Users.Add(user);

        await context.SaveChangesAsync();

        var service =
            new GenerationService(context);

        await InvokeCompleteGenerationAsync(
            service,
            user,
            isAdmin: false,
            isPremium: true);

        Assert.Equal(
            5,
            user.FreeGenerationsUsed);
    }

    [Fact]
    public async Task SuccessfulAdminGeneration_DoesNotConsumeFreeQuota()
    {
        await using var connection =
            new SqliteConnection("DataSource=:memory:");

        await connection.OpenAsync();

        await using var context =
            CreateContext(connection);

        var user = new User
        {
            Name = "Admin Generator",
            Email = "admin-generator@test.hu",
            PasswordHash = "test",
            Role = "Admin",
            FreeGenerationsUsed = 5
        };

        context.Users.Add(user);

        await context.SaveChangesAsync();

        var service =
            new GenerationService(context);

        await InvokeCompleteGenerationAsync(
            service,
            user,
            isAdmin: true,
            isPremium: false);

        Assert.Equal(
            5,
            user.FreeGenerationsUsed);
    }

    private static async Task InvokeCompleteGenerationAsync(
        GenerationService service,
        User user,
        bool isAdmin,
        bool isPremium)
    {
        var method =
            typeof(GenerationService).GetMethod(
                "CompleteGenerationAsync",
                BindingFlags.Instance |
                BindingFlags.NonPublic);

        Assert.NotNull(method);

        var result =
            new GeneratedTrainingPlanResponseDto
            {
                SportType = "BeachVolleyball",
                TargetDuration = 60,
                Difficulty = "Beginner",
                Intensity = "Low",
                FocusAreas = new List<string>(),
                Items = new List<GeneratedTrainingPlanItemDto>()
            };

        var task =
            method!.Invoke(
                service,
                new object[]
                {
                    user,
                    isAdmin,
                    isPremium,
                    result
                }) as Task<GeneratedTrainingPlanResponseDto>;

        Assert.NotNull(task);

        await task!;
    }

    [Fact]
    public async Task FreeUser_SuccessfulRealGeneration_IncreasesQuota()
    {
        await using var connection =
            new SqliteConnection("DataSource=:memory:");

        await connection.OpenAsync();

        await using var context =
            CreateContext(connection);

        // 1. Free felhasználó
        var user = new User
        {
            Name = "Real Generation User",
            Email = "real-generation@test.hu",
            PasswordHash = "test",
            Role = "User",
            FreeGenerationsUsed = 0
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        // 2. Egy fókusz tag
        var focusTag = new Tag
        {
            Name = "Fullbody",
            Type = TagType.Focus
        };

        context.Tags.Add(focusTag);
        await context.SaveChangesAsync();

        // 3. Egy 10 perces Gym bemelegítés
        var difficulty =
            Enum.GetValues<DifficultyLevel>().First();

        var intensity =
            Enum.GetValues<IntensityLevel>().First();

        var exercise = new Exercise
        {
            Title = "Teszt bemelegítés",
            Description = "Automatizált teszthez.",
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

        // 4. A gyakorlatot összekötjük a Fullbody fókusz taggel
        context.ExerciseTags.Add(
            new ExerciseTag
            {
                ExerciseId = exercise.Id,
                TagId = focusTag.Id
            });

        await context.SaveChangesAsync();

        // 5. Valódi generálási kérés
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

        var service =
            new GenerationService(context);

        // 6. Tényleges generálás
        var result =
            await service.GenerateAsync(
                user.Id,
                request);

        // 7. Ellenőrzések
        Assert.NotNull(result);

        Assert.Equal(
            10,
            result.TargetDuration);

        Assert.NotEmpty(
            result.Items);

        Assert.Equal(
            10,
            result.Items.Sum(
                item => item.PlannedDuration));

        Assert.Equal(
            1,
            user.FreeGenerationsUsed);
    }

    [Fact]
    public async Task PremiumUser_SuccessfulRealGeneration_DoesNotConsumeQuota()
    {
        await using var connection =
            new SqliteConnection("DataSource=:memory:");

        await connection.OpenAsync();

        await using var context =
            CreateContext(connection);

        var user = new User
        {
            Name = "Premium Real Generation User",
            Email = "premium-real@test.hu",
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
                    DateTime.UtcNow.AddDays(-1),
                CurrentPeriodEnd =
                    DateTime.UtcNow.AddDays(30)
            });

        await context.SaveChangesAsync();

        var focusTag = new Tag
        {
            Name = "Fullbody",
            Type = TagType.Focus
        };

        context.Tags.Add(focusTag);
        await context.SaveChangesAsync();

        var difficulty =
            Enum.GetValues<DifficultyLevel>().First();

        var intensity =
            Enum.GetValues<IntensityLevel>().First();

        var exercise = new Exercise
        {
            Title = "Premium teszt bemelegítés",
            Description = "Automatizált Premium teszt.",
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

        var service =
            new GenerationService(context);

        var result =
            await service.GenerateAsync(
                user.Id,
                request);

        Assert.NotNull(result);

        Assert.Equal(
            10,
            result.TargetDuration);

        Assert.NotEmpty(
            result.Items);

        Assert.Equal(
            10,
            result.Items.Sum(
                item => item.PlannedDuration));

        Assert.Equal(
            5,
            user.FreeGenerationsUsed);
    }

    [Fact]
    public async Task FailedGeneration_DoesNotConsumeFreeQuota()
    {
        await using var connection =
            new SqliteConnection("DataSource=:memory:");

        await connection.OpenAsync();

        await using var context =
            CreateContext(connection);

        var user = new User
        {
            Name = "Failed Generation User",
            Email = "failed-generation@test.hu",
            PasswordHash = "test",
            Role = "User",
            FreeGenerationsUsed = 2
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        var difficulty =
            Enum.GetValues<DifficultyLevel>().First();

        var intensity =
            Enum.GetValues<IntensityLevel>().First();

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

        var service =
            new GenerationService(context);

        var exception =
            await Assert.ThrowsAsync<Exception>(
                () =>
                    service.GenerateAsync(
                        user.Id,
                        request));

        Assert.Contains(
            "Nem található megfelelő gyakorlat",
            exception.Message);

        Assert.Equal(
            2,
            user.FreeGenerationsUsed);
    }
}