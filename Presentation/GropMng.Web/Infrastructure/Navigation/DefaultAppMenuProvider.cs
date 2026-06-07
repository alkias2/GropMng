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
    private readonly IAppMenuSiteMap _appMenuSiteMap;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultAppMenuProvider"/> class.
    /// </summary>
    public DefaultAppMenuProvider(IHttpContextAccessor httpContextAccessor, IAppMenuSiteMap appMenuSiteMap)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _appMenuSiteMap = appMenuSiteMap ?? throw new ArgumentNullException(nameof(appMenuSiteMap));
    }

    /// <inheritdoc />
    public async Task<IList<AppMenuItemModel>> BuildAsync(CancellationToken cancellationToken = default)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        var isAdministrator = user?.Identity?.IsAuthenticated == true
            && (user.IsInRole("Administrator")
                || string.Equals(user.FindFirstValue(ClaimTypes.Role), "Administrator", StringComparison.OrdinalIgnoreCase));

        var rootNode = await _appMenuSiteMap.LoadAsync(cancellationToken);
        return rootNode.Children
            .Select(node => MapNode(node, isAdministrator))
            .Where(item => item is not null)
            .Cast<AppMenuItemModel>()
            .ToList();
    }

    private static AppMenuItemModel? MapNode(AppMenuSiteMapNode node, bool isAdministrator)
    {
        if (!node.Visible || (node.RequiresAdministrator && !isAdministrator))
            return null;

        var children = node.Children
            .Select(child => MapNode(child, isAdministrator))
            .Where(item => item is not null)
            .Cast<AppMenuItemModel>()
            .ToList();

        if (node.ItemType != AppMenuItemType.Header && string.IsNullOrWhiteSpace(node.Title) && children.Count == 0)
            return null;

        if (node.ItemType != AppMenuItemType.Header &&
            string.IsNullOrWhiteSpace(node.Controller) &&
            string.IsNullOrWhiteSpace(node.Action) &&
            children.Count == 0)
        {
            return null;
        }

        return new AppMenuItemModel
        {
            Key = BuildKey(node.SystemName, node.Title),
            SystemName = node.SystemName,
            Title = node.Title,
            ItemType = node.ItemType,
            IconClass = node.IconClass,
            Area = node.Area,
            Controller = node.Controller,
            Action = node.Action,
            Children = children
        };
    }

    private static string BuildKey(string systemName, string title)
    {
        var source = !string.IsNullOrWhiteSpace(systemName) ? systemName : title;
        if (string.IsNullOrWhiteSpace(source))
            return Guid.NewGuid().ToString("N");

        return source
            .Trim()
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("&", "and");
    }
}