namespace eKvarovi.Shared.Models;

public class FaultReport
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? DueDate { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    public bool IsArchived { get; set; }
    public DateTime? ArchivedAt { get; set; }

    public int LocationId { get; set; }
    public Location? Location { get; set; }

    public int ReporterId { get; set; }
    public Employee? Reporter { get; set; }

    public int FaultStatusId { get; set; }
    public FaultStatus? FaultStatus { get; set; }

    public int? FaultTypeId { get; set; }
    public FaultType? FaultType { get; set; }

    public int? FaultPriorityId { get; set; }
    public FaultPriority? FaultPriority { get; set; }

    public ICollection<WorkAssignment> Assignments { get; set; }
        = new List<WorkAssignment>();

    public ICollection<FaultMedia> Media { get; set; }
        = new List<FaultMedia>();
}