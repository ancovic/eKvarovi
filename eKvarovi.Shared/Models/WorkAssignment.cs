namespace eKvarovi.Shared.Models;

public class WorkAssignment
{
    public int Id { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }

    public bool IsActive { get; set; } = true;

    public string? Note { get; set; }

    public int FaultReportId { get; set; }
    public FaultReport? FaultReport { get; set; }

    public int TechnicianId { get; set; }
    public Technician? Technician { get; set; }

    public ICollection<Intervention> Interventions { get; set; }
        = new List<Intervention>();
}