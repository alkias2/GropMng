using GropMng.Core.Common.Exceptions;
using GropMng.Core.Interfaces.Services.Localization;
using GropMng.Core.Interfaces.Services.User;
using GropMng.Web.Areas.Admin.Models.Owner;
using GropMng.Web.Areas.Admin.Factories.User;
using GropMng.Web.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;

namespace GropMng.Web.Areas.Admin.Controllers;

/// <summary>
/// Administers owner accounts in the admin area.
/// </summary>
[Area("Admin")]
[AuthorizeAdmin]
[CheckPermission(GropMngPermissions.Owners.ManageOwners)]
public class OwnerController : Controller
{
    private readonly IOwnerModelFactory _ownerModelFactory;
    private readonly IOwnerAccountFlowService _ownerAccountFlowService;
    private readonly ILocalizationService _localizationService;

    public OwnerController(
        IOwnerModelFactory ownerModelFactory,
        IOwnerAccountFlowService ownerAccountFlowService,
        ILocalizationService localizationService)
    {
        _ownerModelFactory = ownerModelFactory ?? throw new ArgumentNullException(nameof(ownerModelFactory));
        _ownerAccountFlowService = ownerAccountFlowService ?? throw new ArgumentNullException(nameof(ownerAccountFlowService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
    }

    [HttpGet]
    public IActionResult Index()
        => RedirectToAction(nameof(List));

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var model = await _ownerModelFactory.PrepareSearchModelAsync(cancellationToken: cancellationToken);
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> OwnerList([FromForm] OwnerSearchModel searchModel, CancellationToken cancellationToken)
    {
        var listModel = await _ownerModelFactory.PrepareListModelAsync(searchModel, cancellationToken);
        return Json(listModel);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var model = await _ownerModelFactory.PrepareEditModelAsync(id, cancellationToken);
        if (model is null)
            return RedirectToAction(nameof(List));

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(OwnerEditModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var invalidModel = await _ownerModelFactory.PrepareEditModelAsync(model.OwnerId, cancellationToken) ?? model;
            MergePostedValues(invalidModel, model);
            return View(invalidModel);
        }

        try
        {
            var updated = await _ownerModelFactory.SaveEditAsync(model, cancellationToken);
            if (!updated)
                return RedirectToAction(nameof(List));

            TempData["SuccessMessage"] = await _localizationService.GetResourceAsync("admin.owner.notifications.edit.success");
            return RedirectToAction(nameof(Edit), new { id = model.OwnerId });
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var hydratedModel = await _ownerModelFactory.PrepareEditModelAsync(model.OwnerId, cancellationToken) ?? model;
            MergePostedValues(hydratedModel, model);
            return View(hydratedModel);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(OwnerEditModel model, CancellationToken cancellationToken)
    {
        if (model.OwnerId == Guid.Empty)
            return RedirectToAction(nameof(List));

        if (string.IsNullOrWhiteSpace(model.Password))
            ModelState.AddModelError(nameof(model.Password), await _localizationService.GetResourceAsync("admin.owner.password.validation.required"));

        if (!ModelState.IsValid)
        {
            var invalidModel = await _ownerModelFactory.PrepareEditModelAsync(model.OwnerId, cancellationToken) ?? model;
            MergePostedValues(invalidModel, model);
            return View("Edit", invalidModel);
        }

        var result = await _ownerAccountFlowService.ChangeOwnerPasswordAsync(
            new ChangeOwnerPasswordRequest(model.OwnerId, model.Password),
            cancellationToken);

        if (!result.Success)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(nameof(model.Password), await _localizationService.GetResourceAsync(error));

            var invalidModel = await _ownerModelFactory.PrepareEditModelAsync(model.OwnerId, cancellationToken) ?? model;
            MergePostedValues(invalidModel, model);
            return View("Edit", invalidModel);
        }

        TempData["SuccessMessage"] = await _localizationService.GetResourceAsync("admin.owner.password.notifications.change.success");
        return RedirectToAction(nameof(Edit), new { id = model.OwnerId });
    }

    private static void MergePostedValues(OwnerEditModel destination, OwnerEditModel source)
    {
        destination.FirstName = source.FirstName;
        destination.LastName = source.LastName;
        destination.DisplayName = source.DisplayName;
        destination.Email = source.Email;
        destination.Status = source.Status;
        destination.IsActive = source.IsActive;
        destination.IsEmailConfirmed = source.IsEmailConfirmed;
        destination.SelectedRoleSystemNames = source.SelectedRoleSystemNames;
        destination.Password = source.Password;
    }
}
