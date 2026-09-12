using eKvarovi.Api.Data;
using eKvarovi.Shared.DTOs;
using eKvarovi.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly EKvaroviDbContext _context;

    public EmployeesController(EKvaroviDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<EmployeeDto>>> GetEmployees(
        [FromQuery] int? locationId)
    {
        var query = _context.Employees
            .Include(employee => employee.Location)
            .AsQueryable();

        if (locationId.HasValue)
        {
            query = query.Where(employee =>
                employee.LocationId == locationId.Value);
        }

        var employees = await query
            .OrderBy(employee => employee.LastName)
            .ThenBy(employee => employee.FirstName)
            .ToListAsync();

        var result = employees
            .Select(ToDto)
            .ToList();

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeDto>> GetEmployeeById(int id)
    {
        var employee = await _context.Employees
            .Include(employee => employee.Location)
            .FirstOrDefaultAsync(employee => employee.Id == id);

        if (employee is null)
        {
            return NotFound();
        }

        return Ok(ToDto(employee));
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> CreateEmployee(
        SaveEmployeeDto request)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName))
        {
            return BadRequest("Ime i prezime djelatnika su obavezni.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest("Email djelatnika je obavezan.");
        }

        if (request.LocationId <= 0)
        {
            return BadRequest("Lokacija djelatnika je obavezna.");
        }

        var locationExists = await _context.Locations
            .AnyAsync(location =>
                location.Id == request.LocationId &&
                location.IsActive);

        if (!locationExists)
        {
            return BadRequest(
                "Odabrana lokacija ne postoji ili nije aktivna.");
        }

        var employee = new Employee
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim(),
            Phone = string.IsNullOrWhiteSpace(request.Phone)
                ? null
                : request.Phone.Trim(),
            IsActive = request.IsActive,
            LocationId = request.LocationId
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        await _context.Entry(employee)
            .Reference(item => item.Location)
            .LoadAsync();

        return CreatedAtAction(
            nameof(GetEmployeeById),
            new { id = employee.Id },
            ToDto(employee));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEmployee(
        int id,
        SaveEmployeeDto request)
    {
        var employee = await _context.Employees.FindAsync(id);

        if (employee is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName))
        {
            return BadRequest("Ime i prezime djelatnika su obavezni.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest("Email djelatnika je obavezan.");
        }

        if (request.LocationId <= 0)
        {
            return BadRequest("Lokacija djelatnika je obavezna.");
        }

        if (request.LocationId != employee.LocationId)
        {
            var locationExists = await _context.Locations
                .AnyAsync(location =>
                    location.Id == request.LocationId &&
                    location.IsActive);

            if (!locationExists)
            {
                return BadRequest(
                    "Odabrana lokacija ne postoji ili nije aktivna.");
            }
        }

        employee.FirstName = request.FirstName.Trim();
        employee.LastName = request.LastName.Trim();
        employee.Email = request.Email.Trim();
        employee.Phone = string.IsNullOrWhiteSpace(request.Phone)
            ? null
            : request.Phone.Trim();
        employee.IsActive = request.IsActive;
        employee.LocationId = request.LocationId;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        var employee = await _context.Employees
            .Include(employee => employee.ReportedFaults)
            .FirstOrDefaultAsync(employee => employee.Id == id);

        if (employee is null)
        {
            return NotFound();
        }

        if (employee.ReportedFaults.Count > 0)
        {
            return BadRequest(
                "Djelatnika nije moguće obrisati jer ima povezane prijave kvarova. Djelatnika deaktivirajte.");
        }

        var linkedUserExists = await _context.AppUsers
            .AnyAsync(user => user.EmployeeId == id);

        // zbog nezeljenog stanja ako zelimo obrisati employee profil povezan 
        // sa korisnickim racunom
        if (linkedUserExists)
        {
            return BadRequest(
                "Djelatnika nije moguće obrisati jer je povezan s korisničkim računom. Djelatnika deaktivirajte.");
        }

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("lookup")]
    public async Task<ActionResult<List<LookupDto>>> GetEmployeesLookup()
    {
        var employees = await _context.Employees
            .Where(employee => employee.IsActive)
            .OrderBy(employee => employee.LastName)
            .ThenBy(employee => employee.FirstName)
            .Select(employee => new LookupDto
            {
                Id = employee.Id,
                Name = employee.FirstName + " " + employee.LastName
            })
            .ToListAsync();

        return Ok(employees);
    }

    private static EmployeeDto ToDto(Employee employee)
    {
        return new EmployeeDto
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            Phone = employee.Phone,
            IsActive = employee.IsActive,
            LocationId = employee.LocationId,
            LocationName = employee.Location != null
                ? employee.Location.Name
                : ""
        };
    }
}