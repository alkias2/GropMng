using FluentValidation;
using GropMng.Core.Common.Exceptions;
using GropMng.Core.Interfaces.Services.Garden.Care;
using GropMng.Core.Interfaces.Services.Localization;
using GropMng.Web.Areas.Admin.Factories.Fertilizer;
using GropMng.Web.Areas.Admin.Models.Fertilizer;
using GropMng.Web.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;

namespace GropMng.Web.Areas.Admin.Controllers;

/// <summary>
/// Administers fertilizer catalog entries in the Admin area.
/// </summary>
[Area("Admin")]
[AuthorizeAdmin]
[CheckPermission(GropMngPermissions.Garden.ManageReferenceData)]
public class FertilizerController : Controller
{
    #region Fields

    private readonly IFertilizerModelFactory _fertilizerModelFactory;
    private readonly IFertilizerService _fertilizerService;
    private readonly ILocalizationService _localizationService;

    #endregion

    #region Ctor

    public FertilizerController(
        IFertilizerModelFactory fertilizerModelFactory,
        IFertilizerService fertilizerService,
        ILocalizationService localizationService)
    {
        _fertilizerModelFactory = fertilizerModelFactory;
        _fertilizerService = fertilizerService;
        _localizationService = localizationService;
    }

    #endregion

    #region Public

    [HttpGet]
    public IActionResult Index() => RedirectToAction(nameof(List));

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var searchModel = await _fertilizerModelFactory.PrepareSearchModelAsync(cancellationToken: cancellationToken);
        return View(searchModel);
    }

    [HttpPost]
    public async Task<IActionResult> FertilizerList([FromForm] FertilizerSearchModel searchModel, CancellationToken cancellationToken)
    {
        var listModel = await _fertilizerModelFactory.PrepareListModelAsync(searchModel, cancellationToken);
        return Json(listModel);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = await _fertilizerModelFactory.PrepareCreateModelAsync(cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FertilizerModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var preparedModel = await _fertilizerModelFactory.PrepareCreateModelAsync(cancellationToken);
            MergePostedValues(preparedModel, model);
            return View(preparedModel);
        }

        try
        {
            await _fertilizerModelFactory.SaveCreateAsync(model, cancellationToken);
            TempData["SuccessMessage"] = await _localizationService.GetResourceAsync("admin.fertilizer.notifications.create.success");
            return RedirectToAction(nameof(List));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var preparedModel = await _fertilizerModelFactory.PrepareCreateModelAsync(cancellationToken);
            MergePostedValues(preparedModel, model);
            return View(preparedModel);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var model = await _fertilizerModelFactory.PrepareEditModelAsync(id, cancellationToken);
        if (model == null)
            return RedirectToAction(nameof(List));

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(FertilizerModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var hydratedModel = await _fertilizerModelFactory.PrepareEditModelAsync(model.Id, cancellationToken)
                ?? await _fertilizerModelFactory.PrepareCreateModelAsync(cancellationToken);
            MergePostedValues(hydratedModel, model);
            return View(hydratedModel);
        }

        try
        {
            var updated = await _fertilizerModelFactory.SaveEditAsync(model, cancellationToken);
            if (!updated)
                return RedirectToAction(nameof(List));

            TempData["SuccessMessage"] = await _localizationService.GetResourceAsync("admin.fertilizer.notifications.edit.success");
            return RedirectToAction(nameof(Edit), new { id = model.Id });
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var hydratedModel = await _fertilizerModelFactory.PrepareEditModelAsync(model.Id, cancellationToken)
                ?? await _fertilizerModelFactory.PrepareCreateModelAsync(cancellationToken);
            MergePostedValues(hydratedModel, model);
            return View(hydratedModel);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _fertilizerService.DeleteFertilizerAsync(id, cancellationToken);
            TempData["SuccessMessage"] = await _localizationService.GetResourceAsync("admin.fertilizer.notifications.delete.success");
        }
        catch (DomainException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(List));
    }

    #endregion

    #region Privates

    private static void MergePostedValues(FertilizerModel target, FertilizerModel source)
    {
        target.Id = source.Id;
        target.Name = source.Name;
        target.Brand = source.Brand;
        target.FertilizerType = source.FertilizerType;
        target.NpkRatio = source.NpkRatio;
        target.ApplicationMethod = source.ApplicationMethod;
        target.IsOrganic = source.IsOrganic;
        target.Notes = source.Notes;
    }

    #endregion
}
