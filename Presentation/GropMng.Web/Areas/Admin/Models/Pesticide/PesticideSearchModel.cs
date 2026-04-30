using GropMng.Web.Framework.Models;
using GropMng.Web.Framework.Models.DataTables;

namespace GropMng.Web.Areas.Admin.Models.Pesticide;

/// <summary>
/// Search and filter parameters for the Pesticide DataTables grid.
/// </summary>
public class PesticideSearchModel : BaseSearchModel
{
    public GropDataTablesModel GridModel { get; set; } = new();

    public string? SearchTerm { get; set; }
}
