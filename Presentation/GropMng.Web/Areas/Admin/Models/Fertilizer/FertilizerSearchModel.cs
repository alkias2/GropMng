using GropMng.Web.Framework.Models;
using GropMng.Web.Framework.Models.DataTables;

namespace GropMng.Web.Areas.Admin.Models.Fertilizer;

/// <summary>
/// Search and filter parameters for the Fertilizer DataTables grid.
/// </summary>
public class FertilizerSearchModel : BaseSearchModel
{
    public GropDataTablesModel GridModel { get; set; } = new();

    public string? SearchTerm { get; set; }
}
