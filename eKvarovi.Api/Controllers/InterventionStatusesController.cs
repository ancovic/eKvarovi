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
public class InterventionStatusesController : ControllerBase
{
    private readonly EKvaroviDbContext _context;

    public InterventionStatusesController(
        EKvaroviDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<LookupDto>>> GetInterventionStatuses()
    {
        var statuses =
            await _context.InterventionStatuses
                .OrderBy(status => status.Id)
                .Select(status =>
                    new LookupDto
                    {
                        Id = status.Id,
                        Name = status.Name
                    })
                .ToListAsync();

        return Ok(statuses);
    }
}