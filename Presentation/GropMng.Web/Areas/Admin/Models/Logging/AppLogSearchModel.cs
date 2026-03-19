using GropMng.Web.Framework.Models;

namespace GropMng.Web.Areas.Admin.Models.Logging;

/// <summary>
/// Search and filter parameters for the AppLog DataTables grid.
/// Extends <see cref="BaseSearchModel"/> to inherit DataTables paging (draw/start/length).
/// </summary>
public class AppLogSearchModel : BaseSearchModel
{
    /// <summary>Optional log level filter (e.g. "Information", "Warning", "Error", "Critical").</summary>
    public string? Level { get; set; }

    /// <summary>Optional start-of-range date filter (UTC, inclusive).</summary>
    public DateTime? FromDate { get; set; }

    /// <summary>Optional end-of-range date filter (UTC, inclusive).</summary>
    public DateTime? ToDate { get; set; }
}
