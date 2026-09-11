namespace eKvarovi.Shared.Models;

public class Material
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public int MaterialUnitId { get; set; }
    public MaterialUnit? MaterialUnit { get; set; }

    public ICollection<InterventionMaterial> Interventions { get; set; }
        = new List<InterventionMaterial>();
}