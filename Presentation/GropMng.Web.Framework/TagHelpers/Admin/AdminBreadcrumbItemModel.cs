namespace GropMng.Web.Framework.TagHelpers.Admin;

/// <summary>
/// Single breadcrumb item for admin page header rendering.
/// </summary>
public class AdminBreadcrumbItemModel
{
    /// <summary>
    /// Gets or sets breadcrumb text.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets optional URL. When empty, item is rendered as active.
    /// </summary>
    public string? Url { get; set; }
}
