using GropMng.Web.Framework.Models;
using GropMng.Web.Framework.Models.DataTables;

namespace GropMng.Web.Areas.Admin.Models.Localization;

/// <summary>
/// Search and filter parameters for the LocaleResource DataTables grid.
/// Extends <see cref="BaseSearchModel"/> to inherit DataTables paging (draw/start/length).
/// </summary>
public class LocaleResourceSearchModel : BaseSearchModel
{
    public LocaleResourceSearchModel()
    {
    }

    /// <summary>
    /// The fully configured DataTables model, populated by the factory.
    /// </summary>
    public GropDataTablesModel GridModel { get; set; } = new();

    /// <summary>
    /// The language ID for which to retrieve resources.
    /// </summary>
    public int LanguageId { get; set; }

    /// <summary>
    /// Optional resource name filter (case-insensitive substring match).
    /// </summary>
    public string ResourceName { get; set; } = string.Empty;
}
