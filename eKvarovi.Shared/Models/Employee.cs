namespace eKvarovi.Shared.Models;

public class Employee
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }

    public bool IsActive { get; set; } = true;

    public int LocationId { get; set; }
    public Location? Location { get; set; }

    public ICollection<FaultReport> ReportedFaults { get; set; }
        = new List<FaultReport>();
}