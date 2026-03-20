using FluentValidation;
using GropMng.Core.Common.Exceptions;
using GropMng.Core.Interfaces.Services.Garden.Plants;
using GropMng.Web.Areas.Admin.Models.Plant;
using GropMng.Web.Factories.Plant;
using Microsoft.AspNetCore.Mvc;

namespace GropMng.Web.Areas.Admin.Controllers;

/// <summary>
/// Administers plant catalog entities in the Admin area.
/// </summary>
[Area("Admin")]
public class PlantController : Controller
{
    #region Fields

    private readonly IPlantModelFactory _plantModelFactory;
    private readonly IPlantService _plantService;
    private readonly IValidator<PlantModel> _plantValidator;

    #endregion

    #region Ctor

    public PlantController(
        IPlantModelFactory plantModelFactory,
        IPlantService plantService,
        IValidator<PlantModel> plantValidator)
    {
        _plantModelFactory = plantModelFactory;
        _plantService = plantService;
        _plantValidator = plantValidator;
    }

    #endregion

    #region Public

    /// <summary>
    /// Renders the Plant index page.
    /// </summary>
    [HttpGet]
    public IActionResult Index()
    {
        var searchModel = _plantModelFactory.PrepareSearchModel();
        return View(searchModel);
    }

    /// <summary>
    /// Returns paged and filtered plant rows for DataTables.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> List([FromForm] PlantSearchModel searchModel, CancellationToken cancellationToken)
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
            TempData["SuccessMessage"] = "Plant was created successfully.";
            return RedirectToAction(nameof(Index));
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
            return RedirectToAction(nameof(Index));

        return View(model);
    }

    /// <summary>
    /// Updates an existing Plant catalog entry.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PlantModel model, CancellationToken cancellationToken)
    {
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
                return RedirectToAction(nameof(Index));

            TempData["SuccessMessage"] = "Plant was updated successfully.";
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
            TempData["SuccessMessage"] = "Plant was deleted successfully.";
        }
        catch (DomainException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Privates

    private static void MergePostedValues(PlantModel target, PlantModel source)
    {
        target.Id = source.Id;
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

    #endregion
}
