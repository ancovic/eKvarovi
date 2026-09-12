namespace eKvarovi.Shared.DTOs;

public class LocationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? City { get; set; }
    public bool IsActive { get; set; }

    public int LocationTypeId { get; set; }
    public string LocationTypeName { get; set; } = string.Empty;
}