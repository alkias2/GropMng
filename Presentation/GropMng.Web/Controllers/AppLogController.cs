using GropMng.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using GropMng.Core.Interfaces.Services;

namespace GropMng.Web.Controllers;

/// <summary>
/// Represents the AppLogController component.
/// Defines responsibilities and data relevant to its role in the GropMng solution.
/// </summary>
public class AppLogController : Controller
{
    private readonly IAppLogService _appLogService;

    /// <summary>
    /// Initializes a new instance of the AppLogController class.
    /// </summary>
    /// <param name="appLogService">Service used to query and manage application logs.</param>
    public AppLogController(IAppLogService appLogService)
    {
        _appLogService = appLogService;
    }

    /// <summary>
    /// Renders the main application logs page.
    /// </summary>
    /// <returns>The logs index view.</returns>
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Returns paged log data in DataTables-compatible JSON format.
    /// </summary>
    /// <returns>A JSON payload containing paging metadata and current page rows.</returns>
    [HttpPost]
    public async Task<IActionResult> List()
    {
        var draw = ParseInt(Request.Form["draw"], 1);
        var start = ParseInt(Request.Form["start"], 0);
        var length = ParseInt(Request.Form["length"], 10);

        var pageIndex = length > 0 ? start / length : 0;
        var pageSize = length > 0 ? length : 10;

        var level = Request.Form["level"].ToString();
        var fromDate = ParseDate(Request.Form["fromDate"]);
        var toDate = ParseDate(Request.Form["toDate"]);

        var totalCount = await _appLogService.GetLogsCountAsync();
        var paged = await _appLogService.GetAllLogsAsync(pageIndex, pageSize, level, fromDate, toDate);

        var data = paged.Select(x => new AppLogListItemViewModel
        {
            Id = x.Id,
            Level = x.Level,
            Category = x.Category,
            Message = x.Message,
            Timestamp = x.Timestamp
        });

        return Json(new
        {
            draw,
            recordsTotal = totalCount,
            recordsFiltered = paged.TotalCount,
            data
        });
    }

    /// <summary>
    /// Displays details for a single log entry.
    /// </summary>
    /// <param name="id">The identifier of the log entry to display.</param>
    /// <returns>The details view when found; otherwise redirects to Index.</returns>
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var log = await _appLogService.GetLogByIdAsync(id);
        if (log == null)
            return RedirectToAction("Index");

        return View(log);
    }

    /// <summary>
    /// Deletes a single log entry.
    /// </summary>
    /// <param name="id">The identifier of the log entry to delete.</param>
    /// <returns>A redirect to the logs index page.</returns>
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await _appLogService.DeleteLogAsync(id);
        return RedirectToAction("Index");
    }

    /// <summary>
    /// Deletes all log entries.
    /// </summary>
    /// <returns>A redirect to the logs index page.</returns>
    [HttpPost]
    public async Task<IActionResult> DeleteAll()
    {
        await _appLogService.ClearAllLogsAsync();
        return RedirectToAction("Index");
    }

    /// <summary>
    /// Parses an integer value and falls back to a default when parsing fails.
    /// </summary>
    /// <param name="value">The raw value to parse.</param>
    /// <param name="fallback">The fallback value to return on parse failure.</param>
    /// <returns>The parsed integer or the provided fallback value.</returns>
    private static int ParseInt(string? value, int fallback)
    {
        return int.TryParse(value, out var parsed) ? parsed : fallback;
    }

    /// <summary>
    /// Parses an optional date value from request input.
    /// </summary>
    /// <param name="value">The raw date value from the request.</param>
    /// <returns>
    /// A UTC DateTime value when parsing succeeds; otherwise null.
    /// </returns>
    private static DateTime? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return DateTime.TryParse(value, out var parsed)
            ? DateTime.SpecifyKind(parsed, DateTimeKind.Utc)
            : null;
    }
}
