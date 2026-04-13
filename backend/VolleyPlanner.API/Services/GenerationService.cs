using Microsoft.EntityFrameworkCore;
using VolleyPlanner.API.Data;
using VolleyPlanner.API.DTOs.Generation;
using VolleyPlanner.API.Entities;
using VolleyPlanner.API.Enums;
using VolleyPlanner.API.Interfaces;

namespace VolleyPlanner.API.Services;

public class GenerationService : IGenerationService
{
    private readonly AppDbContext _context;

    private static readonly HashSet<string> AllowedFocuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Nyitás",
        "Nyitásfogadás",
        "Feladás",
        "Támadás",
        "Blokk/Védekezés"
    };

    public GenerationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<GeneratedTrainingPlanResponseDto> GenerateAsync(GenerateTrainingPlanRequestDto request)
    {
        if (request.SportTypes == null || !request.SportTypes.Any())
        {
            throw new Exception("Legalább egy sportág megadása kötelező.");
        }

        if (request.Difficulties == null || !request.Difficulties.Any())
        {
            throw new Exception("Legalább egy nehézség megadása kötelező.");
        }

        if (request.Intensities == null || !request.Intensities.Any())
        {
            throw new Exception("Legalább egy intenzitás megadása kötelező.");
        }

        if (string.IsNullOrWhiteSpace(request.PrimaryFocus))
        {
            throw new Exception("A fő fókusz megadása kötelező.");
        }

        if (!AllowedFocuses.Contains(request.PrimaryFocus.Trim()))
        {
            throw new Exception("Érvénytelen fókuszterület.");
        }

        var parsedSportTypes = request.SportTypes
            .Select(value => Enum.TryParse<SportType>(value, true, out var parsed) ? parsed : (SportType?)null)
            .Where(value => value.HasValue)
            .Select(value => value!.Value)
            .ToList();

        var parsedDifficulties = request.Difficulties
            .Select(value => Enum.TryParse<DifficultyLevel>(value, true, out var parsed) ? parsed : (DifficultyLevel?)null)
            .Where(value => value.HasValue)
            .Select(value => value!.Value)
            .ToList();

        var parsedIntensities = request.Intensities
            .Select(value => Enum.TryParse<IntensityLevel>(value, true, out var parsed) ? parsed : (IntensityLevel?)null)
            .Where(value => value.HasValue)
            .Select(value => value!.Value)
            .ToList();

        if (!parsedSportTypes.Any())
        {
            throw new Exception("Nincs érvényes sportág kiválasztva.");
        }

        if (!parsedDifficulties.Any())
        {
            throw new Exception("Nincs érvényes nehézség kiválasztva.");
        }

        if (!parsedIntensities.Any())
        {
            throw new Exception("Nincs érvényes intenzitás kiválasztva.");
        }

        var normalizedFocus = request.PrimaryFocus.Trim();

        var allExercises = await _context.Exercises
            .Include(e => e.ExerciseTags)
                .ThenInclude(et => et.Tag)
            .Where(e =>
                parsedSportTypes.Contains(e.SportType) &&
                parsedDifficulties.Contains(e.Difficulty) &&
                parsedIntensities.Contains(e.Intensity) &&
                e.MinPlayers <= request.PlayerCount &&
                e.MaxPlayers >= request.PlayerCount &&
                e.ExerciseTags.Any(et =>
                    et.Tag.Type == TagType.Focus &&
                    et.Tag.Name == normalizedFocus))
            .OrderBy(e => e.DurationMin)
            .ToListAsync();

        if (!allExercises.Any())
        {
            throw new Exception("Nem található megfelelő gyakorlat a megadott feltételekhez és fókuszterülethez.");
        }

        var warmups = allExercises
            .Where(e => e.Phase == ExercisePhase.Warmup)
            .ToList();

        var mains = allExercises
            .Where(e => e.Phase == ExercisePhase.Main)
            .ToList();

        if (!warmups.Any())
        {
            throw new Exception("Nincs megfelelő bemelegítő gyakorlat a megadott feltételekhez és fókuszterülethez.");
        }

        if (!mains.Any())
        {
            throw new Exception("Nincs megfelelő főrészes gyakorlat a megadott feltételekhez és fókuszterülethez.");
        }

        var selectedItems = new List<(Exercise exercise, string sectionName, int plannedDuration)>();

        int warmupTarget = Math.Max(10, request.DurationMin / 5);
        int mainTarget = request.DurationMin - warmupTarget;

        var selectedWarmup = warmups.First();
        selectedItems.Add((
            selectedWarmup,
            "Warmup",
            Math.Min(selectedWarmup.DurationMin, warmupTarget)
        ));

        int remainingMain = mainTarget;

        foreach (var exercise in mains)
        {
            if (remainingMain <= 0)
            {
                break;
            }

            int duration = Math.Min(exercise.DurationMin, remainingMain);

            selectedItems.Add((
                exercise,
                "Main",
                duration
            ));

            remainingMain -= duration;
        }

        if (!selectedItems.Any())
        {
            throw new Exception("Nem sikerült edzéstervet összeállítani.");
        }

        return new GeneratedTrainingPlanResponseDto
        {
            SportType = string.Join(", ", request.SportTypes),
            TargetDuration = request.DurationMin,
            Difficulty = string.Join(", ", request.Difficulties),
            Intensity = string.Join(", ", request.Intensities),
            PrimaryFocus = normalizedFocus,
            Items = selectedItems.Select(x => new GeneratedTrainingPlanItemDto
            {
                ExerciseId = x.exercise.Id,
                ExerciseTitle = x.exercise.Title,
                SectionName = x.sectionName,
                PlannedDuration = x.plannedDuration
            }).ToList()
        };
    }
}