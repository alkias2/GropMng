namespace GropMng.Web.Areas.Admin.Models.Plant;

/// <summary>
/// Represents a single row in the Plant DataTables grid.
/// </summary>
public class PlantRowModel
{
    public int Id { get; set; }
    public string CommonName { get; set; } = string.Empty;
    public string ScientificName { get; set; } = string.Empty;
    public string? Family { get; set; }
    public string Category { get; set; } = string.Empty;
    public string CategoryLocalized { get; set; } = string.Empty;
    public string FlagsSummary { get; set; } = string.Empty;
    public bool IsEdible { get; set; }
    public bool IsMedicinal { get; set; }
    public bool IsToxic { get; set; }
}
