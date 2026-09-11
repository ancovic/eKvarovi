namespace eKvarovi.Shared.Models;

public class MaterialUnit
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Material> Materials { get; set; }
        = new List<Material>();
}