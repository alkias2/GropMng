using GropMng.Web.Framework.Models;
using GropMng.Web.Framework.Models.DataTables;

namespace GropMng.Web.Areas.Admin.Models.Localization;

/// <summary>
/// Search and filter parameters for the Language DataTables grid.
/// Extends <see cref="BaseSearchModel"/> to inherit DataTables paging (draw/start/length).
/// </summary>
public class LanguageSearchModel : BaseSearchModel
{
    public LanguageSearchModel()
    {
    }

    /// <summary>
    /// The fully configured DataTables model, populated by the factory.
    /// </summary>
    public GropDataTablesModel GridModel { get; set; } = new();

    /// <summary>
    /// Optional language name filter (case-insensitive substring match).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional published status filter (true/false/null for all).
    /// </summary>
    public bool? Published { get; set; }
}
