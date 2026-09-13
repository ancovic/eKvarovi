using System.Security.Claims;
using eKvarovi.Api.Data;
using eKvarovi.Api.Security;
using eKvarovi.Shared.DTOs;
using eKvarovi.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FaultReportsController : ControllerBase
{
    private readonly EKvaroviDbContext _context;

    public FaultReportsController(EKvaroviDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Microsoft.AspNetCore.Authorization.Authorize(
    Policy = eKvarovi.Api.Security.AuthorizationPolicies.Management)]
    public async Task<ActionResult<List<FaultReportDto>>> GetFaultReports(
        [FromQuery] string? search,
        [FromQuery] int? locationId,
        [FromQuery] int? typeId,
        [FromQuery] int? priorityId,
        [FromQuery] int? statusId,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string? sortBy,
        [FromQuery] bool sortDescending = true,
        [FromQuery] bool includeArchived = false)
    {
        var query = _context.FaultReports
            .Include(report => report.Location)
            .Include(report => report.Reporter)
            .Include(report => report.FaultStatus)
            .Include(report => report.FaultType)
            .Include(report => report.FaultPriority)
            .AsQueryable();

        if (!includeArchived)
        {
            query = query.Where(report => !report.IsArchived);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchValue = $"%{search.Trim()}%";

            query = query.Where(report =>
                EF.Functions.Like(report.Title, searchValue) ||
                EF.Functions.Like(report.Description, searchValue) ||
                (report.Location != null &&
                    EF.Functions.Like(
                        report.Location.Name,
                        searchValue)) ||
                (report.Reporter != null &&
                    (EF.Functions.Like(
                        report.Reporter.FirstName,
                        searchValue) ||
                     EF.Functions.Like(
                        report.Reporter.LastName,
                        searchValue))));
        }

        if (locationId.HasValue)
        {
            query = query.Where(report =>
                report.LocationId == locationId.Value);
        }

        if (typeId.HasValue)
        {
            query = query.Where(report =>
                report.FaultTypeId == typeId.Value);
        }

        if (priorityId.HasValue)
        {
            query = query.Where(report =>
                report.FaultPriorityId == priorityId.Value);
        }

        if (statusId.HasValue)
        {
            query = query.Where(report =>
                report.FaultStatusId == statusId.Value);
        }

        if (dateFrom.HasValue)
        {
            query = query.Where(report =>
                report.CreatedAt >= dateFrom.Value.Date);
        }

        if (dateTo.HasValue)
        {
            var endDate = dateTo.Value.Date.AddDays(1);

            query = query.Where(report =>
                report.CreatedAt < endDate);
        }

        query = sortBy?.ToLowerInvariant() switch
        {
            "title" => sortDescending
                ? query.OrderByDescending(report => report.Title)
                : query.OrderBy(report => report.Title),

            "location" => sortDescending
                ? query.OrderByDescending(
                    report => report.Location!.Name)
                : query.OrderBy(
                    report => report.Location!.Name),

            "status" => sortDescending
                ? query.OrderByDescending(
                    report => report.FaultStatus!.Name)
                : query.OrderBy(
                    report => report.FaultStatus!.Name),

            "priority" => sortDescending
                ? query.OrderByDescending(
                    report => report.FaultPriorityId)
                : query.OrderBy(
                    report => report.FaultPriorityId),

            "duedate" => sortDescending
                ? query.OrderByDescending(
                    report => report.DueDate)
                : query.OrderBy(
                    report => report.DueDate),

            _ => sortDescending
                ? query.OrderByDescending(
                    report => report.CreatedAt)
                : query.OrderBy(
                    report => report.CreatedAt)
        };

        var reports = await query.ToListAsync();

        return Ok(
            reports
                .Select(ToDto)
                .ToList());
    }

    [HttpGet("mine")]
    [Microsoft.AspNetCore.Authorization.Authorize(
    Policy = eKvarovi.Api.Security.AuthorizationPolicies.ReporterOnly)]
    public async Task<ActionResult<List<FaultReportDto>>> GetMyFaultReports()
    {
        var employeeIdValue =
            User.FindFirstValue(AppClaimTypes.EmployeeId);

        if (!int.TryParse(employeeIdValue, out var employeeId))
        {
            return Forbid();
        }

        var reports = await _context.FaultReports
            .Include(report => report.Location)
            .Include(report => report.Reporter)
            .Include(report => report.FaultStatus)
            .Include(report => report.FaultType)
            .Include(report => report.FaultPriority)
            .Where(report =>
                report.ReporterId == employeeId &&
                !report.IsArchived)
            .OrderByDescending(report => report.CreatedAt)
            .ToListAsync();

        return Ok(
            reports
                .Select(ToDto)
                .ToList());
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<FaultReportDto>> GetFaultReportById(int id)
    {
        var report = await _context.FaultReports
            .Include(report => report.Location)
            .Include(report => report.Reporter)
            .Include(report => report.FaultStatus)
            .Include(report => report.FaultType)
            .Include(report => report.FaultPriority)
            .Include(report => report.Assignments)
            .FirstOrDefaultAsync(report =>
                report.Id == id);

        if (report is null)
        {
            return NotFound();
        }

        if (User.IsInRole("Admin") ||
            User.IsInRole("Manager"))
        {
            return Ok(ToDto(report));
        }

        if (User.IsInRole("Reporter"))
        {
            var employeeIdValue =
                User.FindFirstValue(
                    AppClaimTypes.EmployeeId);

            if (int.TryParse(
                    employeeIdValue,
                    out var employeeId) &&
                report.ReporterId == employeeId)
            {
                return Ok(ToDto(report));
            }
        }

        if (User.IsInRole("Technician"))
        {
            var technicianIdValue =
                User.FindFirstValue(
                    AppClaimTypes.TechnicianId);

            if (int.TryParse(
                    technicianIdValue,
                    out var technicianId) &&
                report.Assignments.Any(
                    assignment =>
                        assignment.TechnicianId ==
                        technicianId))
            {
                return Ok(ToDto(report));
            }
        }

        return Forbid();
    }

    [HttpPost]
    [Microsoft.AspNetCore.Authorization.Authorize(
    Policy = eKvarovi.Api.Security.AuthorizationPolicies.ReporterOnly)]
    public async Task<ActionResult<FaultReportDto>> CreateFaultReport(
        CreateFaultReportDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(
                "Naslov prijave je obavezan.");
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            return BadRequest(
                "Opis prijave je obavezan.");
        }

        var employeeIdValue =
            User.FindFirstValue(AppClaimTypes.EmployeeId);

        if (!int.TryParse(employeeIdValue, out var employeeId))
        {
            return Forbid();
        }

        var employee = await _context.Employees
            .Include(employee => employee.Location)
            .FirstOrDefaultAsync(employee =>
                employee.Id == employeeId);

        if (employee is null || !employee.IsActive)
        {
            return BadRequest(
                "Prijavljeni korisnik nije povezan s aktivnim djelatnikom.");
        }

        if (employee.Location is null ||
            !employee.Location.IsActive)
        {
            return BadRequest(
                "Lokacija prijavitelja nije aktivna.");
        }

        var receivedStatusId = await _context.FaultStatuses
            .Where(status =>
                status.Name == "Zaprimljeno")
            .Select(status => status.Id)
            .FirstAsync();

        var report = new FaultReport
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),

            CreatedAt = DateTime.UtcNow,

            ReporterId = employee.Id,
            LocationId = employee.LocationId,

            FaultStatusId = receivedStatusId,

            IsArchived = false
        };

        _context.FaultReports.Add(report);
        await _context.SaveChangesAsync();

        await _context.Entry(report)
            .Reference(item => item.Location)
            .LoadAsync();

        await _context.Entry(report)
            .Reference(item => item.Reporter)
            .LoadAsync();

        await _context.Entry(report)
            .Reference(item => item.FaultStatus)
            .LoadAsync();

        return CreatedAtAction(
            nameof(GetFaultReportById),
            new { id = report.Id },
            ToDto(report));
    }

    [HttpPut("{id:int}/review")]
    [Microsoft.AspNetCore.Authorization.Authorize(
    Policy = eKvarovi.Api.Security.AuthorizationPolicies.Management)]
    public async Task<IActionResult> ReviewFaultReport(
        int id,
        ReviewFaultReportDto request)
    {
        var report = await _context.FaultReports
            .FirstOrDefaultAsync(report =>
                report.Id == id);

        if (report is null)
        {
            return NotFound();
        }

        if (report.IsArchived)
        {
            return BadRequest(
                "Arhiviranu prijavu nije moguće pregledavati.");
        }

        var faultTypeExists =
            await _context.FaultTypes
                .AnyAsync(type =>
                    type.Id == request.FaultTypeId);

        if (!faultTypeExists)
        {
            return BadRequest(
                "Odabrana vrsta kvara ne postoji.");
        }

        var priority =
            await _context.FaultPriorities
                .FirstOrDefaultAsync(priority =>
                    priority.Id ==
                    request.FaultPriorityId);

        if (priority is null)
        {
            return BadRequest(
                "Odabrani prioritet ne postoji.");
        }

        if (priority.Name == "Kritičan" &&
            !request.DueDate.HasValue)
        {
            return BadRequest(
                "Kritična prijava mora imati rok.");
        }

        report.FaultTypeId =
            request.FaultTypeId;

        report.FaultPriorityId =
            request.FaultPriorityId;

        report.DueDate =
            request.DueDate;

        var receivedStatusId =
            await _context.FaultStatuses
                .Where(status =>
                    status.Name == "Zaprimljeno")
                .Select(status =>
                    status.Id)
                .FirstAsync();

        if (report.FaultStatusId ==
            receivedStatusId)
        {
            var reviewedStatusId =
                await _context.FaultStatuses
                    .Where(status =>
                        status.Name == "Pregledano")
                    .Select(status =>
                        status.Id)
                    .FirstAsync();

            report.FaultStatusId =
                reviewedStatusId;
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("{id:int}/close")]
    [Microsoft.AspNetCore.Authorization.Authorize(
    Roles = "Manager")]
    public async Task<IActionResult> CloseFaultReport(
    int id)
    {
        var report =
            await _context.FaultReports
                .Include(report =>
                    report.FaultStatus)
                .FirstOrDefaultAsync(report =>
                    report.Id == id);

        if (report is null)
        {
            return NotFound(
                "Prijava kvara nije pronađena.");
        }

        if (report.IsArchived)
        {
            return BadRequest(
                "Arhiviranu prijavu nije moguće zatvoriti.");
        }

        if (report.FaultStatus?.Name != "Riješeno")
        {
            return BadRequest(
                "Zatvoriti je moguće samo riješenu prijavu.");
        }

        var completedStatus =
            await _context.InterventionStatuses
                .FirstOrDefaultAsync(status =>
                    status.Name == "Završena");

        if (completedStatus is null)
        {
            return BadRequest(
                "Status završene intervencije nije pronađen.");
        }

        var hasSuccessfulIntervention =
            await _context.Interventions
                .AnyAsync(intervention =>
                    intervention.WorkAssignment != null &&
                    intervention.WorkAssignment.FaultReportId == id &&
                    intervention.InterventionStatusId ==
                        completedStatus.Id);

        if (!hasSuccessfulIntervention)
        {
            return BadRequest(
                "Prijavu nije moguće zatvoriti bez uspješno završene intervencije.");
        }

        var closedStatus =
            await _context.FaultStatuses
                .FirstOrDefaultAsync(status =>
                    status.Name == "Zatvoreno");

        if (closedStatus is null)
        {
            return BadRequest(
                "Status Zatvoreno nije pronađen.");
        }

        report.FaultStatusId =
            closedStatus.Id;

        report.ClosedAt =
            DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static FaultReportDto ToDto(
        FaultReport report)
    {
        return new FaultReportDto
        {
            Id = report.Id,

            Title = report.Title,
            Description = report.Description,

            CreatedAt = report.CreatedAt,
            DueDate = report.DueDate,
            ResolvedAt = report.ResolvedAt,
            ClosedAt = report.ClosedAt,

            IsArchived = report.IsArchived,
            ArchivedAt = report.ArchivedAt,

            LocationId = report.LocationId,
            LocationName =
                report.Location?.Name ?? "",

            ReporterId = report.ReporterId,
            ReporterName =
                report.Reporter is not null
                    ? report.Reporter.FirstName +
                      " " +
                      report.Reporter.LastName
                    : "",

            FaultStatusId =
                report.FaultStatusId,

            FaultStatusName =
                report.FaultStatus?.Name ?? "",

            FaultTypeId =
                report.FaultTypeId,

            FaultTypeName =
                report.FaultType?.Name,

            FaultPriorityId =
                report.FaultPriorityId,

            FaultPriorityName =
                report.FaultPriority?.Name
        };
    }
}