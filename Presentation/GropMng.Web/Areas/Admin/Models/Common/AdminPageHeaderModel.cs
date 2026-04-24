namespace GropMng.Web.Areas.Admin.Models.Common;

/// <summary>
/// Shared page header model used by admin pages for title, breadcrumbs and optional actions.
/// </summary>
public class AdminPageHeaderModel
{
    /// <summary>
    /// Gets or sets the page title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets breadcrumb items.
    /// </summary>
    public IList<AdminBreadcrumbItemModel> Breadcrumbs { get; set; } = new List<AdminBreadcrumbItemModel>();

    /// <summary>
    /// Gets or sets optional action buttons HTML rendered to the right of the page title.
    /// Use <see cref="Microsoft.AspNetCore.Html.HtmlString"/> to wrap pre-built HTML.
    /// </summary>
    public Microsoft.AspNetCore.Html.IHtmlContent? ActionButtons { get; set; }
}

/// <summary>
/// Single breadcrumb item.
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