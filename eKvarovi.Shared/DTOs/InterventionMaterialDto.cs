namespace eKvarovi.Shared.DTOs;

public class InterventionMaterialDto
{
    public int InterventionId { get; set; }

    public int MaterialId { get; set; }
    public string MaterialName { get; set; } = string.Empty;

    public decimal Quantity { get; set; }

    public string MaterialUnitName { get; set; } = string.Empty;
}