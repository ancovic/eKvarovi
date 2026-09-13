using eKvarovi.Api.Data;
using eKvarovi.Api.Security;
using eKvarovi.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(
    Policy = AuthorizationPolicies.Management)]
public class MaterialUnitsController : ControllerBase
{
    private readonly EKvaroviDbContext _context;

    public MaterialUnitsController(
        EKvaroviDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<LookupDto>>> GetMaterialUnits()
    {
        var units =
            await _context.MaterialUnits
                .OrderBy(unit => unit.Id)
                .Select(unit =>
                    new LookupDto
                    {
                        Id = unit.Id,
                        Name = unit.Name
                    })
                .ToListAsync();

        return Ok(units);
    }
}