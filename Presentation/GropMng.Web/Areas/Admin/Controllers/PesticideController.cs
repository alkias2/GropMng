using GropMng.Core.Common.Exceptions;
using GropMng.Core.Interfaces.Services.Garden.Health;
using GropMng.Core.Interfaces.Services.Localization;
using GropMng.Web.Areas.Admin.Factories.Pesticide;
using GropMng.Web.Areas.Admin.Models.Pesticide;
using GropMng.Web.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;

namespace GropMng.Web.Areas.Admin.Controllers;

/// <summary>
/// Administers pesticide catalog entries in the Admin area.
/// </summary>
[Area("Admin")]
[AuthorizeAdmin]
[CheckPermission(GropMngPermissions.Garden.ManageReferenceData)]
public class PesticideController : Controller
{
    #region Fields

    private readonly IPesticideModelFactory _pesticideModelFactory;
    private readonly IPesticideService _pesticideService;
    private readonly ILocalizationService _localizationService;

    #endregion

    #region Ctor

    public PesticideController(
        IPesticideModelFactory pesticideModelFactory,
        IPesticideService pesticideService,
        ILocalizationService localizationService)
    {
        _pesticideModelFactory = pesticideModelFactory;
        _pesticideService = pesticideService;
        _localizationService = localizationService;
    }

    #endregion

    #region Public

    [HttpGet]
    public IActionResult Index() => RedirectToAction(nameof(List));

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var searchModel = await _pesticideModelFactory.PrepareSearchModelAsync(cancellationToken: cancellationToken);
        return View(searchModel);
    }

    [HttpPost]
    public async Task<IActionResult> PesticideList([FromForm] PesticideSearchModel searchModel, CancellationToken cancellationToken)
    {
        var listModel = await _pesticideModelFactory.PrepareListModelAsync(searchModel, cancellationToken);
        return Json(listModel);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = await _pesticideModelFactory.PrepareCreateModelAsync(cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PesticideModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var preparedModel = await _pesticideModelFactory.PrepareCreateModelAsync(cancellationToken);
            MergePostedValues(preparedModel, model);
            return View(preparedModel);
        }

        try
        {
            await _pesticideModelFactory.SaveCreateAsync(model, cancellationToken);
            TempData["SuccessMessage"] = await _localizationService.GetResourceAsync("admin.pesticide.notifications.create.success");
            return RedirectToAction(nameof(List));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var preparedModel = await _pesticideModelFactory.PrepareCreateModelAsync(cancellationToken);
            MergePostedValues(preparedModel, model);
            return View(preparedModel);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var model = await _pesticideModelFactory.PrepareEditModelAsync(id, cancellationToken);
        if (model == null)
            return RedirectToAction(nameof(List));

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PesticideModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var hydratedModel = await _pesticideModelFactory.PrepareEditModelAsync(model.Id, cancellationToken)
                ?? await _pesticideModelFactory.PrepareCreateModelAsync(cancellationToken);
            MergePostedValues(hydratedModel, model);
            return View(hydratedModel);
        }

        try
        {
            var updated = await _pesticideModelFactory.SaveEditAsync(model, cancellationToken);
            if (!updated)
                return RedirectToAction(nameof(List));

            TempData["SuccessMessage"] = await _localizationService.GetResourceAsync("admin.pesticide.notifications.edit.success");
            return RedirectToAction(nameof(Edit), new { id = model.Id });
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var hydratedModel = await _pesticideModelFactory.PrepareEditModelAsync(model.Id, cancellationToken)
                ?? await _pesticideModelFactory.PrepareCreateModelAsync(cancellationToken);
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
            await _pesticideService.DeletePesticideAsync(id, cancellationToken);
            TempData["SuccessMessage"] = await _localizationService.GetResourceAsync("admin.pesticide.notifications.delete.success");
        }
        catch (DomainException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(List));
    }

    #endregion

    #region Privates

    private static void MergePostedValues(PesticideModel target, PesticideModel source)
    {
        target.Id = source.Id;
        target.Name = source.Name;
        target.Brand = source.Brand;
        target.ActiveIngredient = source.ActiveIngredient;
        target.PesticideType = source.PesticideType;
        target.ApplicationMethod = source.ApplicationMethod;
        target.IsOrganic = source.IsOrganic;
        target.WithholdingDays = source.WithholdingDays;
        target.SafetyNotes = source.SafetyNotes;
        target.Notes = source.Notes;
    }

    #endregion
}
