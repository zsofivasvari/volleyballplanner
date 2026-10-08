using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VolleyPlanner.API.DTOs.Tournament;
using VolleyPlanner.API.Interfaces;

namespace VolleyPlanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TournamentsController : ControllerBase
{
    private readonly ITournamentService _tournamentService;

    public TournamentsController(
        ITournamentService tournamentService)
    {
        _tournamentService = tournamentService;
    }

    [HttpPost]
    public async Task<ActionResult<TournamentDetailsDto>>
        Create(
            CreateTournamentRequestDto request)
    {
        try
        {
            var userId =
                GetCurrentUserId();

            var result =
                await _tournamentService.CreateAsync(
                    userId,
                    request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Id },
                result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new
            {
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<
        ActionResult<List<TournamentListItemDto>>>
        GetAll()
    {
        var result =
            await _tournamentService
                .GetAllAsync();

        return Ok(result);
    }

    [HttpGet("mine")]
    public async Task<
        ActionResult<List<TournamentListItemDto>>>
        GetMine()
    {
        try
        {
            var userId =
                GetCurrentUserId();

            var result =
                await _tournamentService
                    .GetMineAsync(userId);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new
            {
                message = ex.Message
            });
        }
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TournamentDetailsDto>>
        GetById(int id)
    {
        var tournament =
            await _tournamentService
                .GetByIdAsync(id);

        if (tournament == null)
        {
            return NotFound(new
            {
                message =
                    "A verseny nem található."
            });
        }

        return Ok(tournament);
    }

    private int GetCurrentUserId()
    {
        var userIdClaim =
            User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) ||
            !int.TryParse(
                userIdClaim,
                out var userId))
        {
            throw new UnauthorizedAccessException(
                "Érvénytelen vagy hiányzó felhasználói azonosító.");
        }

        return userId;
    }

    [AllowAnonymous]
    [HttpGet("{id:int}/entries")]
    public async Task<
        ActionResult<List<TournamentEntryListItemDto>>>
        GetEntries(int id)
    {
        try
        {
            var result =
                await _tournamentService
                    .GetEntriesAsync(id);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPost("{id:int}/draw")]
    public async Task<ActionResult<TournamentDrawResponseDto>>
        GenerateDraw(int id)
    {
        try
        {
            var userId =
                GetCurrentUserId();

            var result =
                await _tournamentService
                    .GeneratePoolDrawAsync(
                        id,
                        userId);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new
            {
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPost("{tournamentId:int}/matches/{matchId:int}/result")]
    public async Task<
        ActionResult<TournamentMatchResultResponseDto>>
        SubmitMatchResult(
            int tournamentId,
            int matchId,
            SubmitTournamentMatchResultRequestDto request)
    {
        try
        {
            var userId =
                GetCurrentUserId();

            var result =
                await _tournamentService
                    .SubmitMatchResultAsync(
                        tournamentId,
                        matchId,
                        userId,
                        request);

            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new
            {
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [AllowAnonymous]
    [HttpGet("{id:int}/matches")]
    public async Task<
        ActionResult<List<TournamentMatchDto>>>
        GetMatches(int id)
    {
        try
        {
            var result =
                await _tournamentService
                    .GetMatchesAsync(id);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [AllowAnonymous]
    [HttpGet("{id:int}/bracket")]
    public async Task<
        ActionResult<TournamentBracketDto>>
        GetBracket(int id)
    {
        try
        {
            var result =
                await _tournamentService
                    .GetBracketAsync(id);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [AllowAnonymous]
    [HttpGet("{id:int}/final-result")]
    public async Task<ActionResult<TournamentFinalResultDto>>
        GetFinalResult(int id)
    {
        try
        {
            var result =
                await _tournamentService
                    .GetFinalResultAsync(id);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }
}