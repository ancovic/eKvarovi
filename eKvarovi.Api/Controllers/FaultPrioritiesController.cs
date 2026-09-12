using eKvarovi.Api.Data;
using eKvarovi.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FaultPrioritiesController : ControllerBase
{
    private readonly EKvaroviDbContext _context;

    public FaultPrioritiesController(EKvaroviDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<LookupDto>>> GetFaultPriorities()
    {
        var priorities = await _context.FaultPriorities
            .OrderBy(priority => priority.Id)
            .Select(priority => new LookupDto
            {
                Id = priority.Id,
                Name = priority.Name
            })
            .ToListAsync();

        return Ok(priorities);
    }
}