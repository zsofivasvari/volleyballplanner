using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolleyPlanner.API.Data;
using VolleyPlanner.API.DTOs.Exercise;

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
    public async Task<ActionResult<IEnumerable<TagDto>>> GetAll()
    {
        var tags = await _context.Tags
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