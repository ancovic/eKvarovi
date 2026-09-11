namespace eKvarovi.Shared.Models;

public class Location
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? City { get; set; }

    public bool IsActive { get; set; } = true;

    public int LocationTypeId { get; set; }
    public LocationType? LocationType { get; set; }

    public ICollection<Employee> Employees { get; set; }
        = new List<Employee>();

    public ICollection<FaultReport> FaultReports { get; set; }
        = new List<FaultReport>();
}