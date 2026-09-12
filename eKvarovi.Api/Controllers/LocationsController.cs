using eKvarovi.Api.Data;
using eKvarovi.Shared.DTOs;
using eKvarovi.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocationsController : ControllerBase
{
    private readonly EKvaroviDbContext _context;

    public LocationsController(EKvaroviDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<LocationDto>>> GetLocations(
    [FromQuery] int? locationTypeId)
    {
        var query = _context.Locations
            .Include(location => location.LocationType)
            .AsQueryable();

        if (locationTypeId.HasValue)
        {
            query = query.Where(location =>
                location.LocationTypeId == locationTypeId.Value);
        }

        var locations = await query
            .OrderBy(location => location.Name)
            .ToListAsync();

        var result = locations
            .Select(ToDto)
            .ToList();

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LocationDto>> GetLocationById(int id)
    {
        var location = await _context.Locations
            .Include(location => location.LocationType)
            .FirstOrDefaultAsync(location => location.Id == id);

        if (location is null)
        {
            return NotFound();
        }

        return Ok(ToDto(location));
    }

    [HttpPost]
    public async Task<ActionResult<LocationDto>> CreateLocation(
        SaveLocationDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Naziv lokacije je obavezan.");
        }

        if (string.IsNullOrWhiteSpace(request.Address))
        {
            return BadRequest("Adresa lokacije je obavezna.");
        }

        if (request.LocationTypeId <= 0)
        {
            return BadRequest("Vrsta lokacije je obavezna.");
        }

        var locationTypeExists = await _context.LocationTypes
            .AnyAsync(type => type.Id == request.LocationTypeId);

        if (!locationTypeExists)
        {
            return BadRequest("Odabrana vrsta lokacije ne postoji.");
        }

        var location = new Location
        {
            Name = request.Name.Trim(),
            Address = request.Address.Trim(),
            City = string.IsNullOrWhiteSpace(request.City) ? null : request.City.Trim(),
            IsActive = request.IsActive,
            LocationTypeId = request.LocationTypeId
        };

        _context.Locations.Add(location);
        await _context.SaveChangesAsync();

        await _context.Entry(location)
            .Reference(item => item.LocationType)
            .LoadAsync();

        return CreatedAtAction(
            nameof(GetLocationById),
            new { id = location.Id },
            ToDto(location));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLocation(
        int id,
        SaveLocationDto request)
    {
        var location = await _context.Locations.FindAsync(id);

        if (location is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Naziv lokacije je obavezan.");
        }

        if (string.IsNullOrWhiteSpace(request.Address))
        {
            return BadRequest("Adresa lokacije je obavezna.");
        }

        if (request.LocationTypeId <= 0)
        {
            return BadRequest("Vrsta lokacije je obavezna.");
        }

        var locationTypeExists = await _context.LocationTypes
            .AnyAsync(type => type.Id == request.LocationTypeId);

        if (!locationTypeExists)
        {
            return BadRequest("Odabrana vrsta lokacije ne postoji.");
        }

        location.Name = request.Name.Trim();
        location.Address = request.Address.Trim();
        location.City = string.IsNullOrWhiteSpace(request.City)
            ? null
            : request.City.Trim();
        location.IsActive = request.IsActive;
        location.LocationTypeId = request.LocationTypeId;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLocation(int id)
    {
        var location = await _context.Locations
            .Include(location => location.Employees)
            .Include(location => location.FaultReports)
            .FirstOrDefaultAsync(location => location.Id == id);

        if (location is null)
        {
            return NotFound();
        }

        if (location.Employees.Count > 0 ||
            location.FaultReports.Count > 0)
        {
            return BadRequest(
                "Lokaciju nije moguće obrisati jer ima povezane djelatnike ili prijave kvarova. Lokaciju deaktivirajte.");
        }

        _context.Locations.Remove(location);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("lookup")]
    public async Task<ActionResult<List<LookupDto>>> GetLocationsLookup()
    {
        var locations = await _context.Locations
            .Where(location => location.IsActive)
            .OrderBy(location => location.Name)
            .Select(location => new LookupDto
            {
                Id = location.Id,
                Name = location.Name
            })
            .ToListAsync();

        return Ok(locations);
    }

    private static LocationDto ToDto(Location location)
    {
        return new LocationDto
        {
            Id = location.Id,
            Name = location.Name,
            Address = location.Address,
            City = location.City,
            IsActive = location.IsActive,
            LocationTypeId = location.LocationTypeId,
            LocationTypeName = location.LocationType != null ? location.LocationType.Name : ""
        };
    }
}