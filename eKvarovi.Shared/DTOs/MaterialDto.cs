namespace eKvarovi.Shared.DTOs;

public class MaterialDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public int MaterialUnitId { get; set; }
    public string MaterialUnitName { get; set; } = string.Empty;
}