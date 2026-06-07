namespace GropMng.Web.Areas.Admin.Models.Fertilizer;

/// <summary>
/// Represents a single row in the Fertilizer DataTables grid.
/// </summary>
public class FertilizerRowModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? FertilizerTypeLocalized { get; set; }
    public string? NpkRatio { get; set; }
    public string? ApplicationMethodLocalized { get; set; }
    public bool IsOrganic { get; set; }
}
