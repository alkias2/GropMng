using System.Security.Claims;
using GropMng.Web.Models.Navigation;
using Microsoft.AspNetCore.Http;

namespace GropMng.Web.Infrastructure.Navigation;

/// <summary>
/// Default route-based sidebar menu provider.
/// </summary>
public class DefaultAppMenuProvider : IAppMenuProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultAppMenuProvider"/> class.
    /// </summary>
    public DefaultAppMenuProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <inheritdoc />
    public IList<AppMenuItemModel> Build()
    {
        var items = new List<AppMenuItemModel>
        {
            new()
            {
                Key = "section-main",
                Title = "Main",
                ItemType = AppMenuItemType.Header
            },
            new()
            {
                Key = "dashboard",
                Title = "Dashboard",
                IconClass = "bx bx-home-circle",
                Area = string.Empty,
                Controller = "Home",
                Action = "Index",
                ItemType = AppMenuItemType.Link
            }
        };

        var user = _httpContextAccessor.HttpContext?.User;
        var isAdministrator = user?.Identity?.IsAuthenticated == true
            && (user.IsInRole("Administrator")
                || string.Equals(user.FindFirstValue(ClaimTypes.Role), "Administrator", StringComparison.OrdinalIgnoreCase));

        if (!isAdministrator)
            return items;

        items.Add(new AppMenuItemModel
        {
            Key = "section-admin",
            Title = "Administration",
            ItemType = AppMenuItemType.Header
        });

        items.Add(new AppMenuItemModel
        {
            Key = "admin",
            Title = "Admin",
            IconClass = "bx bx-cog",
            ItemType = AppMenuItemType.Link,
            Children = new List<AppMenuItemModel>
            {
                new()
                {
                    Key = "admin-owners",
                    Title = "Owners",
                    Area = "Admin",
                    Controller = "Owner",
                    Action = "List",
                    ItemType = AppMenuItemType.Link
                },
                new()
                {
                    Key = "admin-roles",
                    Title = "Roles & Permissions",
                    Area = "Admin",
                    Controller = "OwnerRole",
                    Action = "List",
                    ItemType = AppMenuItemType.Link
                },
                new()
                {
                    Key = "admin-plants",
                    Title = "Plants",
                    Area = "Admin",
                    Controller = "Plant",
                    Action = "Index",
                    ItemType = AppMenuItemType.Link
                },
                new()
                {
                    Key = "admin-applogs",
                    Title = "App Logs",
                    Area = "Admin",
                    Controller = "AppLog",
                    Action = "Index",
                    ItemType = AppMenuItemType.Link
                },
                new()
                {
                    Key = "admin-settings",
                    Title = "Admin Settings",
                    Area = "Admin",
                    Controller = "Setting",
                    Action = "AdminArea",
                    ItemType = AppMenuItemType.Link
                },
                new()
                {
                    Key = "admin-localization",
                    Title = "Localization",
                    Area = "Admin",
                    Controller = "Localization",
                    Action = "Languages",
                    ItemType = AppMenuItemType.Link
                }
            }
        });

        return items;
    }
}