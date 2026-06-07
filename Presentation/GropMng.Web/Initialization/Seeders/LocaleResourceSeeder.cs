using GropMng.Core.Domain.Localization;
using GropMng.Data.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace GropMng.Web.Initialization.Seeders;

/// <summary>
/// Seeds baseline locale resources required by the current startup flow.
/// </summary>
internal sealed class LocaleResourceSeeder
{
    private static readonly string[] DeprecatedAppLogLevelKeys =
    [
        "admin.applog.level.trace",
        "admin.applog.level.debug",
        "admin.applog.level.information",
        "admin.applog.level.warning",
        "admin.applog.level.error",
        "admin.applog.level.critical"
    ];

    private readonly GropContext _dbContext;
    private readonly IWebHostEnvironment _environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocaleResourceSeeder"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="environment">The web hosting environment used to resolve locale resource file paths.</param>
    public LocaleResourceSeeder(GropContext dbContext, IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _environment = environment;
    }

    /// <summary>
    /// Removes the predefined deprecated app log level resources for the specified language.
    /// </summary>
    /// <param name="languageId">The language identifier.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    public Task RemoveDeprecatedAppLogLevelResourcesAsync(int languageId, CancellationToken cancellationToken = default)
        => RemoveLocaleResourcesAsync(languageId, DeprecatedAppLogLevelKeys, cancellationToken);

    /// <summary>
    /// Seeds the baseline UI locale resources for the specified language.
    /// by loading them from the corresponding XML file in AppData/Localization.
    /// </summary>
    /// <param name="languageSeoCode">The language SEO code.</param>
    /// <param name="languageId">The language identifier.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous seed operation.</returns>
    public async Task SeedDefaultResourcesAsync(string languageSeoCode, int languageId, CancellationToken cancellationToken = default)
    {
        var resources = await LoadResourcesFromXmlAsync(languageSeoCode, cancellationToken);
        await SeedResourcesAsync(languageId, resources, cancellationToken);
    }

    /// <summary>
    /// Synchronizes locale resources for the specified language with the provided key/value set.
    /// New resources are inserted and existing resources are updated when values differ.
    /// </summary>
    /// <param name="languageId">The language identifier.</param>
    /// <param name="resources">The resources to seed.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous seed operation.</returns>
    public async Task SeedResourcesAsync(int languageId, IReadOnlyDictionary<string, string> resources, CancellationToken cancellationToken = default)
    {
        var existingResources = await _dbContext.LocaleStringResources
            .Where(entity => entity.LanguageId == languageId)
            .ToListAsync(cancellationToken);

        var existingMap = existingResources
            .GroupBy(entity => entity.ResourceName.Trim().ToLowerInvariant())
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        var now = DateTime.UtcNow;
        foreach (var resource in resources)
        {
            var key = resource.Key.Trim().ToLowerInvariant();
            var value = resource.Value;

            if (existingMap.TryGetValue(key, out var existing))
            {
                if (!string.Equals(existing.ResourceValue ?? string.Empty, value, StringComparison.Ordinal))
                {
                    existing.ResourceValue = value;
                    existing.UpdatedOnUtc = now;
                }

                continue;
            }

            _dbContext.LocaleStringResources.Add(new LocaleStringResource
            {
                LanguageId = languageId,
                ResourceName = key,
                ResourceValue = value,
                CreatedOnUtc = now,
                UpdatedOnUtc = now
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task RemoveLocaleResourcesAsync(int languageId, IReadOnlyCollection<string> resourceKeys, CancellationToken cancellationToken)
    {
        if (resourceKeys.Count == 0)
            return;

        var normalizedKeys = resourceKeys
            .Select(key => key.Trim().ToLowerInvariant())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var resourcesToDelete = await _dbContext.LocaleStringResources
            .Where(entity => entity.LanguageId == languageId)
            .Where(entity => normalizedKeys.Contains(entity.ResourceName.ToLower()))
            .ToListAsync(cancellationToken);

        if (resourcesToDelete.Count == 0)
            return;

        _dbContext.LocaleStringResources.RemoveRange(resourcesToDelete);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Loads locale resources for the specified language from the corresponding XML file in AppData/Localization.
    /// </summary>
    /// <param name="languageSeoCode">The language SEO code.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A read-only dictionary of resource keys and values, or an empty dictionary if the file does not exist.</returns>
    private async Task<IReadOnlyDictionary<string, string>> LoadResourcesFromXmlAsync(string languageSeoCode, CancellationToken cancellationToken)
    {
        var filePath = Path.Combine(
            _environment.ContentRootPath,
            "AppData",
            "Localization",
            $"LocaleResources.{languageSeoCode}.xml");

        if (!File.Exists(filePath))
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        await using var stream = File.OpenRead(filePath);
        var document = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);

        return document.Root?
            .Elements("LocaleResource")
            .Select(el => (
                Name: el.Element("Name")?.Value,
                Value: el.Element("Value")?.Value ?? string.Empty))
            .Where(r => !string.IsNullOrWhiteSpace(r.Name))
            .ToDictionary(r => r.Name!, r => r.Value, StringComparer.OrdinalIgnoreCase)
            ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }
}
