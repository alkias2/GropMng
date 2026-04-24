using FluentValidation;
using GropMng.Web.Areas.Admin.Models.Localization;
using GropMng.Web.Areas.Admin.Factories.Localization;
using GropMng.Web.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;

namespace GropMng.Web.Areas.Admin.Controllers;

/// <summary>
/// Administers languages and locale resources.
/// Follows the thin-controller / factory pattern: delegates all model preparation
/// to <see cref="ILocalizationModelFactory"/> and only orchestrates HTTP concerns.
/// </summary>
[Area("Admin")]
[AuthorizeAdmin]
[CheckPermission(GropMngPermissions.Localization.ManageLocalization)]
public class LocalizationController : Controller
{
    private readonly ILocalizationModelFactory _factory;
    private readonly IValidator<LanguageSearchModel> _languageSearchValidator;
    private readonly IValidator<LocaleResourceSearchModel> _localeResourceSearchValidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationController"/> class.
    /// </summary>
    public LocalizationController(
        ILocalizationModelFactory factory,
        IValidator<LanguageSearchModel> languageSearchValidator,
        IValidator<LocaleResourceSearchModel> localeResourceSearchValidator)
    {
        _factory = factory;
        _languageSearchValidator = languageSearchValidator;
        _localeResourceSearchValidator = localeResourceSearchValidator;
    }

    /// <summary>
    /// Redirects the legacy index route to the canonical list page.
    /// </summary>
    [HttpGet]
    public IActionResult Index()
        => RedirectToAction(nameof(List));

    /// <summary>
    /// Renders the Languages list page with an initialised search model.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var searchModel = await _factory.PrepareLanguageSearchModelAsync(cancellationToken: cancellationToken);
        return View(searchModel);
    }

    /// <summary>
    /// Returns paged, filtered language rows in the DataTables server-side JSON format.
    /// DataTables posts draw/start/length plus any custom filter fields defined in
    /// <see cref="LanguageSearchModel"/>.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> LanguageList([FromForm] LanguageSearchModel searchModel, CancellationToken cancellationToken)
    {
        var validationResult = await _languageSearchValidator.ValidateAsync(searchModel, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.ErrorMessage).ToArray());

            return BadRequest(new { errors });
        }

        var listModel = await _factory.PrepareLanguageListModelAsync(searchModel, cancellationToken);
        return Json(listModel);
    }

    /// <summary>
    /// Renders the language create page.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var model = await _factory.PrepareLanguageCreateModelAsync(cancellationToken);
        return View(model);
    }

    /// <summary>
    /// Creates a new language.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LanguageModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var preparedModel = await _factory.PrepareLanguageCreateModelAsync(cancellationToken);
            MergePostedValues(preparedModel, model);
            return View(preparedModel);
        }

        try
        {
            await _factory.SaveLanguageCreateAsync(model, cancellationToken);
            TempData["SuccessMessage"] = await _factory.GetLocalizationResourceAsync("admin.localization.language.create.success");
            return RedirectToAction(nameof(List));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var preparedModel = await _factory.PrepareLanguageCreateModelAsync(cancellationToken);
            MergePostedValues(preparedModel, model);
            return View(preparedModel);
        }
    }

    /// <summary>
    /// Renders the language edit page.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var model = await _factory.PrepareLanguageEditModelAsync(id, cancellationToken);
        if (model is null)
            return RedirectToAction(nameof(List));

        return View(model);
    }

    /// <summary>
    /// Updates an existing language.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(LanguageModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var hydratedModel = await _factory.PrepareLanguageEditModelAsync(model.Id, cancellationToken) ?? 
                                await _factory.PrepareLanguageCreateModelAsync(cancellationToken);
            MergePostedValues(hydratedModel, model);
            return View(hydratedModel);
        }

        try
        {
            await _factory.SaveLanguageEditAsync(model, cancellationToken);
            TempData["SuccessMessage"] = await _factory.GetLocalizationResourceAsync("admin.localization.language.edit.success");
            return RedirectToAction(nameof(Edit), new { id = model.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var hydratedModel = await _factory.PrepareLanguageEditModelAsync(model.Id, cancellationToken) ?? 
                                await _factory.PrepareLanguageCreateModelAsync(cancellationToken);
            MergePostedValues(hydratedModel, model);
            return View(hydratedModel);
        }
    }

    /// <summary>
    /// Deletes a language.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _factory.DeleteLanguageAsync(id, cancellationToken);
            TempData["SuccessMessage"] = await _factory.GetLocalizationResourceAsync("admin.localization.language.delete.success");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(List));
    }

    /// <summary>
    /// Shows locale resources for a language.
    /// The route segment maps to <c>id</c> via the default <c>{controller}/{action}/{id?}</c> template.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Resources(int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
            return RedirectToAction(nameof(List));

        var searchModel = await _factory.PrepareLocaleResourceSearchModelAsync(id, cancellationToken: cancellationToken);

        if (string.IsNullOrWhiteSpace(searchModel.LanguageName))
            return RedirectToAction(nameof(List));

        return View(searchModel);
    }

    /// <summary>
    /// Returns paged, filtered locale resource rows in the DataTables server-side JSON format.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> LocaleResourceList([FromForm] LocaleResourceSearchModel searchModel, CancellationToken cancellationToken)
    {
        var validationResult = await _localeResourceSearchValidator.ValidateAsync(searchModel, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.ErrorMessage).ToArray());

            return BadRequest(new { errors });
        }

        var listModel = await _factory.PrepareLocaleResourceListModelAsync(searchModel, cancellationToken);
        return Json(listModel);
    }

    /// <summary>
    /// Creates a new locale string resource inline from the Resources grid.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddLocaleResource([FromForm] LocaleResourceModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    x => x.Key,
                    x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

            return BadRequest(new { errors });
        }

        try
        {
            var row = await _factory.SaveLocaleResourceAddAsync(model, cancellationToken);
            return Json(new { success = true, data = row });
        }
        catch (Exception ex)
        {
            return BadRequest(new { errors = new Dictionary<string, string[]> { [""] = [ex.Message] } });
        }
    }

    /// <summary>
    /// Updates an existing locale string resource inline from the Resources grid.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateLocaleResource([FromForm] LocaleResourceModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    x => x.Key,
                    x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

            return BadRequest(new { errors });
        }

        try
        {
            await _factory.SaveLocaleResourceUpdateAsync(model, cancellationToken);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return BadRequest(new { errors = new Dictionary<string, string[]> { [""] = [ex.Message] } });
        }
    }

    /// <summary>
    /// Deletes a locale string resource from the Resources grid.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLocaleResource([FromForm] int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
            return BadRequest(new { errors = new Dictionary<string, string[]> { ["id"] = ["Invalid resource ID."] } });

        try
        {
            await _factory.DeleteLocaleResourceAsync(id, cancellationToken);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return BadRequest(new { errors = new Dictionary<string, string[]> { [""] = [ex.Message] } });
        }
    }

    private static void MergePostedValues(LanguageModel destination, LanguageModel source)
    {
        destination.Name = source.Name;
        destination.LanguageCulture = source.LanguageCulture;
        destination.UniqueSeoCode = source.UniqueSeoCode;
        destination.FlagImageFileName = source.FlagImageFileName;
        destination.Rtl = source.Rtl;
        destination.Published = source.Published;
        destination.DisplayOrder = source.DisplayOrder;
    }
}

