using GropMng.Core.Common.Exceptions;
using GropMng.Core.Domain.Localization;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.Localization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;
using GropMng.Core;

namespace GropMng.Services.Services.Localization;

/// <summary>
/// Default implementation of localization resource operations.
/// </summary>
public class LocalizationService : ILocalizationService
{
    private const string ResourceCachePrefix = "grop.localization.";

    private readonly IRepository<LocaleStringResource> _resourceRepository;
    private readonly ILanguageService _languageService;
    private readonly ICurrentLanguageContext _currentLanguageContext;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<LocalizationService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationService"/> class.
    /// </summary>
    public LocalizationService(
        IRepository<LocaleStringResource> resourceRepository,
        ILanguageService languageService,
        ICurrentLanguageContext currentLanguageContext,
        IMemoryCache memoryCache,
        ILogger<LocalizationService> logger)
    {
        _resourceRepository = resourceRepository ?? throw new ArgumentNullException(nameof(resourceRepository));
        _languageService = languageService ?? throw new ArgumentNullException(nameof(languageService));
        _currentLanguageContext = currentLanguageContext ?? throw new ArgumentNullException(nameof(currentLanguageContext));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<string> GetResourceAsync(string resourceKey)
    {
        var currentLanguage = await _currentLanguageContext.GetCurrentLanguageAsync();
        if (currentLanguage is not null)
            return await GetResourceAsync(resourceKey, currentLanguage.Id);

        var defaultLanguage = await _languageService.GetDefaultLanguageAsync();
        return await GetResourceAsync(resourceKey, defaultLanguage.Id);
    }

    /// <inheritdoc />
    public async Task<string> GetResourceAsync(string resourceKey, int languageId, bool logIfNotFound = true, string defaultValue = "", bool returnEmptyIfNotFound = false)
    {
        if (string.IsNullOrWhiteSpace(resourceKey))
            return string.Empty;

        var normalizedKey = NormalizeKey(resourceKey);
        var resources = await GetAllResourcesByLanguageAsync(languageId);

        if (resources.TryGetValue(normalizedKey, out var value))
            return value;

        var defaultLanguage = await _languageService.GetDefaultLanguageAsync();
        if (defaultLanguage.Id != languageId)
        {
            var defaultResources = await GetAllResourcesByLanguageAsync(defaultLanguage.Id);
            if (defaultResources.TryGetValue(normalizedKey, out var defaultLanguageValue))
                return defaultLanguageValue;
        }

        if (logIfNotFound)
            _logger.LogWarning("Localization key not found: {ResourceKey} for language id {LanguageId}", normalizedKey, languageId);

        if (!string.IsNullOrEmpty(defaultValue))
            return defaultValue;

        return returnEmptyIfNotFound ? string.Empty : normalizedKey;
    }

    /// <inheritdoc />
    public async Task<IDictionary<string, string>> GetAllResourcesByLanguageAsync(int languageId, CancellationToken cancellationToken = default)
    {
        if (languageId <= 0)
            return new Dictionary<string, string>();

        var cacheKey = BuildCacheKey(languageId);
        if (_memoryCache.TryGetValue(cacheKey, out IDictionary<string, string>? cached) && cached is not null)
            return cached;

        var resources = await _resourceRepository.GetAllAsync(
            query => query
                .Where(resource => resource.LanguageId == languageId)
                .OrderBy(resource => resource.ResourceName),
            cancellationToken: cancellationToken);

        var dictionary = resources
            .GroupBy(resource => NormalizeKey(resource.ResourceName))
            .ToDictionary(group => group.Key, group => group.Last().ResourceValue ?? string.Empty);

        _memoryCache.Set(cacheKey, dictionary, TimeSpan.FromMinutes(30));
        return dictionary;
    }

    /// <inheritdoc />
    public async Task InsertLocaleStringResourceAsync(LocaleStringResource resource, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(resource);
        ValidateResource(resource);
        StampForCreate(resource);

        await _resourceRepository.CreateAsync(resource, cancellationToken: cancellationToken);
        InvalidateLanguageCache(resource.LanguageId);
    }

    /// <inheritdoc />
    public async Task UpdateLocaleStringResourceAsync(LocaleStringResource resource, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(resource);
        ValidateResource(resource);

        var existing = await _resourceRepository.GetByIdAsync(resource.Id, cancellationToken: cancellationToken)
            ?? throw new DomainException($"Locale string resource with id '{resource.Id}' was not found.");

        existing.ResourceName = NormalizeKey(resource.ResourceName);
        existing.ResourceValue = resource.ResourceValue;
        existing.UpdatedOnUtc = DateTime.UtcNow;

        await _resourceRepository.UpdateAsync(existing, cancellationToken: cancellationToken);
        InvalidateLanguageCache(existing.LanguageId);
    }

    /// <inheritdoc />
    public async Task DeleteLocaleStringResourceAsync(LocaleStringResource resource, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(resource);

        var existing = await _resourceRepository.GetByIdAsync(resource.Id, cancellationToken: cancellationToken)
            ?? throw new DomainException($"Locale string resource with id '{resource.Id}' was not found.");

        await _resourceRepository.DeleteAsync(existing, softDelete: false, cancellationToken: cancellationToken);
        InvalidateLanguageCache(existing.LanguageId);
    }

    /// <inheritdoc />
    public async Task<string> ExportResourcesToXmlAsync(Language language, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(language);

        var resources = await _resourceRepository.GetAllAsync(
            query => query
                .Where(resource => resource.LanguageId == language.Id)
                .OrderBy(resource => resource.ResourceName),
            cancellationToken: cancellationToken);

        var document = new XDocument(
            new XElement("Language",
                new XAttribute("Name", language.Name),
                new XAttribute("LanguageCulture", language.LanguageCulture),
                new XAttribute("UniqueSeoCode", language.UniqueSeoCode),
                resources.Select(resource =>
                    new XElement("LocaleResource",
                        new XElement("Name", resource.ResourceName),
                        new XElement("Value", resource.ResourceValue ?? string.Empty)))));

        return document.ToString(SaveOptions.DisableFormatting);
    }

    /// <inheritdoc />
    public async Task ImportResourcesFromXmlAsync(Language language, StreamReader xmlStreamReader, bool updateExistingResources = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(language);
        ArgumentNullException.ThrowIfNull(xmlStreamReader);

        var xml = await xmlStreamReader.ReadToEndAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(xml))
            return;

        var document = XDocument.Parse(xml, LoadOptions.PreserveWhitespace);
        var nodes = document.Root?.Elements("LocaleResource") ?? Enumerable.Empty<XElement>();
        var normalizedResources = new List<(string Name, string Value)>();

        foreach (var node in nodes)
        {
            var name = NormalizeKey(node.Element("Name")?.Value ?? string.Empty);
            var value = node.Element("Value")?.Value ?? string.Empty;

            if (string.IsNullOrWhiteSpace(name))
                continue;

            normalizedResources.Add((name, value));
        }

        if (normalizedResources.Count == 0)
            return;

        var existingResources = await _resourceRepository.GetAllAsync(
            query => query.Where(resource => resource.LanguageId == language.Id),
            cancellationToken: cancellationToken);

        var existingByName = existingResources.ToDictionary(
            resource => NormalizeKey(resource.ResourceName),
            resource => resource,
            StringComparer.OrdinalIgnoreCase);

        foreach (var item in normalizedResources)
        {
            if (!existingByName.TryGetValue(item.Name, out var existing))
            {
                await _resourceRepository.CreateAsync(new LocaleStringResource
                {
                    LanguageId = language.Id,
                    ResourceName = item.Name,
                    ResourceValue = item.Value,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow
                }, cancellationToken: cancellationToken);

                continue;
            }

            if (!updateExistingResources)
                continue;

            existing.ResourceValue = item.Value;
            existing.UpdatedOnUtc = DateTime.UtcNow;
            await _resourceRepository.UpdateAsync(existing, cancellationToken: cancellationToken);
        }

        InvalidateLanguageCache(language.Id);
    }

    /// <summary>
    /// Get localized value of enum
    /// </summary>
    /// <typeparam name="TEnum">Enum type</typeparam>
    /// <param name="enumValue">Enum value</param>
    /// <param name="languageId">Language identifier; pass null to use the current working language</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the localized value
    /// </returns>
    public virtual async Task<string> GetLocalizedEnumAsync<TEnum>(TEnum enumValue, int? languageId = null) where TEnum : struct
    {
        if (!typeof(TEnum).IsEnum)
            throw new ArgumentException("T must be an enumerated type");

        var resolvedLanguageId = languageId
            ?? (await _currentLanguageContext.GetCurrentLanguageAsync()).Id;

        var resourceName = $"Enums.{typeof(TEnum)}.{enumValue}";
        var result = await GetResourceAsync(resourceName, resolvedLanguageId, false, string.Empty, true);

        //set default value if required
        if (string.IsNullOrEmpty(result))
            result = CommonHelper.ConvertEnum(enumValue.ToString());

        return result;
    }

    private static string NormalizeKey(string resourceKey)
    {
        return resourceKey.Trim().ToLowerInvariant();
    }

    private static string BuildCacheKey(int languageId)
    {
        return $"{ResourceCachePrefix}{languageId}";
    }

    private static void ValidateResource(LocaleStringResource resource)
    {
        if (resource.LanguageId <= 0)
            throw new DomainException("LanguageId is required.");

        if (string.IsNullOrWhiteSpace(resource.ResourceName))
            throw new DomainException("ResourceName is required.");

        resource.ResourceName = NormalizeKey(resource.ResourceName);
        resource.ResourceValue ??= string.Empty;
    }

    private static void StampForCreate(LocaleStringResource resource)
    {
        resource.ResourceName = NormalizeKey(resource.ResourceName);

        var now = DateTime.UtcNow;
        resource.CreatedOnUtc = now;
        resource.UpdatedOnUtc = now;
    }

    private void InvalidateLanguageCache(int languageId)
    {
        _memoryCache.Remove(BuildCacheKey(languageId));
    }


}
