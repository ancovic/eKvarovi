namespace eKvarovi.Shared.DTOs;

public class DashboardDto
{
    public int? OpenFaultReportsCount { get; set; }

    public int? CriticalFaultReportsCount { get; set; }

    public int? OverdueFaultReportsCount { get; set; }

    public int? UnassignedFaultReportsCount { get; set; }

    public int? ActiveInterventionsCount { get; set; }

    public double? AverageResolutionHours { get; set; }

    public List<DashboardFaultReportDto> LatestFaultReports { get; set; }
        = new();

    public int? MyFaultReportsCount { get; set; }

    public int? MyWorkAssignmentsCount { get; set; }
}

public class DashboardFaultReportDto
{
    public int Id { get; set; }

    public string Title { get; set; } =
        string.Empty;

    public string LocationName { get; set; } =
        string.Empty;

    public string FaultStatusName { get; set; } =
        string.Empty;

    public DateTime CreatedAt { get; set; }
}