using System.Xml.Linq;
using GropMng.Core.Interfaces.Services.Localization;
using GropMng.Core.Interfaces.Services.User;
using GropMng.Web.Models.Navigation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace GropMng.Web.Infrastructure.Navigation;

/// <summary>
/// Loads the application menu definition from the admin sitemap XML file.
/// </summary>
public class XmlAppMenuSiteMap : IAppMenuSiteMap
{
    private static readonly char[] PermissionSeparator = [','];
    private const string SiteMapRelativePath = "Areas/Admin/sitemap.config";
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="XmlAppMenuSiteMap"/> class.
    /// </summary>
    /// <param name="webHostEnvironment">The hosting environment.</param>
    public XmlAppMenuSiteMap(IWebHostEnvironment webHostEnvironment, IServiceProvider serviceProvider)
    {
        _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <inheritdoc />
    public async Task<AppMenuSiteMapNode> LoadAsync(CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_webHostEnvironment.ContentRootPath, SiteMapRelativePath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"The admin sitemap configuration was not found at '{filePath}'.", filePath);

        var document = XDocument.Load(filePath, LoadOptions.None);
        var xmlRootNode = document.Root?.Element("siteMapNode");
        if (xmlRootNode is null)
            throw new InvalidOperationException("The admin sitemap configuration does not contain a root siteMapNode element.");

        return await ParseNodeAsync(xmlRootNode, cancellationToken);
    }

    private async Task<AppMenuSiteMapNode> ParseNodeAsync(XElement element, CancellationToken cancellationToken)
    {
        var systemName = ((string?)element.Attribute("SystemName") ?? string.Empty).Trim();
        var itemTypeValue = (string?)element.Attribute("ItemType");
        var itemType = Enum.TryParse<AppMenuItemType>(itemTypeValue, ignoreCase: true, out var parsedItemType)
            ? parsedItemType
            : AppMenuItemType.Link;

        var requiresAdministrator = bool.TryParse((string?)element.Attribute("RequiresAdministrator"), out var parsedRequiresAdministrator)
            && parsedRequiresAdministrator;

        var resourceKey = ((string?)element.Attribute("ResourceKey") ?? string.Empty).Trim();
        var permissionNames = (((string?)element.Attribute("PermissionNames")) ?? string.Empty)
            .Split(PermissionSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        var title = ((string?)element.Attribute("Title") ?? string.Empty).Trim();
        if (!string.IsNullOrWhiteSpace(resourceKey))
        {
            var localizationService = _serviceProvider.GetService<ILocalizationService>();
            if (localizationService is not null)
            {
                var localizedTitle = await localizationService.GetResourceAsync(resourceKey);
                if (!string.IsNullOrWhiteSpace(localizedTitle))
                    title = localizedTitle;
            }
        }

                if (string.IsNullOrWhiteSpace(title))
                    title = systemName;

        var visible = true;
        if (permissionNames.Count > 0)
        {
            var permissionService = _serviceProvider.GetService<IPermissionService>();
            visible = permissionService is not null
                && await AuthorizeAnyAsync(permissionService, permissionNames, cancellationToken);
        }

        var children = new List<AppMenuSiteMapNode>();
        foreach (var childElement in element.Elements("siteMapNode"))
            children.Add(await ParseNodeAsync(childElement, cancellationToken));

        return new AppMenuSiteMapNode
        {
            SystemName = systemName,
            Title = title,
            ResourceKey = resourceKey,
            ItemType = itemType,
            IconClass = ((string?)element.Attribute("IconClass"))?.Trim(),
            Area = ((string?)element.Attribute("area"))?.Trim(),
            Controller = ((string?)element.Attribute("controller"))?.Trim(),
            Action = ((string?)element.Attribute("action"))?.Trim(),
            RequiresAdministrator = requiresAdministrator,
            PermissionNames = permissionNames,
            Visible = visible,
            Children = children
        };
    }

    private static async Task<bool> AuthorizeAnyAsync(IPermissionService permissionService, IEnumerable<string> permissionNames, CancellationToken cancellationToken)
    {
        foreach (var permissionName in permissionNames)
        {
            if (await permissionService.AuthorizeAsync(permissionName, cancellationToken))
                return true;
        }

        return false;
    }
}