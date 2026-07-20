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

    private static readonly HashSet<string> BeachVolleyballFocuses =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Nyitás",
            "Nyitásfogadás",
            "Feladás",
            "Támadás",
            "Blokk/Védekezés",
            "Állóképesség"
        };

    private static readonly HashSet<string> GymFocuses =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Alsótest",
            "Törzs",
            "Felsőtest",
            "Fullbody",
            "Állóképesség"
        };

    public GenerationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<GeneratedTrainingPlanResponseDto> GenerateAsync(
        GenerateTrainingPlanRequestDto request)
    {
        if (request.SportTypes == null || request.SportTypes.Count != 1)
        {
            throw new Exception("Egyszerre pontosan egy sportág kiválasztása kötelező.");
        }

        if (!Enum.TryParse<SportType>(
                request.SportTypes[0],
                true,
                out var selectedSportType))
        {
            throw new Exception("Nincs érvényes sportág kiválasztva.");
        }

        var isGym = selectedSportType == SportType.Gym;

        if (request.Difficulties == null || !request.Difficulties.Any())
        {
            throw new Exception("Legalább egy nehézség megadása kötelező.");
        }

        if (request.Intensities == null || !request.Intensities.Any())
        {
            throw new Exception("Legalább egy intenzitás megadása kötelező.");
        }

        if (request.FocusAreas == null || !request.FocusAreas.Any())
        {
            throw new Exception("Legalább egy fókuszterület megadása kötelező.");
        }

        if (request.DurationMin <= 0)
        {
            throw new Exception("Az edzés időtartamának pozitív értéknek kell lennie.");
        }

        if (!isGym && (!request.PlayerCount.HasValue || request.PlayerCount.Value <= 0))
        {
            throw new Exception("Strandröplabda tervnél a játékosok számának pozitív értéknek kell lennie.");
        }

        var normalizedFocusAreas = request.FocusAreas
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (!normalizedFocusAreas.Any())
        {
            throw new Exception("Legalább egy érvényes fókuszterület megadása kötelező.");
        }

        var allowedFocuses = selectedSportType switch
        {
            SportType.BeachVolleyball => BeachVolleyballFocuses,
            SportType.Gym => GymFocuses,
            _ => new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        };

        if (normalizedFocusAreas.Any(focus => !allowedFocuses.Contains(focus)))
        {
            throw new Exception("A kiválasztott sportághoz érvénytelen fókuszterület tartozik.");
        }

        var parsedDifficulties = request.Difficulties
            .Select(value =>
                Enum.TryParse<DifficultyLevel>(value, true, out var parsed)
                    ? parsed
                    : (DifficultyLevel?)null)
            .Where(value => value.HasValue)
            .Select(value => value!.Value)
            .ToList();

        var parsedIntensities = request.Intensities
            .Select(value =>
                Enum.TryParse<IntensityLevel>(value, true, out var parsed)
                    ? parsed
                    : (IntensityLevel?)null)
            .Where(value => value.HasValue)
            .Select(value => value!.Value)
            .ToList();

        if (!parsedDifficulties.Any())
        {
            throw new Exception("Nincs érvényes nehézség kiválasztva.");
        }

        if (!parsedIntensities.Any())
        {
            throw new Exception("Nincs érvényes intenzitás kiválasztva.");
        }

        var eligibleExercisesQuery = _context.Exercises
            .Include(e => e.ExerciseTags)
                .ThenInclude(et => et.Tag)
            .Where(e =>
                e.SportType == selectedSportType &&
                parsedDifficulties.Contains(e.Difficulty) &&
                parsedIntensities.Contains(e.Intensity));

        if (!isGym)
        {
            var playerCount = request.PlayerCount!.Value;

            eligibleExercisesQuery = eligibleExercisesQuery.Where(e =>
                e.MinPlayers <= playerCount &&
                e.MaxPlayers >= playerCount);
        }

        var allEligibleExercises = await eligibleExercisesQuery
            .OrderBy(e => e.DurationMin)
            .ToListAsync();

        if (!allEligibleExercises.Any())
        {
            throw new Exception("Nem található megfelelő gyakorlat a megadott alapfeltételekhez.");
        }

        var focusAreaSet = new HashSet<string>(
            normalizedFocusAreas,
            StringComparer.OrdinalIgnoreCase
        );

        bool MatchesSelectedFocus(Exercise exercise)
        {
            return exercise.ExerciseTags.Any(et =>
                et.Tag.Type == TagType.Focus &&
                focusAreaSet.Contains(et.Tag.Name));
        }

        var focusExercises = allEligibleExercises
            .Where(MatchesSelectedFocus)
            .ToList();

        if (!focusExercises.Any())
        {
            throw new Exception(
                "Nem található a kiválasztott fókuszterületekhez tartozó megfelelő gyakorlat.");
        }

        var selectedItems =
            new List<(Exercise exercise, string sectionName, int plannedDuration, bool matchesFocus)>();

        var usedExerciseIds = new HashSet<int>();

        int remainingTotalDuration = request.DurationMin;

        int warmupTarget = Math.Max(10, request.DurationMin / 5);
        warmupTarget = Math.Min(warmupTarget, request.DurationMin);

        var warmupPool = isGym
            ? allEligibleExercises
                .Where(e => e.Phase == ExercisePhase.Warmup)
                .OrderByDescending(MatchesSelectedFocus)
                .ThenBy(e => e.DurationMin)
                .ToList()
            : focusExercises
                .Where(e => e.Phase == ExercisePhase.Warmup)
                .OrderBy(e => e.DurationMin)
                .ToList();

        if (!warmupPool.Any())
        {
            throw new Exception("Nincs megfelelő bemelegítő gyakorlat a megadott feltételekhez.");
        }

        AddExercisesUntilTarget(
            warmupPool,
            "Warmup",
            warmupTarget,
            ref remainingTotalDuration,
            selectedItems,
            usedExerciseIds,
            MatchesSelectedFocus
        );

        if (remainingTotalDuration <= 0)
        {
            return BuildResponse(
                selectedSportType,
                request,
                normalizedFocusAreas,
                selectedItems
            );
        }

        var focusedMainPool = focusExercises
            .Where(e => e.Phase == ExercisePhase.Main)
            .OrderBy(e => e.DurationMin)
            .ToList();

        if (!focusedMainPool.Any())
        {
            throw new Exception("Nincs megfelelő főrészes fókuszgyakorlat.");
        }

        if (isGym)
        {
            int minimumFocusDuration = (int)Math.Ceiling(request.DurationMin * 0.70);

            int currentFocusDuration = selectedItems
                .Where(item => item.matchesFocus)
                .Sum(item => item.plannedDuration);

            int remainingRequiredFocusDuration =
                Math.Max(0, minimumFocusDuration - currentFocusDuration);

            if (remainingRequiredFocusDuration > 0)
            {
                AddExercisesUntilTarget(
                    focusedMainPool,
                    "Main",
                    remainingRequiredFocusDuration,
                    ref remainingTotalDuration,
                    selectedItems,
                    usedExerciseIds,
                    MatchesSelectedFocus
                );
            }

            currentFocusDuration = selectedItems
                .Where(item => item.matchesFocus)
                .Sum(item => item.plannedDuration);

            if (currentFocusDuration < minimumFocusDuration)
            {
                throw new Exception(
                    "Nincs elegendő fókuszterülethez kapcsolódó Gym gyakorlat a 70%-os fókuszarány teljesítéséhez.");
            }

            var supportMainPool = allEligibleExercises
                .Where(e =>
                    e.Phase == ExercisePhase.Main &&
                    !MatchesSelectedFocus(e))
                .OrderBy(e => e.DurationMin)
                .ToList();

            if (remainingTotalDuration > 0)
            {
                AddExercisesUntilTarget(
                    supportMainPool,
                    "Main",
                    remainingTotalDuration,
                    ref remainingTotalDuration,
                    selectedItems,
                    usedExerciseIds,
                    MatchesSelectedFocus
                );
            }

            if (remainingTotalDuration > 0)
            {
                AddExercisesUntilTarget(
                    focusedMainPool,
                    "Main",
                    remainingTotalDuration,
                    ref remainingTotalDuration,
                    selectedItems,
                    usedExerciseIds,
                    MatchesSelectedFocus
                );
            }
        }
        else
        {
            AddExercisesUntilTarget(
                focusedMainPool,
                "Main",
                remainingTotalDuration,
                ref remainingTotalDuration,
                selectedItems,
                usedExerciseIds,
                MatchesSelectedFocus
            );
        }

        if (remainingTotalDuration > 0)
        {
            throw new Exception(
                "Nincs elegendő megfelelő gyakorlat a kért edzésidő teljes kitöltéséhez.");
        }

        return BuildResponse(
            selectedSportType,
            request,
            normalizedFocusAreas,
            selectedItems
        );
    }

    private static void AddExercisesUntilTarget(
        IEnumerable<Exercise> exercisePool,
        string sectionName,
        int targetMinutes,
        ref int remainingTotalDuration,
        List<(Exercise exercise, string sectionName, int plannedDuration, bool matchesFocus)> selectedItems,
        HashSet<int> usedExerciseIds,
        Func<Exercise, bool> matchesFocus)
    {
        int addedMinutes = 0;

        foreach (var exercise in exercisePool)
        {
            if (addedMinutes >= targetMinutes || remainingTotalDuration <= 0)
            {
                break;
            }

            if (!usedExerciseIds.Add(exercise.Id))
            {
                continue;
            }

            int remainingTarget = targetMinutes - addedMinutes;

            int plannedDuration = Math.Min(
                exercise.DurationMin,
                Math.Min(remainingTarget, remainingTotalDuration)
            );

            if (plannedDuration <= 0)
            {
                continue;
            }

            selectedItems.Add((
                exercise,
                sectionName,
                plannedDuration,
                matchesFocus(exercise)
            ));

            addedMinutes += plannedDuration;
            remainingTotalDuration -= plannedDuration;
        }
    }

    private static GeneratedTrainingPlanResponseDto BuildResponse(
        SportType selectedSportType,
        GenerateTrainingPlanRequestDto request,
        List<string> normalizedFocusAreas,
        List<(Exercise exercise, string sectionName, int plannedDuration, bool matchesFocus)> selectedItems)
    {
        return new GeneratedTrainingPlanResponseDto
        {
            SportType = selectedSportType.ToString(),
            TargetDuration = request.DurationMin,
            Difficulty = string.Join(", ", request.Difficulties),
            Intensity = string.Join(", ", request.Intensities),
            FocusAreas = normalizedFocusAreas,
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