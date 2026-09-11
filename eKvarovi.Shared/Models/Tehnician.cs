namespace eKvarovi.Shared.Models;

public class Technician
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Specialization { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<WorkAssignment> WorkAssignments { get; set; }
        = new List<WorkAssignment>();
}