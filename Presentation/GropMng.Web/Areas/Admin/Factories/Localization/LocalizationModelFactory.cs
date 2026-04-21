using AutoMapper;
using GropMng.Core.Domain.Localization;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.Localization;
using GropMng.Web.Areas.Admin.Models.Localization;
using GropMng.Web.Framework.Models.Extensions;

namespace GropMng.Web.Areas.Admin.Factories.Localization;

/// <summary>
/// Default implementation of <see cref="ILocalizationModelFactory"/>.
/// Prepares search and list models for the Localization admin area.
/// Delegates data access to repositories and the localization service.
/// </summary>
public class LocalizationModelFactory : ILocalizationModelFactory
{
    private readonly IRepository<Language> _languageRepository;
    private readonly IRepository<LocaleStringResource> _resourceRepository;
    private readonly ILocalizationService _localizationService;
    private readonly ILanguageService _languageService;
    private readonly IMapper _mapper;

    public LocalizationModelFactory(
        IRepository<Language> languageRepository,
        IRepository<LocaleStringResource> resourceRepository,
        ILocalizationService localizationService,
        ILanguageService languageService,
        IMapper mapper)
    {
        _languageRepository = languageRepository;
        _resourceRepository = resourceRepository;
        _localizationService = localizationService;
        _languageService = languageService;
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<LanguageSearchModel> PrepareLanguageSearchModelAsync(
        LanguageSearchModel? searchModel = null,
        CancellationToken cancellationToken = default)
    {
        searchModel ??= new LanguageSearchModel();
        searchModel.SetGridPageSize();

        return await Task.FromResult(searchModel);
    }

    /// <inheritdoc />
    public async Task<LanguageListModel> PrepareLanguageListModelAsync(
        LanguageSearchModel searchModel,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        // Repository expects 0-based page index
        var pageIndex = searchModel.Page - 1;

        var languages = await _languageRepository.GetPagedAsync(
            query =>
            {
                // Apply name filter if provided
                if (!string.IsNullOrWhiteSpace(searchModel.Name))
                {
                    var lowerName = searchModel.Name.Trim().ToLowerInvariant();
                    query = query.Where(x => x.Name.ToLower().Contains(lowerName));
                }

                // Apply published filter if provided
                if (searchModel.Published.HasValue)
                {
                    query = query.Where(x => x.Published == searchModel.Published.Value);
                }

                // Order by display order and then by ID
                query = query.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Id);

                return query;
            },
            pageIndex,
            searchModel.PageSize,
            cancellationToken: cancellationToken);

        var mappedRows = languages.Select(x => _mapper.Map<LanguageRowModel>(x)).ToList();

        var listModel = new LanguageListModel();
        return listModel.PrepareToGrid(searchModel, languages, () => mappedRows);
    }

    /// <inheritdoc />
    public async Task<LocaleResourceSearchModel> PrepareLocaleResourceSearchModelAsync(
        int languageId,
        LocaleResourceSearchModel? searchModel = null,
        CancellationToken cancellationToken = default)
    {
        searchModel ??= new LocaleResourceSearchModel();
        searchModel.LanguageId = languageId;
        searchModel.SetGridPageSize();

        return await Task.FromResult(searchModel);
    }

    /// <inheritdoc />
    public async Task<LocaleResourceListModel> PrepareLocaleResourceListModelAsync(
        LocaleResourceSearchModel searchModel,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        // Repository expects 0-based page index
        var pageIndex = searchModel.Page - 1;

        var resources = await _resourceRepository.GetPagedAsync(
            query =>
            {
                // Filter by language ID
                query = query.Where(x => x.LanguageId == searchModel.LanguageId);

                // Apply resource name filter if provided
                if (!string.IsNullOrWhiteSpace(searchModel.ResourceName))
                {
                    var lowerName = searchModel.ResourceName.Trim().ToLowerInvariant();
                    query = query.Where(x => x.ResourceName.ToLower().Contains(lowerName));
                }

                // Order by resource name and then by ID
                query = query.OrderBy(x => x.ResourceName).ThenBy(x => x.Id);

                return query;
            },
            pageIndex,
            searchModel.PageSize,
            cancellationToken: cancellationToken);

        var mappedRows = resources.Select(x => _mapper.Map<LocaleResourceRowModel>(x)).ToList();

        var listModel = new LocaleResourceListModel();
        return listModel.PrepareToGrid(searchModel, resources, () => mappedRows);
    }

    /// <inheritdoc />
    public async Task<LanguageModel> PrepareLanguageCreateModelAsync(
        CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(new LanguageModel());
    }

    /// <inheritdoc />
    public async Task<LanguageModel?> PrepareLanguageEditModelAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var language = await _languageRepository.GetByIdAsync(id, cancellationToken: cancellationToken);
        if (language is null)
            return null;

        return new LanguageModel
        {
            Id = language.Id,
            Name = language.Name,
            LanguageCulture = language.LanguageCulture,
            UniqueSeoCode = language.UniqueSeoCode,
            FlagImageFileName = language.FlagImageFileName,
            Rtl = language.Rtl,
            Published = language.Published,
            DisplayOrder = language.DisplayOrder
        };
    }

    /// <inheritdoc />
    public async Task SaveLanguageCreateAsync(
        LanguageModel model,
        CancellationToken cancellationToken = default)
    {
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
    }

    /// <inheritdoc />
    public async Task SaveLanguageEditAsync(
        LanguageModel model,
        CancellationToken cancellationToken = default)
    {
        var language = await _languageRepository.GetByIdAsync(model.Id, cancellationToken: cancellationToken);
        if (language is null)
            throw new InvalidOperationException($"Language with ID {model.Id} not found.");

        language.Name = model.Name.Trim();
        language.LanguageCulture = model.LanguageCulture.Trim();
        language.UniqueSeoCode = model.UniqueSeoCode.Trim().ToLowerInvariant();
        language.FlagImageFileName = string.IsNullOrWhiteSpace(model.FlagImageFileName) ? null : model.FlagImageFileName.Trim();
        language.Rtl = model.Rtl;
        language.Published = model.Published;
        language.DisplayOrder = model.DisplayOrder;
        language.UpdatedOnUtc = DateTime.UtcNow;

        await _languageRepository.UpdateAsync(language, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteLanguageAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var language = await _languageRepository.GetByIdAsync(id, cancellationToken: cancellationToken);
        if (language is null)
            throw new InvalidOperationException($"Language with ID {id} not found.");

        var hasResources = await _resourceRepository.AnyAsync(entity => entity.LanguageId == id, cancellationToken: cancellationToken);
        if (hasResources)
            throw new InvalidOperationException(await _localizationService.GetResourceAsync("admin.localization.language.delete.hasresources"));

        await _languageRepository.DeleteAsync(language, softDelete: false, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<string> GetLocalizationResourceAsync(
        string resourceKey,
        CancellationToken cancellationToken = default)
    {
        return await _localizationService.GetResourceAsync(resourceKey);
    }
}
