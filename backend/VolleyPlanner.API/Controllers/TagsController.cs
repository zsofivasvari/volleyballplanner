using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolleyPlanner.API.Data;
using VolleyPlanner.API.DTOs.Exercise;
using VolleyPlanner.API.Enums;

namespace VolleyPlanner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly AppDbContext _context;

    public TagsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TagDto>>> GetAll([FromQuery] string? type = null)
    {
        var query = _context.Tags.AsQueryable();

        if (!string.IsNullOrWhiteSpace(type))
        {
            if (!Enum.TryParse<TagType>(type, true, out var parsedType))
            {
                return BadRequest(new { message = "Érvénytelen tag típus." });
            }

            query = query.Where(t => t.Type == parsedType);
        }

        var tags = await query
            .OrderBy(t => t.Name)
            .Select(t => new TagDto
            {
                Id = t.Id,
                Name = t.Name,
                Type = t.Type.ToString()
            })
            .ToListAsync();

        return Ok(tags);
    }
}