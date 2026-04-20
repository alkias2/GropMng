using GropMng.Web.Areas.Admin.Models.Logging;

namespace GropMng.Web.Areas.Admin.Factories.Logging;

/// <summary>
/// Prepares view models for the AppLog administration screens.
/// </summary>
public interface IAppLogModelFactory
{
    /// <summary>
    /// Initialises a search model for the initial GET page load.
    /// Populates filter lists and applies sensible default paging.
    /// </summary>
    Task<AppLogSearchModel> PrepareSearchModelAsync(AppLogSearchModel? searchModel = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes the filtered, paged query and returns a DataTables-compatible list model.
    /// </summary>
    Task<AppLogListModel> PrepareListModelAsync(AppLogSearchModel searchModel);
}
