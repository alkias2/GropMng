using GropMng.Core.Interfaces.Services.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;

namespace GropMng.Web.Extensions;

/// <summary>
/// Provides Razor helper extensions for localization resource lookup.
/// </summary>
public static class HtmlLocalizationExtension
{
    /// <summary>
    /// Resolves a localization resource by key.
    /// </summary>
    /// <param name="htmlHelper">The HTML helper instance.</param>
    /// <param name="resourceKey">The resource key.</param>
    /// <returns>The localized value.</returns>
    public static async Task<string> T(this IHtmlHelper htmlHelper, string resourceKey)
    {
        ArgumentNullException.ThrowIfNull(htmlHelper);

        if (string.IsNullOrWhiteSpace(resourceKey))
            return string.Empty;

        var localizationService = htmlHelper.ViewContext.HttpContext.RequestServices.GetRequiredService<ILocalizationService>();
        return await localizationService.GetResourceAsync(resourceKey);
    }
}
