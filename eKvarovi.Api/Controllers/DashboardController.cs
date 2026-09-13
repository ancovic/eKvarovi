using eKvarovi.Api.Data;
using eKvarovi.Api.Security;
using eKvarovi.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eKvarovi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly EKvaroviDbContext _context;

    public DashboardController(
        EKvaroviDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardDto>> GetDashboard()
    {
        var result =
            new DashboardDto();

        var canViewOperationalDashboard =
            User.IsInRole("Admin") ||
            User.IsInRole("Manager");

        if (canViewOperationalDashboard)
        {
            result.OpenFaultReportsCount =
                await _context.FaultReports
                    .CountAsync(report =>
                        !report.IsArchived &&
                        report.FaultStatus != null &&
                        report.FaultStatus.Name !=
                            "Zatvoreno");

            result.CriticalFaultReportsCount =
                await _context.FaultReports
                    .CountAsync(report =>
                        !report.IsArchived &&
                        report.FaultPriority != null &&
                        report.FaultPriority.Name ==
                            "Kritičan" &&
                        report.FaultStatus != null &&
                        report.FaultStatus.Name !=
                            "Zatvoreno");

            var today =
                DateTime.UtcNow.Date;

            result.OverdueFaultReportsCount =
                await _context.FaultReports
                    .CountAsync(report =>
                        !report.IsArchived &&
                        report.DueDate.HasValue &&
                        report.DueDate.Value <
                            today &&
                        report.FaultStatus != null &&
                        report.FaultStatus.Name !=
                            "Riješeno" &&
                        report.FaultStatus.Name !=
                            "Zatvoreno");

            result.UnassignedFaultReportsCount =
                await _context.FaultReports
                    .CountAsync(report =>
                        !report.IsArchived &&
                        report.FaultStatus != null &&
                        report.FaultStatus.Name !=
                            "Riješeno" &&
                        report.FaultStatus.Name !=
                            "Zatvoreno" &&
                        !report.Assignments.Any(
                            assignment =>
                                assignment.IsActive));

            result.ActiveInterventionsCount =
                await _context.Interventions
                    .CountAsync(intervention =>
                        intervention.StartedAt.HasValue &&
                        !intervention.FinishedAt.HasValue);

            var closedReports =
                await _context.FaultReports
                    .Where(report =>
                        report.ClosedAt.HasValue)
                    .Select(report =>
                        new
                        {
                            report.CreatedAt,
                            report.ClosedAt
                        })
                    .ToListAsync();

            if (closedReports.Count > 0)
            {
                result.AverageResolutionHours =
                    closedReports.Average(report =>
                        (report.ClosedAt!.Value -
                         report.CreatedAt)
                        .TotalHours);
            }

            result.LatestFaultReports =
                await _context.FaultReports
                    .Where(report =>
                        !report.IsArchived)
                    .OrderByDescending(report =>
                        report.CreatedAt)
                    .Take(5)
                    .Select(report =>
                        new DashboardFaultReportDto
                        {
                            Id =
                                report.Id,

                            Title =
                                report.Title,

                            LocationName =
                                report.Location != null
                                    ? report.Location.Name
                                    : string.Empty,

                            FaultStatusName =
                                report.FaultStatus != null
                                    ? report.FaultStatus.Name
                                    : string.Empty,

                            CreatedAt =
                                report.CreatedAt
                        })
                    .ToListAsync();
        }

        if (User.IsInRole("Reporter"))
        {
            var employeeClaim =
                User.FindFirst(
                    AppClaimTypes.EmployeeId)
                    ?.Value;

            if (int.TryParse(
                    employeeClaim,
                    out var employeeId))
            {
                result.MyFaultReportsCount =
                    await _context.FaultReports
                        .CountAsync(report =>
                            report.ReporterId ==
                                employeeId);
            }
        }

        if (User.IsInRole("Technician"))
        {
            var technicianClaim =
                User.FindFirst(
                    AppClaimTypes.TechnicianId)
                    ?.Value;

            if (int.TryParse(
                    technicianClaim,
                    out var technicianId))
            {
                result.MyWorkAssignmentsCount =
                    await _context.WorkAssignments
                        .CountAsync(assignment =>
                            assignment.TechnicianId ==
                                technicianId);
            }
        }

        return Ok(result);
    }
}