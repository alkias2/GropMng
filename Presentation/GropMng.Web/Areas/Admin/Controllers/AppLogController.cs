using GropMng.Core.Interfaces.Services.Logging;
using GropMng.Web.Areas.Admin.Models.Logging;
using GropMng.Web.Factories.Logging;
using Microsoft.AspNetCore.Mvc;

namespace GropMng.Web.Areas.Admin.Controllers;

/// <summary>
/// Administers application log entries within the Admin area.
/// Follows the thin-controller / factory pattern: the controller delegates all
/// model preparation to <see cref="IAppLogModelFactory"/> and only orchestrates
/// HTTP concerns (routing, serialisation and redirects).
/// </summary>
[Area("Admin")]
public class AppLogController : Controller
{
    private readonly IAppLogModelFactory _factory;
    private readonly IAppLogService _appLogService;

    public AppLogController(IAppLogModelFactory factory, IAppLogService appLogService)
    {
        _factory = factory;
        _appLogService = appLogService;
    }

    /// <summary>Renders the AppLog index page, passing an initialised search model to the view.</summary>
    [HttpGet]
    public IActionResult Index()
    {
        var searchModel = _factory.PrepareSearchModel();
        return View(searchModel);
    }

    /// <summary>
    /// Returns paged, filtered log rows in the DataTables server-side JSON format.
    /// DataTables posts draw/start/length plus any custom filter fields defined in
    /// <see cref="AppLogSearchModel"/>.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> List([FromForm] AppLogSearchModel searchModel)
    {
        var listModel = await _factory.PrepareListModelAsync(searchModel);
        return Json(listModel);
    }

    /// <summary>Displays the full details of a single log entry.</summary>
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var log = await _appLogService.GetLogByIdAsync(id);
        if (log == null)
            return RedirectToAction(nameof(Index));

        return View(log);
    }

    /// <summary>Deletes a single log entry and redirects back to the index.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _appLogService.DeleteLogAsync(id);
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Deletes all log entries and redirects back to the index.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAll()
    {
        await _appLogService.ClearAllLogsAsync();
        return RedirectToAction(nameof(Index));
    }
}
