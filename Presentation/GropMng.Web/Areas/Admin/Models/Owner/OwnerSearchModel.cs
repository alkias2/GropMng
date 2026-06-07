using GropMng.Web.Framework.Models;
using GropMng.Web.Framework.Models.DataTables;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GropMng.Web.Areas.Admin.Models.Owner;

/// <summary>
/// Search parameters for the owner administration grid.
/// </summary>
public class OwnerSearchModel : BaseSearchModel
{
    public GropDataTablesModel GridModel { get; set; } = new();

    public string? SearchTerm { get; set; }

    public string? Status { get; set; }

    public IList<SelectListItem> AvailableStatuses { get; set; } = new List<SelectListItem>();
}
