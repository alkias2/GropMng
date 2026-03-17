using System.Diagnostics;
using GropMng.Web.Models;
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
}
