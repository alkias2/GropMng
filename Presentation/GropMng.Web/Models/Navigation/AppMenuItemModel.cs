namespace GropMng.Web.Models.Navigation;

/// <summary>
/// Defines supported sidebar menu item kinds.
/// </summary>
public enum AppMenuItemType
{
    /// <summary>
    /// Clickable menu link.
    /// </summary>
    Link = 0,

    /// <summary>
    /// Non-clickable section header.
    /// </summary>
    Header = 1
}

/// <summary>
/// Represents a menu item used by the application sidebar navigation.
/// </summary>
public class AppMenuItemModel
{
    /// <summary>
    /// Gets or sets the stable key of the menu item.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the stable system name used to identify the active admin menu item.
    /// </summary>
    public string SystemName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the menu item type.
    /// </summary>
    public AppMenuItemType ItemType { get; set; } = AppMenuItemType.Link;

    /// <summary>
    /// Gets or sets the menu display title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the BoxIcons class used by the menu item.
    /// </summary>
    public string? IconClass { get; set; }

    /// <summary>
    /// Gets or sets the MVC area value.
    /// </summary>
    public string? Area { get; set; }

    /// <summary>
    /// Gets or sets the MVC controller value.
    /// </summary>
    public string? Controller { get; set; }

    /// <summary>
    /// Gets or sets the MVC action value.
    /// </summary>
    public string? Action { get; set; }

    /// <summary>
    /// Gets or sets child menu items.
    /// </summary>
    public IList<AppMenuItemModel> Children { get; set; } = new List<AppMenuItemModel>();

    /// <summary>
    /// Gets or sets a value indicating whether the current item is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the current item should be rendered open.
    /// </summary>
    public bool IsOpen { get; set; }
}