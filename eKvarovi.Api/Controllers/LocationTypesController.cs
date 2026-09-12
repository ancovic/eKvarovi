using eKvarovi.Api.Data;
using eKvarovi.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationTypesController : ControllerBase
{
    private readonly EKvaroviDbContext _context;

    public LocationTypesController(EKvaroviDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<LookupDto>>> GetLocationTypes()
    {
        var types = await _context.LocationTypes
            .OrderBy(type => type.Id)
            .Select(type => new LookupDto
            {
                Id = type.Id,
                Name = type.Name
            })
            .ToListAsync();

        return Ok(types);
    }
}