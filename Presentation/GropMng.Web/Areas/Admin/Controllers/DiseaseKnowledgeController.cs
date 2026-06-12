using FluentValidation;
using GropMng.Core.Common.Exceptions;
using GropMng.Core.Interfaces.Services.Garden.Health;
using GropMng.Core.Interfaces.Services.Localization;
using GropMng.Web.Areas.Admin.Factories;
using GropMng.Web.Areas.Admin.Models;
using GropMng.Web.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;

namespace GropMng.Web.Areas.Admin.Controllers;

/// <summary>
/// Administers the disease knowledge base in the Admin area.
/// </summary>
[Area("Admin")]
[AuthorizeAdmin]
[CheckPermission(GropMngPermissions.Garden.ManageReferenceData)]
public class DiseaseKnowledgeController : Controller
{
    #region Fields

    private readonly IDiseaseKnowledgeModelFactory _diseaseKnowledgeModelFactory;
    private readonly IDiseaseKnowledgeService _diseaseKnowledgeService;
    private readonly IAdminNotificationService _adminNotificationService;
    private readonly IPlantProblemService _plantProblemService;
    private readonly ILocalizationService _localizationService;

    #endregion

    #region Ctor

    public DiseaseKnowledgeController(
        IDiseaseKnowledgeModelFactory diseaseKnowledgeModelFactory,
        IDiseaseKnowledgeService diseaseKnowledgeService,
        IAdminNotificationService adminNotificationService,
        IPlantProblemService plantProblemService,
        ILocalizationService localizationService)
    {
        _diseaseKnowledgeModelFactory = diseaseKnowledgeModelFactory;
        _diseaseKnowledgeService = diseaseKnowledgeService;
        _adminNotificationService = adminNotificationService;
        _plantProblemService = plantProblemService;
        _localizationService = localizationService;
    }

    #endregion

    #region Public

    [HttpGet]
    public IActionResult Index() => RedirectToAction(nameof(List));

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var model = await _diseaseKnowledgeModelFactory.PrepareListModelAsync(cancellationToken: cancellationToken);
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> DiseaseKnowledgeList(CancellationToken cancellationToken)
    {
        var listModel = await _diseaseKnowledgeModelFactory.PrepareListModelAsync(cancellationToken);
        return Json(listModel);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? fromNotificationId, CancellationToken cancellationToken)
    {
        var model = await _diseaseKnowledgeModelFactory.PrepareCreateModelAsync(fromNotificationId, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DiseaseKnowledgeEditModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var preparedModel = await _diseaseKnowledgeModelFactory.PrepareCreateModelAsync(model.FromNotificationId, cancellationToken);
            MergePostedValues(preparedModel, model);
            return View(preparedModel);
        }

        try
        {
            var createdId = await _diseaseKnowledgeModelFactory.SaveCreateAsync(model, cancellationToken);

            // Resolve the notification if this was created from one, linking the created knowledge entry
            if (model.FromNotificationId.HasValue)
            {
                await _adminNotificationService.ResolveAsync(model.FromNotificationId.Value, createdId, cancellationToken);

                // Update the original PlantProblemRecord with the new DiseaseKnowledgeId
                // so the user-facing UI shows knowledge info
                var notifications = await _adminNotificationService.GetAllAsync(includeResolved: true, cancellationToken);
                var notification = notifications.FirstOrDefault(n => n.Id == model.FromNotificationId.Value);
                if (notification != null)
                {
                    var records = await _plantProblemService.GetByPlantInstanceAsync(
                        notification.PlantInstanceId, notification.OwnerId, cancellationToken);

                    // Find the record that matches this notification's problem name and is not yet linked
                    var matchingRecord = records.FirstOrDefault(r =>
                        r.ProblemName == notification.ProblemName && !r.DiseaseKnowledgeId.HasValue);

                    if (matchingRecord != null)
                    {
                        matchingRecord.DiseaseKnowledgeId = createdId;
                        await _plantProblemService.UpdateAsync(matchingRecord, notification.OwnerId, cancellationToken);
                    }
                }
            }

            TempData["SuccessMessage"] = await _localizationService.GetResourceAsync("admin.diseaseKnowledge.created");
            return RedirectToAction(nameof(List));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var preparedModel = await _diseaseKnowledgeModelFactory.PrepareCreateModelAsync(model.FromNotificationId, cancellationToken);
            MergePostedValues(preparedModel, model);
            return View(preparedModel);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var model = await _diseaseKnowledgeModelFactory.PrepareEditModelAsync(id, cancellationToken);
        if (model is null)
            return RedirectToAction(nameof(List));

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(DiseaseKnowledgeEditModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var hydratedModel = await _diseaseKnowledgeModelFactory.PrepareEditModelAsync(model.Id!.Value, cancellationToken)
                ?? await _diseaseKnowledgeModelFactory.PrepareCreateModelAsync(null, cancellationToken);
            MergePostedValues(hydratedModel, model);
            return View(hydratedModel);
        }

        try
        {
            await _diseaseKnowledgeModelFactory.SaveEditAsync(model, cancellationToken);
            TempData["SuccessMessage"] = await _localizationService.GetResourceAsync("admin.diseaseKnowledge.updated");
            return RedirectToAction(nameof(Edit), new { id = model.Id });
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var hydratedModel = await _diseaseKnowledgeModelFactory.PrepareEditModelAsync(model.Id!.Value, cancellationToken)
                ?? await _diseaseKnowledgeModelFactory.PrepareCreateModelAsync(null, cancellationToken);
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
            await _diseaseKnowledgeService.DeleteAsync(id, cancellationToken);
            TempData["SuccessMessage"] = await _localizationService.GetResourceAsync("admin.diseaseKnowledge.deleted");
        }
        catch (DomainException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(List));
    }

    #endregion

    #region Privates

    private static void MergePostedValues(DiseaseKnowledgeEditModel target, DiseaseKnowledgeEditModel source)
    {
        target.Id = source.Id;
        target.CommonName = source.CommonName;
        target.ScientificName = source.ScientificName;
        target.Description = source.Description;
        target.TreatmentGuidelines = source.TreatmentGuidelines;
        target.SelectedPlantIds = source.SelectedPlantIds;
        target.FromNotificationId = source.FromNotificationId;
    }

    #endregion
}