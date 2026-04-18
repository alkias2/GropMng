using GropMng.Core.Domain.Localization;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.Localization;
using GropMng.Web.Areas.Admin.Models.Localization;
using Microsoft.AspNetCore.Mvc;

namespace GropMng.Web.Areas.Admin.Controllers;

/// <summary>
/// Administers languages and locale resources.
/// </summary>
[Area("Admin")]
public class LocalizationController : Controller
{
    private readonly IRepository<Language> _languageRepository;
    private readonly IRepository<LocaleStringResource> _resourceRepository;
    private readonly ILocalizationService _localizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationController"/> class.
    /// </summary>
    public LocalizationController(
        IRepository<Language> languageRepository,
        IRepository<LocaleStringResource> resourceRepository,
        ILocalizationService localizationService)
    {
        _languageRepository = languageRepository;
        _resourceRepository = resourceRepository;
        _localizationService = localizationService;
    }

    /// <summary>
    /// Shows the language list.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Languages(CancellationToken cancellationToken)
    {
        ViewData["ActiveMenu"] = "Localization";

        var languages = await _languageRepository.GetAllAsync(
            query => query.OrderBy(entity => entity.DisplayOrder).ThenBy(entity => entity.Id),
            cancellationToken: cancellationToken);

        return View(languages);
    }

    /// <summary>
    /// Renders the language create page.
    /// </summary>
    [HttpGet]
    public IActionResult CreateLanguage()
    {
        ViewData["ActiveMenu"] = "Localization";
        return View(new LanguageModel());
    }

    /// <summary>
    /// Creates a new language.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateLanguage(LanguageModel model, CancellationToken cancellationToken)
    {
        ViewData["ActiveMenu"] = "Localization";

        if (!ModelState.IsValid)
            return View(model);

        var now = DateTime.UtcNow;
        await _languageRepository.CreateAsync(new Language
        {
            Name = model.Name.Trim(),
            LanguageCulture = model.LanguageCulture.Trim(),
            UniqueSeoCode = model.UniqueSeoCode.Trim().ToLowerInvariant(),
            FlagImageFileName = string.IsNullOrWhiteSpace(model.FlagImageFileName) ? null : model.FlagImageFileName.Trim(),
            Rtl = model.Rtl,
            Published = model.Published,
            DisplayOrder = model.DisplayOrder,
            CreatedOnUtc = now,
            UpdatedOnUtc = now
        }, cancellationToken: cancellationToken);

        TempData["SuccessMessage"] = await _localizationService.GetResourceAsync("admin.localization.language.create.success");
        return RedirectToAction(nameof(Languages));
    }

    /// <summary>
    /// Renders the language edit page.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> EditLanguage(int id, CancellationToken cancellationToken)
    {
        ViewData["ActiveMenu"] = "Localization";

        var language = await _languageRepository.GetByIdAsync(id, cancellationToken: cancellationToken);
        if (language is null)
            return RedirectToAction(nameof(Languages));

        return View(new LanguageModel
        {
            Id = language.Id,
            Name = language.Name,
            LanguageCulture = language.LanguageCulture,
            UniqueSeoCode = language.UniqueSeoCode,
            FlagImageFileName = language.FlagImageFileName,
            Rtl = language.Rtl,
            Published = language.Published,
            DisplayOrder = language.DisplayOrder
        });
    }

    /// <summary>
    /// Updates an existing language.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditLanguage(LanguageModel model, CancellationToken cancellationToken)
    {
        ViewData["ActiveMenu"] = "Localization";

        if (!ModelState.IsValid)
            return View(model);

        var language = await _languageRepository.GetByIdAsync(model.Id, cancellationToken: cancellationToken);
        if (language is null)
            return RedirectToAction(nameof(Languages));

        language.Name = model.Name.Trim();
        language.LanguageCulture = model.LanguageCulture.Trim();
        language.UniqueSeoCode = model.UniqueSeoCode.Trim().ToLowerInvariant();
        language.FlagImageFileName = string.IsNullOrWhiteSpace(model.FlagImageFileName) ? null : model.FlagImageFileName.Trim();
        language.Rtl = model.Rtl;
        language.Published = model.Published;
        language.DisplayOrder = model.DisplayOrder;
        language.UpdatedOnUtc = DateTime.UtcNow;

        await _languageRepository.UpdateAsync(language, cancellationToken: cancellationToken);

        TempData["SuccessMessage"] = await _localizationService.GetResourceAsync("admin.localization.language.edit.success");
        return RedirectToAction(nameof(EditLanguage), new { id = model.Id });
    }

    /// <summary>
    /// Deletes a language.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLanguage(int id, CancellationToken cancellationToken)
    {
        var language = await _languageRepository.GetByIdAsync(id, cancellationToken: cancellationToken);
        if (language is null)
            return RedirectToAction(nameof(Languages));

        var hasResources = await _resourceRepository.AnyAsync(entity => entity.LanguageId == id, cancellationToken: cancellationToken);
        if (hasResources)
        {
            TempData["ErrorMessage"] = await _localizationService.GetResourceAsync("admin.localization.language.delete.hasresources");
            return RedirectToAction(nameof(Languages));
        }

        await _languageRepository.DeleteAsync(language, softDelete: false, cancellationToken: cancellationToken);
        TempData["SuccessMessage"] = await _localizationService.GetResourceAsync("admin.localization.language.delete.success");

        return RedirectToAction(nameof(Languages));
    }

    /// <summary>
    /// Shows locale resources for a language.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Resources(int languageId, string? searchTerm, CancellationToken cancellationToken)
    {
        ViewData["ActiveMenu"] = "Localization";

        var language = await _languageRepository.GetByIdAsync(languageId, cancellationToken: cancellationToken);
        if (language is null)
            return RedirectToAction(nameof(Languages));

        var normalizedSearch = searchTerm?.Trim().ToLowerInvariant();

        var resources = await _resourceRepository.GetAllAsync(
            query =>
            {
                query = query.Where(entity => entity.LanguageId == languageId);

                if (!string.IsNullOrWhiteSpace(normalizedSearch))
                    query = query.Where(entity => entity.ResourceName.ToLower().Contains(normalizedSearch));

                return query.OrderBy(entity => entity.ResourceName).ThenBy(entity => entity.Id);
            },
            cancellationToken: cancellationToken);

        ViewData["Language"] = language;
        ViewData["SearchTerm"] = searchTerm ?? string.Empty;

        return View(resources);
    }

    /// <summary>
    /// Renders locale resource create page.
    /// </summary>
    [HttpGet]
    public IActionResult CreateResource(int languageId)
    {
        ViewData["ActiveMenu"] = "Localization";
        return View(new LocaleResourceModel { LanguageId = languageId });
    }

    /// <summary>
    /// Creates a locale resource.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateResource(LocaleResourceModel model, CancellationToken cancellationToken)
    {
        ViewData["ActiveMenu"] = "Localization";

        if (!ModelState.IsValid)
            return View(model);

        await _localizationService.InsertLocaleStringResourceAsync(new LocaleStringResource
        {
            LanguageId = model.LanguageId,
            ResourceName = model.ResourceName,
            ResourceValue = model.ResourceValue
        }, cancellationToken);

        TempData["SuccessMessage"] = await _localizationService.GetResourceAsync("admin.localization.resource.create.success");
        return RedirectToAction(nameof(Resources), new { languageId = model.LanguageId });
    }

    /// <summary>
    /// Renders locale resource edit page.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> EditResource(int id, CancellationToken cancellationToken)
    {
        ViewData["ActiveMenu"] = "Localization";

        var resource = await _resourceRepository.GetByIdAsync(id, cancellationToken: cancellationToken);
        if (resource is null)
            return RedirectToAction(nameof(Languages));

        return View(new LocaleResourceModel
        {
            Id = resource.Id,
            LanguageId = resource.LanguageId,
            ResourceName = resource.ResourceName,
            ResourceValue = resource.ResourceValue
        });
    }

    /// <summary>
    /// Updates a locale resource.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditResource(LocaleResourceModel model, CancellationToken cancellationToken)
    {
        ViewData["ActiveMenu"] = "Localization";

        if (!ModelState.IsValid)
            return View(model);

        await _localizationService.UpdateLocaleStringResourceAsync(new LocaleStringResource
        {
            Id = model.Id,
            LanguageId = model.LanguageId,
            ResourceName = model.ResourceName,
            ResourceValue = model.ResourceValue
        }, cancellationToken);

        TempData["SuccessMessage"] = await _localizationService.GetResourceAsync("admin.localization.resource.edit.success");
        return RedirectToAction(nameof(Resources), new { languageId = model.LanguageId });
    }

    /// <summary>
    /// Deletes a locale resource.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteResource(int id, int languageId, CancellationToken cancellationToken)
    {
        var resource = await _resourceRepository.GetByIdAsync(id, cancellationToken: cancellationToken);
        if (resource is not null)
            await _localizationService.DeleteLocaleStringResourceAsync(resource, cancellationToken);

        TempData["SuccessMessage"] = await _localizationService.GetResourceAsync("admin.localization.resource.delete.success");
        return RedirectToAction(nameof(Resources), new { languageId });
    }

    /// <summary>
    /// Exports resources as XML.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ExportResources(int languageId, CancellationToken cancellationToken)
    {
        var language = await _languageRepository.GetByIdAsync(languageId, cancellationToken: cancellationToken);
        if (language is null)
            return RedirectToAction(nameof(Languages));

        var xml = await _localizationService.ExportResourcesToXmlAsync(language, cancellationToken);
        var fileName = $"resources-{language.UniqueSeoCode}-{DateTime.UtcNow:yyyyMMddHHmmss}.xml";
        return File(System.Text.Encoding.UTF8.GetBytes(xml), "application/xml", fileName);
    }

    /// <summary>
    /// Imports resources from XML.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportResources(int languageId, IFormFile? xmlFile, bool updateExistingResources = true, CancellationToken cancellationToken = default)
    {
        if (xmlFile is null || xmlFile.Length == 0)
        {
            TempData["ErrorMessage"] = await _localizationService.GetResourceAsync("admin.localization.resource.import.empty");
            return RedirectToAction(nameof(Resources), new { languageId });
        }

        var language = await _languageRepository.GetByIdAsync(languageId, cancellationToken: cancellationToken);
        if (language is null)
            return RedirectToAction(nameof(Languages));

        await using var stream = xmlFile.OpenReadStream();
        using var reader = new StreamReader(stream);
        await _localizationService.ImportResourcesFromXmlAsync(language, reader, updateExistingResources, cancellationToken);

        TempData["SuccessMessage"] = await _localizationService.GetResourceAsync("admin.localization.resource.import.success");
        return RedirectToAction(nameof(Resources), new { languageId });
    }
}

