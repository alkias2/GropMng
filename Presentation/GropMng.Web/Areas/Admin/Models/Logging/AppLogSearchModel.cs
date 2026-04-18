using GropMng.Web.Framework.Models;
using GropMng.Web.Framework.Models.DataTables;
using AppLogLevel = GropMng.Core.Domain.Logging.LogLevel;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GropMng.Web.Areas.Admin.Models.Logging;

/// <summary>
/// Search and filter parameters for the AppLog DataTables grid.
/// Extends <see cref="BaseSearchModel"/> to inherit DataTables paging (draw/start/length).
/// </summary>
public class AppLogSearchModel : BaseSearchModel
{
    public AppLogSearchModel()
    {
        AvailableLogLevels = new List<SelectListItem>();
    }

    /// <summary>
    /// The fully configured DataTables model, populated by the factory.
    /// </summary>
    public GropDataTablesModel GridModel { get; set; } = new();

    /// <summary>Optional log level filter.</summary>
    public AppLogLevel? Level { get; set; }

    /// <summary>Optional free-text message filter.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Optional start-of-range date filter (UTC, inclusive).</summary>
    public DateTime? FromDate { get; set; }

    /// <summary>Optional end-of-range date filter (UTC, inclusive).</summary>
    public DateTime? ToDate { get; set; }

    /// <summary>Localized and prebuilt list items for the level filter dropdown.</summary>
    public IList<SelectListItem> AvailableLogLevels { get; set; }
}
