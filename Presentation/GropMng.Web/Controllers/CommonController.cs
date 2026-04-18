using System.Diagnostics;
using GropMng.Web.Models;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace GropMng.Web.Controllers;

/// <summary>
/// Represents the CommonController component.
/// Defines responsibilities and data relevant to its role in the GropMng solution.
/// </summary>
public class CommonController : Controller
{
    /// <summary>
    /// Renders the generic error page with the current request identifier.
    /// </summary>
    /// <returns>The error view model used by the shared error page.</returns>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }

    /// <summary>
    /// Sets the application language culture via cookie and redirects to the return URL.
    /// </summary>
    /// <param name="culture">The culture code to set (e.g., "el-GR", "en-US").</param>
    /// <param name="returnUrl">The URL to redirect to after setting the culture. Defaults to "/" if not provided or invalid.</param>
    /// <returns>A redirect result to the specified or default URL.</returns>
    [HttpPost]
    public IActionResult SetLanguage(string culture, string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(culture))
            return RedirectToAction("Index", "Home");

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) });

        // Validate returnUrl to prevent open redirect vulnerabilities
        if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
            return RedirectToAction("Index", "Home");

        return Redirect(returnUrl);
    }
}
