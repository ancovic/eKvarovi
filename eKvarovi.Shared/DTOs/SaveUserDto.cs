namespace eKvarovi.Shared.DTOs;

public class SaveUserDto
{
    public string Email { get; set; } =
        string.Empty;

    public string DisplayName { get; set; } =
        string.Empty;

    public string? Password { get; set; }

    public bool IsActive { get; set; } =
        true;

    public int? EmployeeId { get; set; }

    public int? TechnicianId { get; set; }

    public List<int> RoleIds { get; set; } =
        new();
}