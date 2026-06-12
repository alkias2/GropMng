using GropMng.Core.Interfaces.Services.Localization;
using GropMng.Web.Areas.Admin.Factories;
using GropMng.Web.Areas.Admin.Models;
using GropMng.Web.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;

namespace GropMng.Web.Areas.Admin.Controllers;

/// <summary>
/// Lists pending admin notifications for unknown disease reports.
/// </summary>
[Area("Admin")]
[AuthorizeAdmin]
[CheckPermission(GropMngPermissions.Garden.ManageReferenceData)]
public class AdminNotificationController : Controller
{
    #region Fields

    private readonly IAdminNotificationModelFactory _notificationModelFactory;
    private readonly ILocalizationService _localizationService;

    #endregion

    #region Ctor

    public AdminNotificationController(
        IAdminNotificationModelFactory notificationModelFactory,
        ILocalizationService localizationService)
    {
        _notificationModelFactory = notificationModelFactory;
        _localizationService = localizationService;
    }

    #endregion

    #region Public

    [HttpGet]
    public IActionResult Index() => RedirectToAction(nameof(List));

    [HttpGet]
    public async Task<IActionResult> List(bool showResolved = true, CancellationToken cancellationToken = default)
    {
        // Sync missing notifications before displaying the list
        await _notificationModelFactory.SyncMissingAsync(cancellationToken);
        var notifications = await _notificationModelFactory.PrepareListModelAsync(showResolved, cancellationToken);
        return View(notifications);
    }

    #endregion
}