using GropMng.Web.Infrastructure.Navigation;
using GropMng.Web.Models.Navigation;
using Microsoft.AspNetCore.Mvc;

namespace GropMng.Web.ViewComponents;

/// <summary>
/// Renders the application sidebar menu and marks active/open items based on current route.
/// </summary>
public class AppMenuViewComponent : ViewComponent
{
    private readonly IAppMenuProvider _menuProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppMenuViewComponent"/> class.
    /// </summary>
    /// <param name="menuProvider">The menu provider.</param>
    public AppMenuViewComponent(IAppMenuProvider menuProvider)
    {
        _menuProvider = menuProvider;
    }

    /// <summary>
    /// Builds and returns the sidebar menu tree for the current request.
    /// </summary>
    /// <returns>A view result containing a route-aware menu tree.</returns>
    public IViewComponentResult Invoke()
    {
        var currentArea = (ViewContext.RouteData.Values["area"]?.ToString() ?? string.Empty).Trim();
        var currentController = (ViewContext.RouteData.Values["controller"]?.ToString() ?? string.Empty).Trim();
        var currentAction = (ViewContext.RouteData.Values["action"]?.ToString() ?? string.Empty).Trim();

        var menu = _menuProvider.Build();
        MarkState(menu, currentArea, currentController, currentAction);

        return View(menu);
    }

    private static bool MarkState(IEnumerable<AppMenuItemModel> items, string area, string controller, string action)
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

            var selfActive = IsCurrent(item, area, controller, action);
            var childActive = item.Children.Count > 0 && MarkState(item.Children, area, controller, action);

            item.IsActive = selfActive || childActive;
            item.IsOpen = childActive;

            if (item.IsActive)
                anyActive = true;
        }

        return anyActive;
    }

    private static bool IsCurrent(AppMenuItemModel item, string area, string controller, string action)
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