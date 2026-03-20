using GropMng.Core.Domain.Garden.Enums;
using GropMng.Web.Framework.Models;

namespace GropMng.Web.Areas.Admin.Models.Plant;

/// <summary>
/// Search and filter parameters for the Plant DataTables grid.
/// </summary>
public class PlantSearchModel : BaseSearchModel
{
    /// <summary>
    /// Optional free-text search applied to common name, scientific name and family.
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Optional plant category filter.
    /// </summary>
    public PlantCategory? Category { get; set; }
}
