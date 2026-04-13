using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolleyPlanner.API.Data;
using VolleyPlanner.API.DTOs.Exercise;
using VolleyPlanner.API.Enums;
using Microsoft.AspNetCore.Authorization;

namespace VolleyPlanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExercisesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ExercisesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<PagedExercisesResponseDto>> GetAll([FromQuery] ExerciseQueryDto query)
    {
        var exercisesQuery = _context.Exercises.AsQueryable();

        if (query.SportTypes != null && query.SportTypes.Any())
        {
            var parsedSportTypes = query.SportTypes
                .Select(value => Enum.TryParse<SportType>(value, true, out var parsed) ? parsed : (SportType?)null)
                .Where(value => value.HasValue)
                .Select(value => value!.Value)
                .ToList();

            if (parsedSportTypes.Any())
            {
                exercisesQuery = exercisesQuery.Where(e => parsedSportTypes.Contains(e.SportType));
            }
        }

        if (query.Difficulties != null && query.Difficulties.Any())
        {
            var parsedDifficulties = query.Difficulties
                .Select(value => Enum.TryParse<DifficultyLevel>(value, true, out var parsed) ? parsed : (DifficultyLevel?)null)
                .Where(value => value.HasValue)
                .Select(value => value!.Value)
                .ToList();

            if (parsedDifficulties.Any())
            {
                exercisesQuery = exercisesQuery.Where(e => parsedDifficulties.Contains(e.Difficulty));
            }
        }

        if (query.Intensities != null && query.Intensities.Any())
        {
            var parsedIntensities = query.Intensities
                .Select(value => Enum.TryParse<IntensityLevel>(value, true, out var parsed) ? parsed : (IntensityLevel?)null)
                .Where(value => value.HasValue)
                .Select(value => value!.Value)
                .ToList();

            if (parsedIntensities.Any())
            {
                exercisesQuery = exercisesQuery.Where(e => parsedIntensities.Contains(e.Intensity));
            }
        }

        if (query.Phases != null && query.Phases.Any())
        {
            var parsedPhases = query.Phases
                .Select(value => Enum.TryParse<ExercisePhase>(value, true, out var parsed) ? parsed : (ExercisePhase?)null)
                .Where(value => value.HasValue)
                .Select(value => value!.Value)
                .ToList();

            if (parsedPhases.Any())
            {
                exercisesQuery = exercisesQuery.Where(e => parsedPhases.Contains(e.Phase));
            }
        }

        if (query.MinPlayers.HasValue)
        {
            exercisesQuery = exercisesQuery.Where(e => e.MinPlayers >= query.MinPlayers.Value);
        }

        if (query.MaxPlayers.HasValue)
        {
            exercisesQuery = exercisesQuery.Where(e => e.MaxPlayers <= query.MaxPlayers.Value);
        }

        var totalCount = await exercisesQuery.CountAsync();

        var items = await exercisesQuery
            .OrderBy(e => e.Title)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
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
                Phase = e.Phase.ToString()
            })
            .ToListAsync();

        return Ok(new PagedExercisesResponseDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
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
            Tags = exercise.ExerciseTags.Select(t => t.Tag.Name).ToList(),
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

        var exercise = new VolleyPlanner.API.Entities.Exercise
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

        var exerciseTags = request.TagIds.Select(tagId => new VolleyPlanner.API.Entities.ExerciseTag
        {
            ExerciseId = exercise.Id,
            TagId = tagId
        }).ToList();

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
            Tags = createdExercise.ExerciseTags.Select(et => et.Tag.Name).ToList(),
            FocusTags = createdExercise.ExerciseTags
                .Where(et => et.Tag.Type == TagType.Focus)
                .Select(et => et.Tag.Name)
                .ToList()
        };

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}