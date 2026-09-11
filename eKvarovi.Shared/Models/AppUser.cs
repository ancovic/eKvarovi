namespace eKvarovi.Shared.Models;

public class AppUser
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public int? EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public int? TechnicianId { get; set; }
    public Technician? Technician { get; set; }

    public ICollection<AppUserRole> UserRoles { get; set; }
        = new List<AppUserRole>();
}