using GropMng.Core.Domain.Localization;
using System.IO;

namespace GropMng.Core.Interfaces.Services.Localization;

/// <summary>
/// Provides localization resource lookup and XML import-export operations.
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Gets a localized resource for the resolved working language.
    /// </summary>
    /// <param name="resourceKey">The resource key.</param>
    /// <returns>The localized value.</returns>
    Task<string> GetResourceAsync(string resourceKey);

    /// <summary>
    /// Gets a localized resource for a specific language.
    /// </summary>
    /// <param name="resourceKey">The resource key.</param>
    /// <param name="languageId">The language identifier.</param>
    /// <param name="logIfNotFound">A value indicating whether missing keys should be logged.</param>
    /// <param name="defaultValue">A fallback value when the key is missing.</param>
    /// <param name="returnEmptyIfNotFound">A value indicating whether an empty string should be returned when missing.</param>
    /// <returns>The localized value resolved with fallback rules.</returns>
    Task<string> GetResourceAsync(string resourceKey, int languageId, bool logIfNotFound = true, string defaultValue = "", bool returnEmptyIfNotFound = false);

    /// <summary>
    /// Gets all resources for a language.
    /// </summary>
    /// <param name="languageId">The language identifier.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A dictionary of resource keys and values.</returns>
    Task<IDictionary<string, string>> GetAllResourcesByLanguageAsync(int languageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new locale string resource.
    /// </summary>
    /// <param name="resource">The resource to create.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    Task InsertLocaleStringResourceAsync(LocaleStringResource resource, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing locale string resource.
    /// </summary>
    /// <param name="resource">The resource to update.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    Task UpdateLocaleStringResourceAsync(LocaleStringResource resource, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a locale string resource.
    /// </summary>
    /// <param name="resource">The resource to delete.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    Task DeleteLocaleStringResourceAsync(LocaleStringResource resource, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports resources for a language as XML.
    /// </summary>
    /// <param name="language">The source language.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A deterministic XML string.</returns>
    Task<string> ExportResourcesToXmlAsync(Language language, CancellationToken cancellationToken = default);

    /// <summary>
    /// Imports resources from XML for a language.
    /// </summary>
    /// <param name="language">The destination language.</param>
    /// <param name="xmlStreamReader">The XML stream reader.</param>
    /// <param name="updateExistingResources">A value indicating whether existing resources should be updated.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    Task ImportResourcesFromXmlAsync(Language language, StreamReader xmlStreamReader, bool updateExistingResources = true, CancellationToken cancellationToken = default);

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
    Task<string> GetLocalizedEnumAsync<TEnum>(TEnum enumValue, int? languageId = null) where TEnum : struct;
}
