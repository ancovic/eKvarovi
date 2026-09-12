using eKvarovi.Api.Data;
using eKvarovi.Shared.DTOs;
using eKvarovi.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TechniciansController : ControllerBase
{
    private readonly EKvaroviDbContext _context;

    public TechniciansController(EKvaroviDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<TechnicianDto>>> GetTechnicians(
        [FromQuery] bool? isActive)
    {
        var query = _context.Technicians
            .AsQueryable();

        if (isActive.HasValue)
        {
            query = query.Where(technician =>
                technician.IsActive == isActive.Value);
        }

        var technicians = await query
            .OrderBy(technician => technician.LastName)
            .ThenBy(technician => technician.FirstName)
            .ToListAsync();

        var result = technicians
            .Select(ToDto)
            .ToList();

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TechnicianDto>> GetTechnicianById(int id)
    {
        var technician = await _context.Technicians
            .FirstOrDefaultAsync(technician => technician.Id == id);

        if (technician is null)
        {
            return NotFound();
        }

        return Ok(ToDto(technician));
    }

    [HttpPost]
    public async Task<ActionResult<TechnicianDto>> CreateTechnician(
        SaveTechnicianDto request)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName))
        {
            return BadRequest(
                "Ime i prezime izvršitelja su obavezni.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(
                "Email izvršitelja je obavezan.");
        }

        var technician = new Technician
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim(),
            Phone = string.IsNullOrWhiteSpace(request.Phone)
                ? null
                : request.Phone.Trim(),
            Specialization = string.IsNullOrWhiteSpace(
                request.Specialization)
                ? null
                : request.Specialization.Trim(),
            IsActive = request.IsActive
        };

        _context.Technicians.Add(technician);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetTechnicianById),
            new { id = technician.Id },
            ToDto(technician));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTechnician(
        int id,
        SaveTechnicianDto request)
    {
        var technician = await _context.Technicians.FindAsync(id);

        if (technician is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName))
        {
            return BadRequest(
                "Ime i prezime izvršitelja su obavezni.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(
                "Email izvršitelja je obavezan.");
        }

        technician.FirstName = request.FirstName.Trim();
        technician.LastName = request.LastName.Trim();
        technician.Email = request.Email.Trim();
        technician.Phone = string.IsNullOrWhiteSpace(request.Phone)
            ? null
            : request.Phone.Trim();
        technician.Specialization =
            string.IsNullOrWhiteSpace(request.Specialization)
                ? null
                : request.Specialization.Trim();
        technician.IsActive = request.IsActive;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTechnician(int id)
    {
        var technician = await _context.Technicians
            .Include(technician => technician.WorkAssignments)
            .FirstOrDefaultAsync(technician => technician.Id == id);

        if (technician is null)
        {
            return NotFound();
        }

        if (technician.WorkAssignments.Count > 0)
        {
            return BadRequest(
                "Izvršitelja nije moguće obrisati jer ima povezane radne naloge. Izvršitelja deaktivirajte.");
        }

        // logika ista kao i za DeleteEmployee()
        var linkedUserExists = await _context.AppUsers
            .AnyAsync(user => user.TechnicianId == id);

        if (linkedUserExists)
        {
            return BadRequest(
                "Izvršitelja nije moguće obrisati jer je povezan s korisničkim računom. Izvršitelja deaktivirajte.");
        }

        _context.Technicians.Remove(technician);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("lookup")]
    public async Task<ActionResult<List<LookupDto>>> GetTechniciansLookup()
    {
        var technicians = await _context.Technicians
            .Where(technician => technician.IsActive)
            .OrderBy(technician => technician.LastName)
            .ThenBy(technician => technician.FirstName)
            .Select(technician => new LookupDto
            {
                Id = technician.Id,
                Name = technician.FirstName + " " + technician.LastName
            })
            .ToListAsync();

        return Ok(technicians);
    }

    private static TechnicianDto ToDto(Technician technician)
    {
        return new TechnicianDto
        {
            Id = technician.Id,
            FirstName = technician.FirstName,
            LastName = technician.LastName,
            Email = technician.Email,
            Phone = technician.Phone,
            Specialization = technician.Specialization,
            IsActive = technician.IsActive
        };
    }
}