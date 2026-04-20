using GropMng.Core.Common.Exceptions;
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

    public OwnerController(IOwnerModelFactory ownerModelFactory)
    {
        _ownerModelFactory = ownerModelFactory ?? throw new ArgumentNullException(nameof(ownerModelFactory));
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

            TempData["SuccessMessage"] = "Owner account updated successfully.";
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
    }
}
