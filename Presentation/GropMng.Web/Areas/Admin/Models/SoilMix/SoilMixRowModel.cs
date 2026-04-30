namespace GropMng.Web.Areas.Admin.Models.SoilMix;

/// <summary>
/// DataTables row model for SoilMix list pages.
/// </summary>
public class SoilMixRowModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? TextureLocalized { get; set; }

    public string? DrainageLocalized { get; set; }

    public string? PhRange { get; set; }
}
