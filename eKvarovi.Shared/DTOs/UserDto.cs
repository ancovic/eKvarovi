namespace eKvarovi.Shared.DTOs;

public class UserDto
{
    public int Id { get; set; }

    public string Email { get; set; } =
        string.Empty;

    public string DisplayName { get; set; } =
        string.Empty;

    public bool IsActive { get; set; }

    public int? EmployeeId { get; set; }

    public int? TechnicianId { get; set; }

    public List<int> RoleIds { get; set; } =
        new();

    public List<string> Roles { get; set; } =
        new();
}