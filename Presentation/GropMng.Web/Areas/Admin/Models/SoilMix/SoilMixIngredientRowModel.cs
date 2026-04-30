namespace GropMng.Web.Areas.Admin.Models.SoilMix;

/// <summary>
/// DataTables row model for SoilMix ingredient rows.
/// </summary>
public class SoilMixIngredientRowModel
{
    public int Id { get; set; }

    public int SoilMixId { get; set; }

    public string SoilIngredientName { get; set; } = string.Empty;

    public decimal PercentageByVolume { get; set; }

    public string? Notes { get; set; }
}
