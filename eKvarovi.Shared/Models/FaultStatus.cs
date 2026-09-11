namespace eKvarovi.Shared.Models;

public class FaultStatus
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<FaultReport> FaultReports { get; set; }
        = new List<FaultReport>();
}