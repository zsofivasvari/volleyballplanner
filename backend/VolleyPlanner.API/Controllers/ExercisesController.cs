using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolleyPlanner.API.Data;
using VolleyPlanner.API.DTOs.Exercise;
using VolleyPlanner.API.Entities;
using VolleyPlanner.API.Enums;

namespace VolleyPlanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExercisesController : ControllerBase
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

    public ExercisesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<PagedExercisesResponseDto>> GetAll([FromQuery] ExerciseQueryDto query)
    {
        var exercisesQuery = _context.Exercises
            .Include(e => e.ExerciseTags)
            .ThenInclude(et => et.Tag)
            .AsQueryable();

        if (query.SportTypes != null && query.SportTypes.Any())
        {
            var parsedSportTypes = query.SportTypes
                .Select(value => Enum.TryParse<SportType>(value, true, out var parsed)
                    ? parsed
                    : (SportType?)null)
                .Where(value => value.HasValue)
                .Select(value => value!.Value)
                .ToList();

            if (parsedSportTypes.Any())
            {
                exercisesQuery = exercisesQuery
                    .Where(e => parsedSportTypes.Contains(e.SportType));
            }
        }

        if (query.Difficulties != null && query.Difficulties.Any())
        {
            var parsedDifficulties = query.Difficulties
                .Select(value => Enum.TryParse<DifficultyLevel>(value, true, out var parsed)
                    ? parsed
                    : (DifficultyLevel?)null)
                .Where(value => value.HasValue)
                .Select(value => value!.Value)
                .ToList();

            if (parsedDifficulties.Any())
            {
                exercisesQuery = exercisesQuery
                    .Where(e => parsedDifficulties.Contains(e.Difficulty));
            }
        }

        if (query.Intensities != null && query.Intensities.Any())
        {
            var parsedIntensities = query.Intensities
                .Select(value => Enum.TryParse<IntensityLevel>(value, true, out var parsed)
                    ? parsed
                    : (IntensityLevel?)null)
                .Where(value => value.HasValue)
                .Select(value => value!.Value)
                .ToList();

            if (parsedIntensities.Any())
            {
                exercisesQuery = exercisesQuery
                    .Where(e => parsedIntensities.Contains(e.Intensity));
            }
        }

        if (query.Phases != null && query.Phases.Any())
        {
            var parsedPhases = query.Phases
                .Select(value => Enum.TryParse<ExercisePhase>(value, true, out var parsed)
                    ? parsed
                    : (ExercisePhase?)null)
                .Where(value => value.HasValue)
                .Select(value => value!.Value)
                .ToList();

            if (parsedPhases.Any())
            {
                exercisesQuery = exercisesQuery
                    .Where(e => parsedPhases.Contains(e.Phase));
            }
        }

        if (query.FocusTags != null && query.FocusTags.Any())
        {
            var normalizedFocusTags = query.FocusTags
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (normalizedFocusTags.Any())
            {
                exercisesQuery = exercisesQuery.Where(e =>
                    e.ExerciseTags.Any(et =>
                        et.Tag.Type == TagType.Focus &&
                        normalizedFocusTags.Contains(et.Tag.Name)));
            }
        }

        if (query.MinPlayers.HasValue)
        {
            exercisesQuery = exercisesQuery
                .Where(e => e.MinPlayers <= query.MinPlayers.Value);
        }

        if (query.MaxPlayers.HasValue)
        {
            exercisesQuery = exercisesQuery
                .Where(e => e.MaxPlayers >= query.MaxPlayers.Value);
        }

        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 8 : query.PageSize;

        var totalCount = await exercisesQuery.CountAsync();

        var items = await exercisesQuery
            .OrderBy(e => e.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new ExerciseListItemDto
            {
                Id = e.Id,
                Title = e.Title,
                SportType = e.SportType.ToString(),
                DurationMin = e.DurationMin,
                Difficulty = e.Difficulty.ToString(),
                Intensity = e.Intensity.ToString(),
                MinPlayers = e.MinPlayers,
                MaxPlayers = e.MaxPlayers,
                Phase = e.Phase.ToString(),
                FocusTags = e.ExerciseTags
                    .Where(et => et.Tag.Type == TagType.Focus)
                    .Select(et => et.Tag.Name)
                    .ToList()
            })
            .ToListAsync();

        return Ok(new PagedExercisesResponseDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ExerciseDetailsDto>> GetById(int id)
    {
        var exercise = await _context.Exercises
            .Include(e => e.ExerciseTags)
            .ThenInclude(et => et.Tag)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (exercise == null)
        {
            return NotFound();
        }

        return Ok(new ExerciseDetailsDto
        {
            Id = exercise.Id,
            Title = exercise.Title,
            Description = exercise.Description,
            SportType = exercise.SportType.ToString(),
            DurationMin = exercise.DurationMin,
            Difficulty = exercise.Difficulty.ToString(),
            Intensity = exercise.Intensity.ToString(),
            MinPlayers = exercise.MinPlayers,
            MaxPlayers = exercise.MaxPlayers,
            Phase = exercise.Phase.ToString(),
            Tags = exercise.ExerciseTags
                .Select(t => t.Tag.Name)
                .ToList(),
            FocusTags = exercise.ExerciseTags
                .Where(et => et.Tag.Type == TagType.Focus)
                .Select(et => et.Tag.Name)
                .ToList()
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ExerciseDetailsDto>> Create(CreateExerciseRequestDto request)
    {
        if (!Enum.TryParse<SportType>(request.SportType, true, out var sportType))
        {
            return BadRequest(new { message = "Érvénytelen SportType." });
        }

        if (!Enum.TryParse<DifficultyLevel>(request.Difficulty, true, out var difficulty))
        {
            return BadRequest(new { message = "Érvénytelen Difficulty." });
        }

        if (!Enum.TryParse<IntensityLevel>(request.Intensity, true, out var intensity))
        {
            return BadRequest(new { message = "Érvénytelen Intensity." });
        }

        if (!Enum.TryParse<ExercisePhase>(request.Phase, true, out var phase))
        {
            return BadRequest(new { message = "Érvénytelen Phase." });
        }

        if (request.MinPlayers > request.MaxPlayers)
        {
            return BadRequest(new { message = "A MinPlayers nem lehet nagyobb, mint a MaxPlayers." });
        }

        var existingTags = await _context.Tags
            .Where(t => request.TagIds.Contains(t.Id))
            .ToListAsync();

        if (existingTags.Count != request.TagIds.Count)
        {
            return BadRequest(new { message = "Az egyik vagy több megadott TagId nem létezik." });
        }

        var selectedFocusTags = existingTags
            .Where(t => t.Type == TagType.Focus)
            .ToList();

        if (!selectedFocusTags.Any())
        {
            return BadRequest(new { message = "Legalább egy fókuszterület kiválasztása kötelező." });
        }

        var allowedFocuses = sportType switch
        {
            SportType.BeachVolleyball => BeachVolleyballFocuses,
            SportType.Gym => GymFocuses,
            _ => new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        };

        var invalidFocusTags = selectedFocusTags
            .Where(tag => !allowedFocuses.Contains(tag.Name))
            .Select(tag => tag.Name)
            .ToList();

        if (invalidFocusTags.Any())
        {
            return BadRequest(new
            {
                message = $"A kiválasztott sportághoz érvénytelen fókuszterület tartozik: {string.Join(", ", invalidFocusTags)}."
            });
        }

        var exercise = new Exercise
        {
            Title = request.Title,
            Description = request.Description,
            SportType = sportType,
            DurationMin = request.DurationMin,
            Difficulty = difficulty,
            Intensity = intensity,
            MinPlayers = request.MinPlayers,
            MaxPlayers = request.MaxPlayers,
            Phase = phase,
            CreatedAt = DateTime.UtcNow
        };

        _context.Exercises.Add(exercise);
        await _context.SaveChangesAsync();

        var exerciseTags = request.TagIds
            .Select(tagId => new ExerciseTag
            {
                ExerciseId = exercise.Id,
                TagId = tagId
            })
            .ToList();

        _context.ExerciseTags.AddRange(exerciseTags);
        await _context.SaveChangesAsync();

        var createdExercise = await _context.Exercises
            .Include(e => e.ExerciseTags)
            .ThenInclude(et => et.Tag)
            .FirstAsync(e => e.Id == exercise.Id);

        var result = new ExerciseDetailsDto
        {
            Id = createdExercise.Id,
            Title = createdExercise.Title,
            Description = createdExercise.Description,
            SportType = createdExercise.SportType.ToString(),
            DurationMin = createdExercise.DurationMin,
            Difficulty = createdExercise.Difficulty.ToString(),
            Intensity = createdExercise.Intensity.ToString(),
            MinPlayers = createdExercise.MinPlayers,
            MaxPlayers = createdExercise.MaxPlayers,
            Phase = createdExercise.Phase.ToString(),
            Tags = createdExercise.ExerciseTags
                .Select(et => et.Tag.Name)
                .ToList(),
            FocusTags = createdExercise.ExerciseTags
                .Where(et => et.Tag.Type == TagType.Focus)
                .Select(et => et.Tag.Name)
                .ToList()
        };

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}