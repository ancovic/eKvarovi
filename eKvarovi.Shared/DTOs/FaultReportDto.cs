namespace eKvarovi.Shared.DTOs;

public class FaultReportDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    public bool IsArchived { get; set; }
    public DateTime? ArchivedAt { get; set; }

    public int LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;

    public int ReporterId { get; set; }
    public string ReporterName { get; set; } = string.Empty;

    public int FaultStatusId { get; set; }
    public string FaultStatusName { get; set; } = string.Empty;

    public int? FaultTypeId { get; set; }
    public string? FaultTypeName { get; set; }

    public int? FaultPriorityId { get; set; }
    public string? FaultPriorityName { get; set; }
}