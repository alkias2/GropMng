using GropMng.Core.Common.Exceptions;
using GropMng.Web.Areas.Admin.Models.Roles;
using GropMng.Web.Areas.Admin.Factories.User;
using GropMng.Web.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;

namespace GropMng.Web.Areas.Admin.Controllers;

/// <summary>
/// Administers owner roles and permission assignments.
/// </summary>
[Area("Admin")]
[AuthorizeAdmin]
[CheckPermission(GropMngPermissions.Owners.ManageRoles)]
public class OwnerRoleController : Controller
{
    private readonly IOwnerRoleModelFactory _ownerRoleModelFactory;

    public OwnerRoleController(IOwnerRoleModelFactory ownerRoleModelFactory)
    {
        _ownerRoleModelFactory = ownerRoleModelFactory ?? throw new ArgumentNullException(nameof(ownerRoleModelFactory));
    }

    [HttpGet]
    public IActionResult Index()
        => RedirectToAction(nameof(List));

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var model = await _ownerRoleModelFactory.PrepareSearchModelAsync(cancellationToken: cancellationToken);
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> RoleList([FromForm] OwnerRoleSearchModel searchModel, CancellationToken cancellationToken)
    {
        var listModel = await _ownerRoleModelFactory.PrepareListModelAsync(searchModel, cancellationToken);
        return Json(listModel);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var model = await _ownerRoleModelFactory.PrepareEditModelAsync(id, cancellationToken);
        if (model is null)
            return RedirectToAction(nameof(List));

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(OwnerRoleEditModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var invalidModel = await _ownerRoleModelFactory.PrepareEditModelAsync(model.Id, cancellationToken) ?? model;
            MergePostedValues(invalidModel, model);
            return View(invalidModel);
        }

        try
        {
            var updated = await _ownerRoleModelFactory.SaveEditAsync(model, cancellationToken);
            if (!updated)
                return RedirectToAction(nameof(List));

            TempData["SuccessMessage"] = "Role permissions updated successfully.";
            return RedirectToAction(nameof(Edit), new { id = model.Id });
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var hydratedModel = await _ownerRoleModelFactory.PrepareEditModelAsync(model.Id, cancellationToken) ?? model;
            MergePostedValues(hydratedModel, model);
            return View(hydratedModel);
        }
    }

    private static void MergePostedValues(OwnerRoleEditModel destination, OwnerRoleEditModel source)
    {
        destination.Name = source.Name;
        destination.SystemName = source.SystemName;
        destination.Description = source.Description;
        destination.IsActive = source.IsActive;
        destination.SelectedPermissionSystemNames = source.SelectedPermissionSystemNames;

        foreach (var group in destination.PermissionGroups)
        {
            foreach (var permission in group.Permissions)
                permission.Selected = source.SelectedPermissionSystemNames.Contains(permission.SystemName, StringComparer.OrdinalIgnoreCase);
        }
    }
}
