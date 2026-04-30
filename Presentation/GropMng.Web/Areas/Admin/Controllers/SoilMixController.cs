using GropMng.Core.Common.Exceptions;
using GropMng.Core.Interfaces.Services.Garden.Plants;
using GropMng.Core.Interfaces.Services.Localization;
using GropMng.Web.Areas.Admin.Factories.SoilMix;
using GropMng.Web.Areas.Admin.Models.SoilMix;
using GropMng.Web.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;

namespace GropMng.Web.Areas.Admin.Controllers;

/// <summary>
/// Administers soil mix catalog entries and their ingredient rows in the Admin area.
/// </summary>
[Area("Admin")]
[AuthorizeAdmin]
[CheckPermission(GropMngPermissions.Garden.ManageReferenceData)]
public class SoilMixController : Controller
{
    private readonly ISoilMixModelFactory _soilMixModelFactory;
    private readonly ISoilMixService _soilMixService;
    private readonly ILocalizationService _localizationService;

    public SoilMixController(
        ISoilMixModelFactory soilMixModelFactory,
        ISoilMixService soilMixService,
        ILocalizationService localizationService)
    {
        _soilMixModelFactory = soilMixModelFactory;
        _soilMixService = soilMixService;
        _localizationService = localizationService;
    }

    [HttpGet]
    public IActionResult Index() => RedirectToAction(nameof(List));

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var searchModel = await _soilMixModelFactory.PrepareSearchModelAsync(cancellationToken: cancellationToken);
        return View(searchModel);
    }

    [HttpPost]
    public async Task<IActionResult> SoilMixList([FromForm] SoilMixSearchModel searchModel, CancellationToken cancellationToken)
    {
        var listModel = await _soilMixModelFactory.PrepareListModelAsync(searchModel, cancellationToken);
        return Json(listModel);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = await _soilMixModelFactory.PrepareCreateModelAsync(cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SoilMixModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var prepared = await _soilMixModelFactory.PrepareCreateModelAsync(cancellationToken);
            MergePostedValues(prepared, model);
            return View(prepared);
        }

        try
        {
            await _soilMixModelFactory.SaveCreateAsync(model, cancellationToken);
            TempData["SuccessMessage"] = await _localizationService.GetResourceAsync("admin.soilmix.notifications.create.success");
            return RedirectToAction(nameof(List));
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var prepared = await _soilMixModelFactory.PrepareCreateModelAsync(cancellationToken);
            MergePostedValues(prepared, model);
            return View(prepared);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var model = await _soilMixModelFactory.PrepareEditModelAsync(id, cancellationToken);
        if (model == null)
            return RedirectToAction(nameof(List));

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(SoilMixModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var hydrated = await _soilMixModelFactory.PrepareEditModelAsync(model.Id, cancellationToken)
                ?? await _soilMixModelFactory.PrepareCreateModelAsync(cancellationToken);
            MergePostedValues(hydrated, model);
            return View(hydrated);
        }

        try
        {
            var updated = await _soilMixModelFactory.SaveEditAsync(model, cancellationToken);
            if (!updated)
                return RedirectToAction(nameof(List));

            TempData["SuccessMessage"] = await _localizationService.GetResourceAsync("admin.soilmix.notifications.edit.success");
            return RedirectToAction(nameof(Edit), new { id = model.Id });
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var hydrated = await _soilMixModelFactory.PrepareEditModelAsync(model.Id, cancellationToken)
                ?? await _soilMixModelFactory.PrepareCreateModelAsync(cancellationToken);
            MergePostedValues(hydrated, model);
            return View(hydrated);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _soilMixService.DeleteSoilMixAsync(id, cancellationToken);
            TempData["SuccessMessage"] = await _localizationService.GetResourceAsync("admin.soilmix.notifications.delete.success");
        }
        catch (DomainException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(List));
    }

    [HttpGet]
    public async Task<IActionResult> IngredientList(int soilMixId, CancellationToken cancellationToken)
    {
        var rows = await _soilMixModelFactory.PrepareIngredientRowsAsync(soilMixId, cancellationToken);
        return Json(new { data = rows });
    }

    [HttpGet]
    public async Task<IActionResult> AddIngredientForm(int soilMixId, CancellationToken cancellationToken)
    {
        var model = await _soilMixModelFactory.PrepareIngredientCreateModelAsync(soilMixId, cancellationToken);
        return PartialView("_IngredientForm", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddIngredient(SoilMixIngredientModel model, CancellationToken cancellationToken)
    {
        // Manual cross-field validation depending on mode
        if (model.IsNew)
        {
            if (string.IsNullOrWhiteSpace(model.NewIngredientName))
                ModelState.AddModelError(nameof(model.NewIngredientName), "Ingredient name is required.");
        }
        else
        {
            if (model.SoilIngredientId <= 0)
                ModelState.AddModelError(nameof(model.SoilIngredientId), "Please select an ingredient.");
        }

        if (model.PercentageByVolume <= 0)
            ModelState.AddModelError(nameof(model.PercentageByVolume), "Percentage must be greater than 0.");

        if (!ModelState.IsValid)
            return Json(new { success = false, message = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)) });

        try
        {
            await _soilMixModelFactory.AddIngredientAsync(model, cancellationToken);
            return Json(new { success = true });
        }
        catch (DomainException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateIngredient([FromForm] int id, [FromForm] int soilMixId, [FromForm] decimal percentageByVolume, [FromForm] string? notes, CancellationToken cancellationToken)
    {
        if (percentageByVolume <= 0)
            return Json(new { success = false, message = "Percentage must be greater than 0." });

        try
        {
            await _soilMixModelFactory.UpdateIngredientAsync(soilMixId, id, percentageByVolume, notes, cancellationToken);
            return Json(new { success = true });
        }
        catch (DomainException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteIngredient(int id, int soilMixId, CancellationToken cancellationToken)
    {
        try
        {
            await _soilMixModelFactory.DeleteIngredientAsync(soilMixId, id, cancellationToken);
            return Json(new { success = true });
        }
        catch (DomainException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    private static void MergePostedValues(SoilMixModel target, SoilMixModel source)
    {
        target.Id = source.Id;
        target.Name = source.Name;
        target.Composition = source.Composition;
        target.PhMin = source.PhMin;
        target.PhMax = source.PhMax;
        target.Texture = source.Texture;
        target.Drainage = source.Drainage;
        target.Notes = source.Notes;
    }
}
