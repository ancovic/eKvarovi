namespace eKvarovi.Shared.Models;

public class Intervention
{
    public int Id { get; set; }

    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }

    public string? WorkNote { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int WorkAssignmentId { get; set; }
    public WorkAssignment? WorkAssignment { get; set; }

    public int InterventionStatusId { get; set; }
    public InterventionStatus? InterventionStatus { get; set; }

    public ICollection<InterventionMaterial> Materials { get; set; }
        = new List<InterventionMaterial>();
    public ICollection<FaultMedia> Media { get; set; }
        = new List<FaultMedia>();
}