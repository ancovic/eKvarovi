using eKvarovi.Api.Data;
using eKvarovi.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Microsoft.AspNetCore.Authorization.Authorize(
    Policy = eKvarovi.Api.Security.AuthorizationPolicies.Management)]
public class FaultStatusesController : ControllerBase
{
    private readonly EKvaroviDbContext _context;

    public FaultStatusesController(EKvaroviDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<LookupDto>>> GetFaultStatuses()
    {
        var statuses = await _context.FaultStatuses
            .OrderBy(status => status.Id)
            .Select(status => new LookupDto
            {
                Id = status.Id,
                Name = status.Name
            })
            .ToListAsync();

        return Ok(statuses);
    }
}