namespace eKvarovi.Shared.Models;

public class InterventionMaterial
{
    public decimal Quantity { get; set; }

    public int InterventionId { get; set; }
    public Intervention? Intervention { get; set; }

    public int MaterialId { get; set; }
    public Material? Material { get; set; }
}