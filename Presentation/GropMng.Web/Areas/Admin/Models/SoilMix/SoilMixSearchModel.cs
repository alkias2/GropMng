using GropMng.Web.Framework.Models;
using GropMng.Web.Framework.Models.DataTables;

namespace GropMng.Web.Areas.Admin.Models.SoilMix;

/// <summary>
/// Search and filter parameters for the SoilMix DataTables grid.
/// </summary>
public class SoilMixSearchModel : BaseSearchModel
{
    public GropDataTablesModel GridModel { get; set; } = new();

    public string? SearchTerm { get; set; }
}
