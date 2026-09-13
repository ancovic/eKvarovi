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
public class InterventionsController : ControllerBase
{
    private readonly EKvaroviDbContext _context;

    public InterventionsController(
        EKvaroviDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize(
        Policy = AuthorizationPolicies.Management)]
    public async Task<ActionResult<List<InterventionDto>>> GetInterventions(
        [FromQuery] string? search,
        [FromQuery] int? faultReportId,
        [FromQuery] int? statusId,
        [FromQuery] int? technicianId,
        [FromQuery] DateTime? dateFrom,
        [FromQuery] DateTime? dateTo,
        [FromQuery] string? sortBy,
        [FromQuery] bool sortDescending = true)
    {
        var query =
            _context.Interventions
                .Include(intervention =>
                    intervention.InterventionStatus)
                .Include(intervention =>
                    intervention.WorkAssignment)
                    .ThenInclude(assignment =>
                        assignment!.Technician)
                .Include(intervention =>
                    intervention.WorkAssignment)
                    .ThenInclude(assignment =>
                        assignment!.FaultReport)
                        .ThenInclude(report =>
                            report!.Location)
                .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim();
            var pattern = $"%{value}%";

            if (int.TryParse(
                    value,
                    out var reportNumber))
            {
                query = query.Where(intervention =>
                    intervention.WorkAssignment!.FaultReportId ==
                        reportNumber ||
                    EF.Functions.Like(
                        intervention.WorkAssignment.FaultReport!.Title,
                        pattern) ||
                    EF.Functions.Like(
                        intervention.WorkAssignment.Technician!.FirstName,
                        pattern) ||
                    EF.Functions.Like(
                        intervention.WorkAssignment.Technician.LastName,
                        pattern) ||
                    EF.Functions.Like(
                        intervention.WorkAssignment.FaultReport.Location!.Name,
                        pattern) ||
                    (intervention.WorkNote != null &&
                     EF.Functions.Like(
                         intervention.WorkNote,
                         pattern)));
            }
            else
            {
                query = query.Where(intervention =>
                    EF.Functions.Like(
                        intervention.WorkAssignment!.FaultReport!.Title,
                        pattern) ||
                    EF.Functions.Like(
                        intervention.WorkAssignment.Technician!.FirstName,
                        pattern) ||
                    EF.Functions.Like(
                        intervention.WorkAssignment.Technician.LastName,
                        pattern) ||
                    EF.Functions.Like(
                        intervention.WorkAssignment.FaultReport.Location!.Name,
                        pattern) ||
                    (intervention.WorkNote != null &&
                     EF.Functions.Like(
                         intervention.WorkNote,
                         pattern)));
            }
        }

        if (faultReportId.HasValue)
        {
            query = query.Where(intervention =>
                intervention.WorkAssignment!.FaultReportId ==
                    faultReportId.Value);
        }

        if (statusId.HasValue)
        {
            query = query.Where(intervention =>
                intervention.InterventionStatusId ==
                    statusId.Value);
        }

        if (technicianId.HasValue)
        {
            query = query.Where(intervention =>
                intervention.WorkAssignment!.TechnicianId ==
                    technicianId.Value);
        }

        if (dateFrom.HasValue)
        {
            query = query.Where(intervention =>
                intervention.CreatedAt >=
                    dateFrom.Value.Date);
        }

        if (dateTo.HasValue)
        {
            var dateToExclusive =
                dateTo.Value.Date.AddDays(1);

            query = query.Where(intervention =>
                intervention.CreatedAt <
                    dateToExclusive);
        }

        query =
            (sortBy ?? "createdat")
                .Trim()
                .ToLowerInvariant()
            switch
            {
                "report" =>
                    sortDescending
                        ? query.OrderByDescending(intervention =>
                            intervention.WorkAssignment!.FaultReportId)
                        : query.OrderBy(intervention =>
                            intervention.WorkAssignment!.FaultReportId),

                "technician" =>
                    sortDescending
                        ? query
                            .OrderByDescending(intervention =>
                                intervention.WorkAssignment!.Technician!.LastName)
                            .ThenByDescending(intervention =>
                                intervention.WorkAssignment!.Technician!.FirstName)
                        : query
                            .OrderBy(intervention =>
                                intervention.WorkAssignment!.Technician!.LastName)
                            .ThenBy(intervention =>
                                intervention.WorkAssignment!.Technician!.FirstName),

                "status" =>
                    sortDescending
                        ? query.OrderByDescending(intervention =>
                            intervention.InterventionStatus!.Name)
                        : query.OrderBy(intervention =>
                            intervention.InterventionStatus!.Name),

                "startedat" =>
                    sortDescending
                        ? query.OrderByDescending(intervention =>
                            intervention.StartedAt)
                        : query.OrderBy(intervention =>
                            intervention.StartedAt),

                "finishedat" =>
                    sortDescending
                        ? query.OrderByDescending(intervention =>
                            intervention.FinishedAt)
                        : query.OrderBy(intervention =>
                            intervention.FinishedAt),

                _ =>
                    sortDescending
                        ? query.OrderByDescending(intervention =>
                            intervention.CreatedAt)
                        : query.OrderBy(intervention =>
                            intervention.CreatedAt)
            };

        var interventions =
            await query.ToListAsync();

        return Ok(
            interventions
                .Select(ToDto)
                .ToList());
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<ActionResult<InterventionDto>> GetInterventionById(
        int id)
    {
        var intervention =
            await GetInterventionQuery()
                .FirstOrDefaultAsync(
                    intervention =>
                        intervention.Id == id);

        if (intervention is null)
        {
            return NotFound();
        }

        if (!CanViewAssignment(
                intervention.WorkAssignment))
        {
            return Forbid();
        }

        return Ok(ToDto(intervention));
    }

    [HttpGet("workassignment/{workAssignmentId:int}")]
    [Authorize]
    public async Task<ActionResult<List<InterventionDto>>> GetForWorkAssignment(
        int workAssignmentId)
    {
        var assignment =
            await _context.WorkAssignments
                .Include(assignment =>
                    assignment.FaultReport)
                .FirstOrDefaultAsync(
                    assignment =>
                        assignment.Id ==
                        workAssignmentId);

        if (assignment is null)
        {
            return NotFound(
                "Radni nalog nije pronađen.");
        }

        if (!CanViewAssignment(
                assignment))
        {
            return Forbid();
        }

        var interventions =
            await GetInterventionQuery()
                .Where(intervention =>
                    intervention.WorkAssignmentId ==
                        workAssignmentId)
                .OrderByDescending(intervention =>
                    intervention.CreatedAt)
                .ToListAsync();

        return Ok(
            interventions
                .Select(ToDto)
                .ToList());
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<InterventionDto>> StartIntervention(
        CreateInterventionDto request)
    {
        if (request.WorkAssignmentId <= 0)
        {
            return BadRequest(
                "Radni nalog je obavezan.");
        }

        if (!User.IsInRole("Admin") &&
            !User.IsInRole("Technician"))
        {
            return Forbid();
        }

        var assignment =
            await _context.WorkAssignments
                .Include(assignment =>
                    assignment.Technician)
                .Include(assignment =>
                    assignment.FaultReport)
                    .ThenInclude(report =>
                        report!.Location)
                .Include(assignment =>
                    assignment.FaultReport)
                    .ThenInclude(report =>
                        report!.FaultStatus)
                .FirstOrDefaultAsync(
                    assignment =>
                        assignment.Id ==
                        request.WorkAssignmentId);

        if (assignment is null)
        {
            return NotFound(
                "Radni nalog nije pronađen.");
        }

        if (!User.IsInRole("Admin"))
        {
            var technicianIdValue =
                User.FindFirstValue(
                    AppClaimTypes.TechnicianId);

            if (!int.TryParse(
                    technicianIdValue,
                    out var technicianId) ||
                assignment.TechnicianId !=
                    technicianId)
            {
                return Forbid();
            }
        }

        if (!assignment.IsActive)
        {
            return BadRequest(
                "Intervenciju je moguće pokrenuti samo na aktivnom radnom nalogu.");
        }

        if (assignment.FaultReport is null ||
            assignment.FaultReport.IsArchived)
        {
            return BadRequest(
                "Na ovoj prijavi nije moguće pokrenuti intervenciju.");
        }

        if (assignment.FaultReport.FaultStatus?.Name ==
                "Riješeno" ||
            assignment.FaultReport.FaultStatus?.Name ==
                "Zatvoreno")
        {
            return BadRequest(
                "Riješena ili zatvorena prijava ne može imati novu intervenciju.");
        }

        var hasOpenIntervention =
            await _context.Interventions
                .AnyAsync(intervention =>
                    intervention.WorkAssignmentId ==
                        assignment.Id &&
                    intervention.FinishedAt == null);

        if (hasOpenIntervention)
        {
            return BadRequest(
                "Na ovom radnom nalogu već postoji intervencija u tijeku.");
        }

        var inProgressStatus =
            await _context.InterventionStatuses
                .FirstOrDefaultAsync(status =>
                    status.Name == "U tijeku");

        var reportInProgressStatus =
            await _context.FaultStatuses
                .FirstOrDefaultAsync(status =>
                    status.Name == "U radu");

        if (inProgressStatus is null ||
            reportInProgressStatus is null)
        {
            return Problem(
                "Potrebni statusi nisu pronađeni.");
        }

        var now = DateTime.UtcNow;

        var intervention =
            new Intervention
            {
                CreatedAt = now,
                StartedAt = now,
                FinishedAt = null,
                WorkNote = null,

                WorkAssignmentId =
                    assignment.Id,

                WorkAssignment =
                    assignment,

                InterventionStatusId =
                    inProgressStatus.Id,

                InterventionStatus =
                    inProgressStatus
            };

        assignment.FaultReport.FaultStatusId =
            reportInProgressStatus.Id;

        assignment.FaultReport.FaultStatus =
            reportInProgressStatus;

        _context.Interventions.Add(
            intervention);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetInterventionById),
            new { id = intervention.Id },
            ToDto(intervention));
    }

    [HttpPut("{id:int}/finish")]
    [Authorize]
    public async Task<IActionResult> FinishIntervention(
        int id,
        FinishInterventionDto request)
    {
        if (!User.IsInRole("Admin") &&
            !User.IsInRole("Technician"))
        {
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(
                request.WorkNote))
        {
            return BadRequest(
                "Bilješka rada je obavezna.");
        }

        var intervention =
            await _context.Interventions
                .Include(intervention =>
                    intervention.InterventionStatus)
                .Include(intervention =>
                    intervention.WorkAssignment)
                    .ThenInclude(assignment =>
                        assignment!.FaultReport)
                .FirstOrDefaultAsync(
                    intervention =>
                        intervention.Id == id);

        if (intervention is null)
        {
            return NotFound(
                "Intervencija nije pronađena.");
        }

        var assignment =
            intervention.WorkAssignment;

        if (assignment is null)
        {
            return BadRequest(
                "Intervencija nema radni nalog.");
        }

        if (!User.IsInRole("Admin"))
        {
            var technicianIdValue =
                User.FindFirstValue(
                    AppClaimTypes.TechnicianId);

            if (!int.TryParse(
                    technicianIdValue,
                    out var technicianId) ||
                assignment.TechnicianId !=
                    technicianId)
            {
                return Forbid();
            }
        }

        if (!assignment.IsActive)
        {
            return BadRequest(
                "Mijenjati je moguće samo intervenciju aktivnog radnog naloga.");
        }

        if (intervention.FinishedAt.HasValue ||
            intervention.InterventionStatus?.Name !=
                "U tijeku")
        {
            return BadRequest(
                "Intervencija je već završena.");
        }

        if (!intervention.StartedAt.HasValue)
        {
            return BadRequest(
                "Završena intervencija mora imati vrijeme početka.");
        }

        if (assignment.FaultReport is null ||
            assignment.FaultReport.IsArchived)
        {
            return BadRequest(
                "Prijavu nije moguće ažurirati.");
        }

        var targetStatusName =
            request.IsSuccessful
                ? "Završena"
                : "Neuspješna";

        var targetStatus =
            await _context.InterventionStatuses
                .FirstOrDefaultAsync(status =>
                    status.Name ==
                        targetStatusName);

        if (targetStatus is null)
        {
            return Problem(
                $"Status {targetStatusName} nije pronađen.");
        }

        var now = DateTime.UtcNow;

        intervention.WorkNote =
            request.WorkNote.Trim();

        intervention.FinishedAt =
            now;

        intervention.InterventionStatusId =
            targetStatus.Id;

        intervention.InterventionStatus =
            targetStatus;

        if (request.IsSuccessful)
        {
            var resolvedStatus =
                await _context.FaultStatuses
                    .FirstOrDefaultAsync(status =>
                        status.Name ==
                            "Riješeno");

            if (resolvedStatus is null)
            {
                return Problem(
                    "Status Riješeno nije pronađen.");
            }

            assignment.FaultReport.FaultStatusId =
                resolvedStatus.Id;

            assignment.FaultReport.ResolvedAt =
                now;

            assignment.IsActive =
                false;

            assignment.EndedAt =
                now;
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    private IQueryable<Intervention> GetInterventionQuery()
    {
        return _context.Interventions
            .Include(intervention =>
                intervention.InterventionStatus)
            .Include(intervention =>
                intervention.WorkAssignment)
                .ThenInclude(assignment =>
                    assignment!.Technician)
            .Include(intervention =>
                intervention.WorkAssignment)
                .ThenInclude(assignment =>
                    assignment!.FaultReport)
                    .ThenInclude(report =>
                        report!.Location);
    }

    private bool CanViewAssignment(
        WorkAssignment? assignment)
    {
        if (assignment is null)
        {
            return false;
        }

        if (User.IsInRole("Admin") ||
            User.IsInRole("Manager"))
        {
            return true;
        }

        if (User.IsInRole("Technician"))
        {
            var technicianIdValue =
                User.FindFirstValue(
                    AppClaimTypes.TechnicianId);

            return
                int.TryParse(
                    technicianIdValue,
                    out var technicianId) &&
                assignment.TechnicianId ==
                    technicianId;
        }

        if (User.IsInRole("Reporter"))
        {
            var employeeIdValue =
                User.FindFirstValue(
                    AppClaimTypes.EmployeeId);

            return
                int.TryParse(
                    employeeIdValue,
                    out var employeeId) &&
                assignment.FaultReport?.ReporterId ==
                    employeeId;
        }

        return false;
    }

    private static InterventionDto ToDto(
        Intervention intervention)
    {
        var assignment =
            intervention.WorkAssignment;

        return new InterventionDto
        {
            Id =
                intervention.Id,

            CreatedAt =
                intervention.CreatedAt,

            StartedAt =
                intervention.StartedAt,

            FinishedAt =
                intervention.FinishedAt,

            WorkNote =
                intervention.WorkNote,

            WorkAssignmentId =
                intervention.WorkAssignmentId,

            FaultReportId =
                assignment?.FaultReportId ?? 0,

            FaultReportTitle =
                assignment?.FaultReport?.Title
                ?? string.Empty,

            LocationName =
                assignment?.FaultReport?.Location?.Name
                ?? string.Empty,

            TechnicianId =
                assignment?.TechnicianId ?? 0,

            TechnicianName =
                assignment?.Technician is null
                    ? string.Empty
                    : $"{assignment.Technician.FirstName} {assignment.Technician.LastName}",

            InterventionStatusId =
                intervention.InterventionStatusId,

            InterventionStatusName =
                intervention.InterventionStatus?.Name
                ?? string.Empty
        };
    }
}