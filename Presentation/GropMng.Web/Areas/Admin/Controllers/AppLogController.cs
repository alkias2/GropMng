using FluentValidation;
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
    private readonly IValidator<AppLogSearchModel> _searchValidator;

    public AppLogController(
        IAppLogModelFactory factory,
        IAppLogService appLogService,
        IValidator<AppLogSearchModel> searchValidator)
    {
        _factory = factory;
        _appLogService = appLogService;
        _searchValidator = searchValidator;
    }

    /// <summary>Renders the AppLog index page, passing an initialised search model to the view.</summary>
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var searchModel = await _factory.PrepareSearchModelAsync(cancellationToken: cancellationToken);
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
        var validationResult = await _searchValidator.ValidateAsync(searchModel);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.ErrorMessage).ToArray());

            return BadRequest(new { errors });
        }

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

    /// <summary>
    /// Deletes selected log entries and returns a JSON response for AJAX callers.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSelected([FromForm] int[] selectedIds)
    {
        if (selectedIds == null || selectedIds.Length == 0)
            return BadRequest(new { success = false, message = "No log entries were selected." });

        await _appLogService.DeleteLogsAsync(selectedIds);
        return Json(new { success = true, deletedCount = selectedIds.Length });
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
