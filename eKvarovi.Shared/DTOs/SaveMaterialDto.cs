namespace eKvarovi.Shared.DTOs;

public class SaveMaterialDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public int MaterialUnitId { get; set; }
}