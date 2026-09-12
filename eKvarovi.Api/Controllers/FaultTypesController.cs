using eKvarovi.Api.Data;
using eKvarovi.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FaultTypesController : ControllerBase
{
    private readonly EKvaroviDbContext _context;

    public FaultTypesController(EKvaroviDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<LookupDto>>> GetFaultTypes()
    {
        var types = await _context.FaultTypes
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