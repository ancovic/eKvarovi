namespace eKvarovi.Shared.DTOs;

public class SaveWorkAssignmentDto
{
    public int FaultReportId { get; set; }
    public int TechnicianId { get; set; }

    public string? Note { get; set; }
}