using GropMng.Core.Domain.Garden.Enums;
using GropMng.Web.Framework.Models;
using GropMng.Web.Framework.Models.DataTables;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GropMng.Web.Areas.Admin.Models.Plant;

/// <summary>
/// Search and filter parameters for the Plant DataTables grid.
/// </summary>
public class PlantSearchModel : BaseSearchModel
{
    /// <summary>
    /// The fully configured DataTables model, populated by the factory.
    /// </summary>
    public GropDataTablesModel GridModel { get; set; } = new();

    /// <summary>
    /// Localized category options for the filter drop-down. Populated by the factory.
    /// </summary>
    public IList<SelectListItem> AvailableCategories { get; set; } = new List<SelectListItem>();

    /// <summary>
    /// Optional free-text search applied to common name, scientific name and family.
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Optional plant category filter.
    /// </summary>
    public PlantCategory? Category { get; set; }
}
