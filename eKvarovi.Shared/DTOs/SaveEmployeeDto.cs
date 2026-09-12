namespace eKvarovi.Shared.DTOs;

public class SaveEmployeeDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }

    public bool IsActive { get; set; } = true;

    public int LocationId { get; set; }
}