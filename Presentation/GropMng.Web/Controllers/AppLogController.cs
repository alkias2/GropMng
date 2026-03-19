using Microsoft.AspNetCore.Mvc;
using GropMng.Core.Interfaces.Services.Logging;

namespace GropMng.Web.Controllers;

/// <summary>
/// Legacy AppLog controller — redirects all routes to the Admin area equivalent.
/// The actual AppLog UI and API now live in <c>Areas/Admin/Controllers/AppLogController</c>.
/// This class remains to avoid breaking any bookmarked or hard-coded URLs.
/// </summary>
public class AppLogController : Controller
{
    [HttpGet]
    public IActionResult Index()
        => RedirectToAction("Index", "AppLog", new { area = "Admin" });

    [HttpPost]
    public IActionResult List()
        => RedirectToAction("Index", "AppLog", new { area = "Admin" });

    [HttpGet]
    public IActionResult Details(int id)
        => RedirectToAction("Details", "AppLog", new { area = "Admin", id });

    [HttpPost]
    public IActionResult Delete(int id)
        => RedirectToAction("Index", "AppLog", new { area = "Admin" });

    [HttpPost]
    public IActionResult DeleteAll()
        => RedirectToAction("Index", "AppLog", new { area = "Admin" });
}
