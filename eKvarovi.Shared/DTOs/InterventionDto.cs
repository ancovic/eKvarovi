namespace eKvarovi.Shared.DTOs;

public class InterventionDto
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }

    public string? WorkNote { get; set; }

    public int WorkAssignmentId { get; set; }

    public int FaultReportId { get; set; }
    public string FaultReportTitle { get; set; } = string.Empty;

    public string LocationName { get; set; } = string.Empty;

    public int TechnicianId { get; set; }
    public string TechnicianName { get; set; } = string.Empty;

    public int InterventionStatusId { get; set; }
    public string InterventionStatusName { get; set; } = string.Empty;
}