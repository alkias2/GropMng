using GropMng.Web.Models.Navigation;

namespace GropMng.Web.Infrastructure.Navigation;

/// <summary>
/// Represents one declarative node loaded from the application menu sitemap.
/// </summary>
public class AppMenuSiteMapNode
{
    /// <summary>
    /// Gets or sets the stable system name.
    /// </summary>
    public string SystemName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional localization resource key used to resolve the title.
    /// </summary>
    public string ResourceKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the menu item type.
    /// </summary>
    public AppMenuItemType ItemType { get; set; } = AppMenuItemType.Link;

    /// <summary>
    /// Gets or sets the optional icon CSS class.
    /// </summary>
    public string? IconClass { get; set; }

    /// <summary>
    /// Gets or sets the MVC area.
    /// </summary>
    public string? Area { get; set; }

    /// <summary>
    /// Gets or sets the MVC controller.
    /// </summary>
    public string? Controller { get; set; }

    /// <summary>
    /// Gets or sets the MVC action.
    /// </summary>
    public string? Action { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the node is administrator-only.
    /// </summary>
    public bool RequiresAdministrator { get; set; }

    /// <summary>
    /// Gets or sets the optional permission system names that control node visibility.
    /// </summary>
    public IList<string> PermissionNames { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets a value indicating whether the node should be visible after sitemap resolution.
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Gets or sets the child sitemap nodes.
    /// </summary>
    public IList<AppMenuSiteMapNode> Children { get; set; } = new List<AppMenuSiteMapNode>();
}