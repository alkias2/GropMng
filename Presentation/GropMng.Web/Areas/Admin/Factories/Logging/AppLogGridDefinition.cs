using GropMng.Web.Areas.Admin.Models.Logging;
using GropMng.Web.Framework.Models.DataTables;

namespace GropMng.Web.Areas.Admin.Factories.Logging;

/// <summary>
/// Provides declarative DataTables configuration for the AppLog admin grid.
/// </summary>
public static class AppLogGridDefinition
{
    #region Public

    /// <summary>
    /// Builds the <see cref="GropDataTablesModel"/> for the AppLog index grid.
    /// </summary>
    /// <param name="searchModel">The search model with paging defaults.</param>
    /// <param name="titles">The localized table header titles.</param>
    /// <returns>A fully configured <see cref="GropDataTablesModel"/> instance.</returns>
    public static GropDataTablesModel Build(AppLogSearchModel searchModel, AppLogGridColumnTitles titles)
    {
        ArgumentNullException.ThrowIfNull(searchModel);
        ArgumentNullException.ThrowIfNull(titles);

        return new GropDataTablesModel
        {
            Name = "appLogsTable",
            UrlRead = new GropDataUrl("AppLogList", "AppLog"),
            SearchButtonId = "btnApplyFilters",
            Length = searchModel.PageSize,
            LengthMenu = [10, 25, 50, 100],
            ShowSearch = false,
            Filters =
            [
                new GropFilterParameter("level")
                {
                    ElementId = "levelFilter",
                },
                new GropFilterParameter("fromDate")
                {
                    ElementId = "fromDateFilter",
                },
                new GropFilterParameter("toDate")
                {
                    ElementId = "toDateFilter",
                },
            ],
            ColumnCollection =
            [
                new GropColumnProperty("id")
                {
                    Title = titles.Select,
                    Width = "44px",
                    ClassName = "text-center",
                    Orderable = false,
                    Searchable = false,
                    Render = new RenderCustom("window.GropAppLogGridRenderers.renderRowSelector"),
                },
                new GropColumnProperty("id")
                {
                    Title = titles.Id,
                    Width = "60px",
                },
                new GropColumnProperty("level")
                {
                    Title = titles.Level,
                    Width = "110px",
                    Render = new RenderCustom("window.GropAppLogGridRenderers.renderLevel"),
                },
                new GropColumnProperty("category")
                {
                    Title = titles.Category,
                    Width = "200px",
                    ClassName = "text-truncate",
                },
                new GropColumnProperty("message")
                {
                    Title = titles.Message,
                    Width = "300px",
                    ClassName = "text-truncate",
                },
                new GropColumnProperty("timestamp")
                {
                    Title = titles.Timestamp,
                    Width = "160px",
                    Render = new RenderCustom("window.GropAppLogGridRenderers.renderTimestamp"),
                },
                new GropColumnProperty("id")
                {
                    Title = titles.Actions,
                    Width = "120px",
                    ClassName = "text-center",
                    Orderable = false,
                    Searchable = false,
                    Render = new RenderCustom("window.GropAppLogGridRenderers.renderActions"),
                },
            ],
        };
    }

    #endregion
}

/// <summary>
/// Represents localized table header titles for the AppLog grid.
/// </summary>
public sealed class AppLogGridColumnTitles
{
    /// <summary>
    /// Gets or sets the title for the Select column.
    /// </summary>
    public string Select { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title for the Id column.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title for the Level column.
    /// </summary>
    public string Level { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title for the Category column.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title for the Message column.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title for the Timestamp column.
    /// </summary>
    public string Timestamp { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title for the Actions column.
    /// </summary>
    public string Actions { get; set; } = string.Empty;
}