namespace GropMng.Web.Areas.Admin.Models.Pesticide;

/// <summary>
/// Represents a single row in the Pesticide DataTables grid.
/// </summary>
public class PesticideRowModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? ActiveIngredient { get; set; }
    public string? PesticideTypeLocalized { get; set; }
    public string? ApplicationMethodLocalized { get; set; }
    public bool IsOrganic { get; set; }
}
