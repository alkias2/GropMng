namespace GropMng.Web.Infrastructure.Navigation;

/// <summary>
/// Loads the declarative application menu sitemap.
/// </summary>
public interface IAppMenuSiteMap
{
    /// <summary>
    /// Loads the root sitemap node.
    /// </summary>
    /// <returns>The root node of the sitemap.</returns>
    Task<AppMenuSiteMapNode> LoadAsync(CancellationToken cancellationToken = default);
}