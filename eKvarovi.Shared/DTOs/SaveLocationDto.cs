namespace eKvarovi.Shared.DTOs;

public class SaveLocationDto
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? City { get; set; }
    public bool IsActive { get; set; } = true;

    public int LocationTypeId { get; set; }
}