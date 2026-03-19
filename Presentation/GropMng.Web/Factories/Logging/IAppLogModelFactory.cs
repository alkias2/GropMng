using GropMng.Web.Areas.Admin.Models.Logging;

namespace GropMng.Web.Factories.Logging;

/// <summary>
/// Prepares view models for the AppLog administration screens.
/// </summary>
public interface IAppLogModelFactory
{
    /// <summary>
    /// Initialises or resets a search model for the initial GET page load.
    /// Sets sensible default paging and returns the same instance.
    /// </summary>
    AppLogSearchModel PrepareSearchModel(AppLogSearchModel? searchModel = null);

    /// <summary>
    /// Executes the filtered, paged query and returns a DataTables-compatible list model.
    /// </summary>
    Task<AppLogListModel> PrepareListModelAsync(AppLogSearchModel searchModel);
}
