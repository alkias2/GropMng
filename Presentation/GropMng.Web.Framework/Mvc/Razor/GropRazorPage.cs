using GropMng.Core.Interfaces.Services.Localization;
using GropMng.Web.Framework.UI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace GropMng.Web.Framework.Mvc.Razor;

/// <summary>
/// Base Razor page that exposes Nop-style localization helper methods with culture awareness.
/// </summary>
/// <typeparam name="TModel">The page model type.</typeparam>
public abstract class GropRazorPage<TModel> : RazorPage<TModel>
{
    /// <summary>
    /// Gets the request-scoped UI helper used to coordinate admin page state.
    /// </summary>
    public IGropHtmlHelper GropHtml => Context.RequestServices.GetRequiredService<IGropHtmlHelper>();

    /// <summary>
    /// Resolves a localized resource by key, respecting the current request culture.
    /// </summary>
    /// <param name="resourceKey">The resource key.</param>
    /// <returns>The localized string.</returns>
    public string T(string resourceKey)
    {
        if (string.IsNullOrWhiteSpace(resourceKey))
            return string.Empty;

        var localizationService = Context.RequestServices.GetRequiredService<ILocalizationService>();
        var languageService = Context.RequestServices.GetRequiredService<ILanguageService>();
        
        // Resolve language ID from current request culture
        var languageId = GetCurrentLanguageIdAsync(languageService).GetAwaiter().GetResult();
        
        // Fallback to default language if culture resolution fails
        if (languageId <= 0)
        {
            var defaultLanguage = languageService.GetDefaultLanguageAsync().GetAwaiter().GetResult();
            languageId = defaultLanguage.Id;
        }

        return localizationService.GetResourceAsync(resourceKey, languageId).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Gets the currently selected culture code (e.g., "el-GR", "en-US").
    /// Used for UI indication of active language.
    /// </summary>
    /// <returns>The culture code if available; otherwise null.</returns>
    public string GetCurrentCultureCode()
    {
        try
        {
            var httpContext = ViewContext?.HttpContext;
            if (httpContext == null)
                return null;

            var requestCultureFeature = httpContext.Features.Get<IRequestCultureFeature>();
            return requestCultureFeature?.RequestCulture.Culture.Name;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Resolves the current language ID from the HttpContext request culture.
    /// </summary>
    /// <param name="languageService">The language service to query available languages.</param>
    /// <returns>The language ID if resolved from culture; otherwise 0.</returns>
    private async Task<int> GetCurrentLanguageIdAsync(ILanguageService languageService)
    {
        try
        {
            var httpContext = ViewContext?.HttpContext;
            if (httpContext == null)
                return 0;

            var requestCultureFeature = httpContext.Features.Get<IRequestCultureFeature>();
            if (requestCultureFeature?.RequestCulture.Culture == null)
                return 0;

            var cultureName = requestCultureFeature.RequestCulture.Culture.Name;
            if (string.IsNullOrWhiteSpace(cultureName))
                return 0;

            // Map culture code (e.g., "el-GR" -> "el", "en-US" -> "en")
            var cultureCode = cultureName.Split('-')[0].ToLowerInvariant();
            
            // Query language by SEO code (el, en, etc.)
            var languages = await languageService.GetAllLanguagesAsync();
            var language = languages.FirstOrDefault(l => 
                l.UniqueSeoCode != null && string.Equals(l.UniqueSeoCode, cultureCode, StringComparison.OrdinalIgnoreCase));

            return language?.Id ?? 0;
        }
        catch
        {
            // Silent fail - will fall back to default language in caller
            return 0;
        }
    }
}
