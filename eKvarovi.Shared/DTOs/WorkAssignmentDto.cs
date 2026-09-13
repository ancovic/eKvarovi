namespace eKvarovi.Shared.DTOs;

public class WorkAssignmentDto
{
    public int Id { get; set; }

    public DateTime AssignedAt { get; set; }
    public DateTime? EndedAt { get; set; }

    public bool IsActive { get; set; }

    public string? Note { get; set; }

    public int FaultReportId { get; set; }
    public string FaultReportTitle { get; set; } = string.Empty;

    public int LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;

    public int FaultStatusId { get; set; }
    public string FaultStatusName { get; set; } = string.Empty;

    public int TechnicianId { get; set; }
    public string TechnicianName { get; set; } = string.Empty;
}