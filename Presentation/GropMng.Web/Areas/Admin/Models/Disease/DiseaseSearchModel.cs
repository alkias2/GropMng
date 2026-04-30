using GropMng.Web.Framework.Models;
using GropMng.Web.Framework.Models.DataTables;

namespace GropMng.Web.Areas.Admin.Models.Disease;

/// <summary>
/// Represents the search/filter model for the disease list DataTable.
/// </summary>
public class DiseaseSearchModel : BaseSearchModel
{
    public string? SearchTerm { get; set; }

    public GropDataTablesModel GridModel { get; set; } = new();
}
