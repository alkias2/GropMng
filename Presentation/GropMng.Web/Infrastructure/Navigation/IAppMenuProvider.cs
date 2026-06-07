using GropMng.Web.Models.Navigation;

namespace GropMng.Web.Infrastructure.Navigation;

/// <summary>
/// Provides the sidebar menu definition used by the web application.
/// </summary>
public interface IAppMenuProvider
{
    /// <summary>
    /// Builds the menu tree.
    /// </summary>
    /// <returns>A list of root menu items.</returns>
    Task<IList<AppMenuItemModel>> BuildAsync(CancellationToken cancellationToken = default);
}