using GropMng.Core.Common.Exceptions;
using GropMng.Core.Interfaces.Services.Garden.Health;
using GropMng.Core.Interfaces.Services.Localization;
using GropMng.Web.Areas.Admin.Factories.Disease;
using GropMng.Web.Areas.Admin.Models.Disease;
using GropMng.Web.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;

namespace GropMng.Web.Areas.Admin.Controllers;

/// <summary>
/// Administers disease catalog entries and their remedy links in the Admin area.
/// </summary>
[Area("Admin")]
[AuthorizeAdmin]
[CheckPermission(GropMngPermissions.Garden.ManageReferenceData)]
public class DiseaseController : Controller
{
    #region Fields

    private readonly IDiseaseModelFactory _diseaseModelFactory;
    private readonly IDiseaseService _diseaseService;
    private readonly ILocalizationService _localizationService;

    #endregion

    #region Ctor

    public DiseaseController(
        IDiseaseModelFactory diseaseModelFactory,
        IDiseaseService diseaseService,
        ILocalizationService localizationService)
    {
        _diseaseModelFactory = diseaseModelFactory;
        _diseaseService = diseaseService;
        _localizationService = localizationService;
    }

    #endregion

    #region Disease CRUD

    [HttpGet]
    public IActionResult Index() => RedirectToAction(nameof(List));

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var searchModel = await _diseaseModelFactory.PrepareSearchModelAsync(cancellationToken: cancellationToken);
        return View(searchModel);
    }

    [HttpPost]
    public async Task<IActionResult> DiseaseList([FromForm] DiseaseSearchModel searchModel, CancellationToken cancellationToken)
    {
        var listModel = await _diseaseModelFactory.PrepareListModelAsync(searchModel, cancellationToken);
        return Json(listModel);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = await _diseaseModelFactory.PrepareCreateModelAsync(cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DiseaseModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var preparedModel = await _diseaseModelFactory.PrepareCreateModelAsync(cancellationToken);
            MergePostedValues(preparedModel, model);
            return View(preparedModel);
        }

        try
        {
            await _diseaseModelFactory.SaveCreateAsync(model, cancellationToken);
            TempData["SuccessMessage"] = await _localizationService.GetResourceAsync("admin.disease.notifications.create.success");
            return RedirectToAction(nameof(List));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var preparedModel = await _diseaseModelFactory.PrepareCreateModelAsync(cancellationToken);
            MergePostedValues(preparedModel, model);
            return View(preparedModel);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var model = await _diseaseModelFactory.PrepareEditModelAsync(id, cancellationToken);
        if (model == null)
            return RedirectToAction(nameof(List));

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(DiseaseModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var hydratedModel = await _diseaseModelFactory.PrepareEditModelAsync(model.Id, cancellationToken)
                ?? await _diseaseModelFactory.PrepareCreateModelAsync(cancellationToken);
            MergePostedValues(hydratedModel, model);
            return View(hydratedModel);
        }

        try
        {
            var updated = await _diseaseModelFactory.SaveEditAsync(model, cancellationToken);
            if (!updated)
                return RedirectToAction(nameof(List));

            TempData["SuccessMessage"] = await _localizationService.GetResourceAsync("admin.disease.notifications.edit.success");
            return RedirectToAction(nameof(Edit), new { id = model.Id });
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var hydratedModel = await _diseaseModelFactory.PrepareEditModelAsync(model.Id, cancellationToken)
                ?? await _diseaseModelFactory.PrepareCreateModelAsync(cancellationToken);
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
            await _diseaseService.DeleteDiseaseAsync(id, cancellationToken);
            TempData["SuccessMessage"] = await _localizationService.GetResourceAsync("admin.disease.notifications.delete.success");
        }
        catch (DomainException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(List));
    }

    #endregion

    #region RemedyLink sub-operations

    [HttpGet]
    public async Task<IActionResult> RemedyLinkList(int diseaseId, CancellationToken cancellationToken)
    {
        var rows = await _diseaseModelFactory.PrepareRemedyLinkRowsAsync(diseaseId, cancellationToken);
        return Json(new { data = rows });
    }

    [HttpGet]
    public async Task<IActionResult> AddRemedyLinkForm(int diseaseId, CancellationToken cancellationToken)
    {
        var model = await _diseaseModelFactory.PrepareRemedyLinkCreateModelAsync(diseaseId, cancellationToken);
        return PartialView("_RemedyLinkForm", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddRemedyLink(DiseaseRemedyLinkModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, message = "Validation failed." });

        try
        {
            await _diseaseModelFactory.AddRemedyLinkAsync(model, cancellationToken);
            return Json(new { success = true });
        }
        catch (DomainException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRemedyLink(int id, int diseaseId, CancellationToken cancellationToken)
    {
        try
        {
            await _diseaseModelFactory.DeleteRemedyLinkAsync(diseaseId, id, cancellationToken);
        }
        catch (DomainException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Edit), new { id = diseaseId });
    }

    #endregion

    #region Privates

    private static void MergePostedValues(DiseaseModel target, DiseaseModel source)
    {
        target.Id = source.Id;
        target.Name = source.Name;
        target.DiseaseType = source.DiseaseType;
        target.Symptoms = source.Symptoms;
        target.AffectedParts = source.AffectedParts;
        target.PreventionNotes = source.PreventionNotes;
        target.Notes = source.Notes;
    }

    #endregion
}
