using FluentValidation;
using GropMng.Core.Common.Exceptions;
using GropMng.Core.Interfaces.Services.Garden.Plants;
using GropMng.Core.Interfaces.Services.Localization;
using GropMng.Web.Areas.Admin.Models.Plant;
using GropMng.Web.Areas.Admin.Factories.Plant;
using GropMng.Web.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace GropMng.Web.Areas.Admin.Controllers;

/// <summary>
/// Administers plant catalog entities in the Admin area.
/// </summary>
[Area("Admin")]
[AuthorizeAdmin]
[CheckPermission(GropMngPermissions.Garden.ManagePlants)]
public class PlantController : Controller
{
    #region Fields

    private readonly IPlantModelFactory _plantModelFactory;
    private readonly IPlantService _plantService;
    private readonly IValidator<PlantModel> _plantValidator;
    private readonly ILocalizationService _localizationService;

    #endregion

    #region Ctor

    public PlantController(
        IPlantModelFactory plantModelFactory,
        IPlantService plantService,
        IValidator<PlantModel> plantValidator,
        ILocalizationService localizationService)
    {
        _plantModelFactory = plantModelFactory;
        _plantService = plantService;
        _plantValidator = plantValidator;
        _localizationService = localizationService;
    }

    #endregion

    #region Public

    /// <summary>
    /// Redirects the legacy index route to the canonical list page.
    /// </summary>
    [HttpGet]
    public IActionResult Index()
        => RedirectToAction(nameof(List));

    /// <summary>
    /// Renders the Plant list page.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var searchModel = await _plantModelFactory.PrepareSearchModelAsync(cancellationToken: cancellationToken);
        return View(searchModel);
    }

    /// <summary>
    /// Returns paged and filtered plant rows for DataTables.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> PlantList([FromForm] PlantSearchModel searchModel, CancellationToken cancellationToken)
    {
        var listModel = await _plantModelFactory.PrepareListModelAsync(searchModel, cancellationToken);
        return Json(listModel);
    }

    /// <summary>
    /// Renders the create Plant page.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = await _plantModelFactory.PrepareCreateModelAsync(cancellationToken);
        return View(model);
    }

    /// <summary>
    /// Creates a new Plant catalog entry.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PlantModel model, CancellationToken cancellationToken)
    {
        HydratePictureIdFromForm(model);

        var validation = await _plantValidator.ValidateAsync(model, cancellationToken);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }

        if (!ModelState.IsValid)
        {
            var preparedModel = await _plantModelFactory.PrepareCreateModelAsync(cancellationToken);
            MergePostedValues(preparedModel, model);
            return View(preparedModel);
        }

        try
        {
            await _plantModelFactory.SaveCreateAsync(model, cancellationToken);
            TempData["SuccessMessage"] = await _localizationService.GetResourceAsync("admin.plant.notifications.create.success");
            return RedirectToAction(nameof(List));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var preparedModel = await _plantModelFactory.PrepareCreateModelAsync(cancellationToken);
            MergePostedValues(preparedModel, model);
            return View(preparedModel);
        }
    }

    /// <summary>
    /// Renders the edit Plant page.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var model = await _plantModelFactory.PrepareEditModelAsync(id, cancellationToken);
        if (model == null)
            return RedirectToAction(nameof(List));

        return View(model);
    }

    /// <summary>
    /// Updates an existing Plant catalog entry.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PlantModel model, CancellationToken cancellationToken)
    {
        HydratePictureIdFromForm(model);

        var validation = await _plantValidator.ValidateAsync(model, cancellationToken);
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }

        if (!ModelState.IsValid)
        {
            var hydratedModel = await _plantModelFactory.PrepareEditModelAsync(model.Id, cancellationToken)
                ?? await _plantModelFactory.PrepareCreateModelAsync(cancellationToken);

            MergePostedValues(hydratedModel, model);

            return View(hydratedModel);
        }

        try
        {
            var updated = await _plantModelFactory.SaveEditAsync(model, cancellationToken);
            if (!updated)
                return RedirectToAction(nameof(List));

            TempData["SuccessMessage"] = await _localizationService.GetResourceAsync("admin.plant.notifications.edit.success");
            return RedirectToAction(nameof(Edit), new { id = model.Id });
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);

            var hydratedModel = await _plantModelFactory.PrepareEditModelAsync(model.Id, cancellationToken)
                ?? await _plantModelFactory.PrepareCreateModelAsync(cancellationToken);

            MergePostedValues(hydratedModel, model);

            return View(hydratedModel);
        }
    }

    /// <summary>
    /// Deletes a Plant catalog entry.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _plantService.DeletePlantAsync(id, cancellationToken);
            TempData["SuccessMessage"] = await _localizationService.GetResourceAsync("admin.plant.notifications.delete.success");
        }
        catch (DomainException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(List));
    }

    #endregion

    #region Privates

    private static void MergePostedValues(PlantModel target, PlantModel source)
    {
        target.Id = source.Id;
        target.PictureId = source.PictureId;
        target.CommonName = source.CommonName;
        target.ScientificName = source.ScientificName;
        target.Family = source.Family;
        target.Category = source.Category;
        target.GrowthType = source.GrowthType;
        target.SunRequirement = source.SunRequirement;
        target.WaterRequirement = source.WaterRequirement;
        target.MinTempCelsius = source.MinTempCelsius;
        target.MaxTempCelsius = source.MaxTempCelsius;
        target.IsEdible = source.IsEdible;
        target.IsMedicinal = source.IsMedicinal;
        target.IsToxic = source.IsToxic;
        target.GeneralNotes = source.GeneralNotes;
    }

    private void HydratePictureIdFromForm(PlantModel model)
    {
        if (model.PictureId > 0)
            return;

        if (!TryGetPictureIdFromForm(Request.Form, out var pictureId))
            return;

        model.PictureId = pictureId;
    }

    private static bool TryGetPictureIdFromForm(IFormCollection form, out int pictureId)
    {
        pictureId = 0;

        if (TryParsePictureId(form, "PictureId", out pictureId))
            return true;

        // Defensive fallback for prefixed form keys (e.g. "Model.PictureId").
        foreach (var key in form.Keys)
        {
            if (!key.EndsWith(".PictureId", StringComparison.OrdinalIgnoreCase))
                continue;

            if (TryParsePictureId(form, key, out pictureId))
                return true;
        }

        return false;
    }

    private static bool TryParsePictureId(IFormCollection form, string key, out int pictureId)
    {
        pictureId = 0;

        if (!form.TryGetValue(key, out StringValues values))
            return false;

        var raw = values.ToString();
        return int.TryParse(raw, out pictureId) && pictureId > 0;
    }

    #endregion
}
