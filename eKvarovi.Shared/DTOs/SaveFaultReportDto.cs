namespace eKvarovi.Shared.DTOs;

public class SaveFaultReportDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public int LocationId { get; set; }
    public int ReporterId { get; set; }

    public int? FaultTypeId { get; set; }
    public int? FaultPriorityId { get; set; }

    public DateTime? DueDate { get; set; }
}