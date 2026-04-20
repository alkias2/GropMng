using GropMng.Web.Infrastructure.Navigation;
using GropMng.Web.Models.Navigation;
using GropMng.Web.Framework.UI;
using Microsoft.AspNetCore.Mvc;

namespace GropMng.Web.ViewComponents;

/// <summary>
/// Renders the application sidebar menu and marks active/open items based on the selected menu system name.
/// </summary>
public class AppMenuViewComponent : ViewComponent
{
    private readonly IAppMenuProvider _menuProvider;
    private readonly IGropHtmlHelper _gropHtmlHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppMenuViewComponent"/> class.
    /// </summary>
    /// <param name="menuProvider">The menu provider.</param>
    /// <param name="gropHtmlHelper">The request-scoped UI helper.</param>
    public AppMenuViewComponent(IAppMenuProvider menuProvider, IGropHtmlHelper gropHtmlHelper)
    {
        _menuProvider = menuProvider;
        _gropHtmlHelper = gropHtmlHelper;
    }

    /// <summary>
    /// Builds and returns the sidebar menu tree for the current request.
    /// </summary>
    /// <returns>A view result containing a route-aware menu tree.</returns>
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var activeSystemName = _gropHtmlHelper.GetActiveMenuItemSystemName();
        var currentArea = (ViewContext.RouteData.Values["area"]?.ToString() ?? string.Empty).Trim();
        var currentController = (ViewContext.RouteData.Values["controller"]?.ToString() ?? string.Empty).Trim();
        var currentAction = (ViewContext.RouteData.Values["action"]?.ToString() ?? string.Empty).Trim();

        var menu = await _menuProvider.BuildAsync(HttpContext.RequestAborted);
        MarkState(menu, activeSystemName, currentArea, currentController, currentAction);

        return View(menu);
    }

    private static bool MarkState(IEnumerable<AppMenuItemModel> items, string activeSystemName, string area, string controller, string action)
    {
        var anyActive = false;

        foreach (var item in items)
        {
            if (item.ItemType == AppMenuItemType.Header)
            {
                item.IsActive = false;
                item.IsOpen = false;
                continue;
            }

            var selfActive = IsCurrentSystemName(item, activeSystemName)
                || (string.IsNullOrWhiteSpace(activeSystemName) && IsCurrentRoute(item, area, controller, action));
            var childActive = item.Children.Count > 0 && MarkState(item.Children, activeSystemName, area, controller, action);

            item.IsActive = selfActive || childActive;
            item.IsOpen = childActive;

            if (item.IsActive)
                anyActive = true;
        }

        return anyActive;
    }

    private static bool IsCurrentSystemName(AppMenuItemModel item, string activeSystemName)
    {
        return !string.IsNullOrWhiteSpace(activeSystemName)
            && !string.IsNullOrWhiteSpace(item.SystemName)
            && string.Equals(item.SystemName, activeSystemName, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCurrentRoute(AppMenuItemModel item, string area, string controller, string action)
    {
        if (string.IsNullOrWhiteSpace(item.Controller) || string.IsNullOrWhiteSpace(item.Action))
            return false;

        var itemArea = item.Area ?? string.Empty;
        var itemController = item.Controller ?? string.Empty;
        var itemAction = item.Action ?? string.Empty;

        return string.Equals(itemArea, area, StringComparison.OrdinalIgnoreCase)
            && string.Equals(itemController, controller, StringComparison.OrdinalIgnoreCase)
            && string.Equals(itemAction, action, StringComparison.OrdinalIgnoreCase);
    }
}