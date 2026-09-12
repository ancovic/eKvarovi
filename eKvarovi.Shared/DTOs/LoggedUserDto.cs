namespace eKvarovi.Shared.DTOs;

public class LoggedUserDto
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;

    public List<string> Roles { get; set; } = new();

    public int? EmployeeId { get; set; }
    public int? TechnicianId { get; set; }
}