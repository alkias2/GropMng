using GropMng.Web.Models.Navigation;

namespace GropMng.Web.Infrastructure.Navigation;

/// <summary>
/// Default route-based sidebar menu provider.
/// </summary>
public class DefaultAppMenuProvider : IAppMenuProvider
{
    /// <inheritdoc />
    public IList<AppMenuItemModel> Build()
    {
        return new List<AppMenuItemModel>
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
            },
            new()
            {
                Key = "section-admin",
                Title = "Administration",
                ItemType = AppMenuItemType.Header
            },
            new()
            {
                Key = "admin",
                Title = "Admin",
                IconClass = "bx bx-cog",
                ItemType = AppMenuItemType.Link,
                Children = new List<AppMenuItemModel>
                {
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
            }
        };
    }
}