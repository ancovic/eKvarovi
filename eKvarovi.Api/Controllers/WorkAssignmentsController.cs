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
public class WorkAssignmentsController : ControllerBase
{
    private readonly EKvaroviDbContext _context;

    public WorkAssignmentsController(
        EKvaroviDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize(
        Policy = AuthorizationPolicies.Management)]
    public async Task<ActionResult<List<WorkAssignmentDto>>> GetWorkAssignments(
        [FromQuery] int? faultReportId,
        [FromQuery] int? technicianId,
        [FromQuery] bool? isActive)
    {
        var query = _context.WorkAssignments
            .Include(assignment =>
                assignment.FaultReport)
                .ThenInclude(report =>
                    report!.Location)
            .Include(assignment =>
                assignment.FaultReport)
                .ThenInclude(report =>
                    report!.FaultStatus)
            .Include(assignment =>
                assignment.Technician)
            .AsQueryable();

        if (faultReportId.HasValue)
        {
            query = query.Where(
                assignment =>
                    assignment.FaultReportId ==
                    faultReportId.Value);
        }

        if (technicianId.HasValue)
        {
            query = query.Where(
                assignment =>
                    assignment.TechnicianId ==
                    technicianId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(
                assignment =>
                    assignment.IsActive ==
                    isActive.Value);
        }

        var assignments = await query
            .OrderByDescending(
                assignment =>
                    assignment.IsActive)
            .ThenByDescending(
                assignment =>
                    assignment.AssignedAt)
            .ToListAsync();

        return Ok(
            assignments
                .Select(ToDto)
                .ToList());
    }

    [HttpGet("mine")]
    [Authorize(
        Policy = AuthorizationPolicies.TechnicianOnly)]
    public async Task<ActionResult<List<WorkAssignmentDto>>> GetMyAssignments()
    {
        var technicianIdValue =
            User.FindFirstValue(
                AppClaimTypes.TechnicianId);

        if (!int.TryParse(
                technicianIdValue,
                out var technicianId))
        {
            return Forbid();
        }

        var assignments =
            await _context.WorkAssignments
                .Include(assignment =>
                    assignment.FaultReport)
                    .ThenInclude(report =>
                        report!.Location)
                .Include(assignment =>
                    assignment.FaultReport)
                    .ThenInclude(report =>
                        report!.FaultStatus)
                .Include(assignment =>
                    assignment.Technician)
                .Where(assignment =>
                    assignment.TechnicianId ==
                    technicianId)
                .OrderByDescending(
                    assignment =>
                        assignment.IsActive)
                .ThenByDescending(
                    assignment =>
                        assignment.AssignedAt)
                .ToListAsync();

        return Ok(
            assignments
                .Select(ToDto)
                .ToList());
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<WorkAssignmentDto>> GetWorkAssignmentById(
        int id)
    {
        var assignment =
            await _context.WorkAssignments
                .Include(item =>
                    item.FaultReport)
                    .ThenInclude(report =>
                        report!.Location)
                .Include(item =>
                    item.FaultReport)
                    .ThenInclude(report =>
                        report!.FaultStatus)
                .Include(item =>
                    item.Technician)
                .FirstOrDefaultAsync(
                    item =>
                        item.Id == id);

        if (assignment is null)
        {
            return NotFound();
        }

        if (User.IsInRole("Admin") ||
            User.IsInRole("Manager"))
        {
            return Ok(ToDto(assignment));
        }

        if (User.IsInRole("Technician"))
        {
            var technicianIdValue =
                User.FindFirstValue(
                    AppClaimTypes.TechnicianId);

            if (int.TryParse(
                    technicianIdValue,
                    out var technicianId) &&
                assignment.TechnicianId ==
                    technicianId)
            {
                return Ok(ToDto(assignment));
            }
        }

        return Forbid();
    }

    [HttpGet("faultreport/{faultReportId:int}/latest")]
    [Authorize]
    public async Task<ActionResult<WorkAssignmentDto?>> GetLatestForFaultReport(
    int faultReportId)
    {
        var report =
            await _context.FaultReports
                .FirstOrDefaultAsync(
                    report =>
                        report.Id == faultReportId);

        if (report is null)
        {
            return NotFound();
        }

        var query =
            _context.WorkAssignments
                .Include(item =>
                    item.FaultReport)
                    .ThenInclude(report =>
                        report!.Location)
                .Include(item =>
                    item.FaultReport)
                    .ThenInclude(report =>
                        report!.FaultStatus)
                .Include(item =>
                    item.Technician)
                .Where(item =>
                    item.FaultReportId ==
                        faultReportId)
                .AsQueryable();

        if (User.IsInRole("Admin") ||
            User.IsInRole("Manager"))
        {
            // Manager i Admin vide zadnju dodjelu prijave.
        }
        else if (User.IsInRole("Reporter"))
        {
            var employeeIdValue =
                User.FindFirstValue(
                    AppClaimTypes.EmployeeId);

            if (!int.TryParse(
                    employeeIdValue,
                    out var employeeId) ||
                report.ReporterId != employeeId)
            {
                return Forbid();
            }
        }
        else if (User.IsInRole("Technician"))
        {
            var technicianIdValue =
                User.FindFirstValue(
                    AppClaimTypes.TechnicianId);

            if (!int.TryParse(
                    technicianIdValue,
                    out var technicianId))
            {
                return Forbid();
            }

            query = query.Where(
                item =>
                    item.TechnicianId ==
                        technicianId);
        }
        else
        {
            return Forbid();
        }

        var assignment =
            await query
                .OrderByDescending(item =>
                    item.IsActive)
                .ThenByDescending(item =>
                    item.AssignedAt)
                .FirstOrDefaultAsync();

        if (assignment is null)
        {
            return Ok(null);
        }

        return Ok(ToDto(assignment));
    }

    [HttpPost]
    [Authorize(
        Policy = AuthorizationPolicies.Management)]
    public async Task<ActionResult<WorkAssignmentDto>> SaveWorkAssignment(
        SaveWorkAssignmentDto request)
    {
        if (request.FaultReportId <= 0 ||
            request.TechnicianId <= 0)
        {
            return BadRequest(
                "Prijava i izvršitelj su obavezni.");
        }

        var report =
            await _context.FaultReports
                .Include(item =>
                    item.Location)
                .Include(item =>
                    item.FaultStatus)
                .Include(item =>
                    item.FaultPriority)
                .FirstOrDefaultAsync(
                    item =>
                        item.Id ==
                        request.FaultReportId);

        if (report is null)
        {
            return NotFound(
                "Prijava kvara nije pronađena.");
        }

        if (report.IsArchived)
        {
            return BadRequest(
                "Arhiviranu prijavu nije moguće dodijeliti.");
        }

        if (report.FaultStatus?.Name ==
            "Zaprimljeno")
        {
            return BadRequest(
                "Prijava mora biti pregledana prije dodjele izvršitelju.");
        }

        if (report.FaultStatus?.Name ==
                "Riješeno" ||
            report.FaultStatus?.Name ==
                "Zatvoreno")
        {
            return BadRequest(
                "Riješenu ili zatvorenu prijavu nije moguće dodijeliti.");
        }

        if (!report.FaultTypeId.HasValue ||
            !report.FaultPriorityId.HasValue)
        {
            return BadRequest(
                "Prije dodjele potrebno je odrediti vrstu i prioritet kvara.");
        }

        if (report.FaultPriority?.Name ==
                "Kritičan" &&
            !report.DueDate.HasValue)
        {
            return BadRequest(
                "Kritična prijava mora imati rok.");
        }

        var technician =
            await _context.Technicians
                .FirstOrDefaultAsync(
                    item =>
                        item.Id ==
                        request.TechnicianId);

        if (technician is null)
        {
            return BadRequest(
                "Izvršitelj nije pronađen.");
        }

        if (!technician.IsActive)
        {
            return BadRequest(
                "Prijavu nije moguće dodijeliti neaktivnom izvršitelju.");
        }

        var activeAssignment =
            await _context.WorkAssignments
                .FirstOrDefaultAsync(
                    item =>
                        item.FaultReportId ==
                            report.Id &&
                        item.IsActive);

        if (activeAssignment is not null &&
            activeAssignment.TechnicianId ==
                technician.Id)
        {
            return BadRequest(
                "Prijava je već aktivno dodijeljena ovom izvršitelju.");
        }

        var assignedStatus =
            await _context.FaultStatuses
                .FirstOrDefaultAsync(
                    status =>
                        status.Name ==
                        "Dodijeljeno");

        if (assignedStatus is null)
        {
            return Problem(
                "Status Dodijeljeno nije pronađen.");
        }

        var now = DateTime.UtcNow;

        await using var transaction =
            await _context.Database
                .BeginTransactionAsync();

        if (activeAssignment is not null)
        {
            activeAssignment.IsActive = false;
            activeAssignment.EndedAt = now;

            await _context.SaveChangesAsync();
        }

        var assignment =
            new WorkAssignment
            {
                AssignedAt = now,
                IsActive = true,

                Note =
                    string.IsNullOrWhiteSpace(
                        request.Note)
                        ? null
                        : request.Note.Trim(),

                FaultReportId =
                    report.Id,

                FaultReport =
                    report,

                TechnicianId =
                    technician.Id,

                Technician =
                    technician
            };

        _context.WorkAssignments.Add(
            assignment);

        report.FaultStatusId =
            assignedStatus.Id;

        report.FaultStatus =
            assignedStatus;

        await _context.SaveChangesAsync();

        await transaction.CommitAsync();

        return CreatedAtAction(
            nameof(GetWorkAssignmentById),
            new { id = assignment.Id },
            ToDto(assignment));
    }

    private static WorkAssignmentDto ToDto(
        WorkAssignment assignment)
    {
        return new WorkAssignmentDto
        {
            Id = assignment.Id,

            AssignedAt =
                assignment.AssignedAt,

            EndedAt =
                assignment.EndedAt,

            IsActive =
                assignment.IsActive,

            Note =
                assignment.Note,

            FaultReportId =
                assignment.FaultReportId,

            FaultReportTitle =
                assignment.FaultReport?.Title
                ?? string.Empty,

            LocationId =
                assignment.FaultReport?.LocationId
                ?? 0,

            LocationName =
                assignment.FaultReport?.Location?.Name
                ?? string.Empty,

            FaultStatusId =
                assignment.FaultReport?.FaultStatusId
                ?? 0,

            FaultStatusName =
                assignment.FaultReport?.FaultStatus?.Name
                ?? string.Empty,

            TechnicianId =
                assignment.TechnicianId,

            TechnicianName =
                assignment.Technician is null
                    ? string.Empty
                    : $"{assignment.Technician.FirstName} {assignment.Technician.LastName}"
        };
    }
}