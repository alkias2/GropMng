using GropMng.Web.Areas.Admin.Models.Localization;

namespace GropMng.Web.Areas.Admin.Factories.Localization;

/// <summary>
/// Contract for preparing view models used in the Localization admin area.
/// </summary>
public interface ILocalizationModelFactory
{
    /// <summary>
    /// Prepares a <see cref="LanguageSearchModel"/> with initial search and paging configuration.
    /// Called by the Languages list page GET request to initialize the search view.
    /// </summary>
    /// <param name="searchModel">Optional pre-populated search model; if null, a new one is created.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An initialized <see cref="LanguageSearchModel"/> ready for view rendering.</returns>
    Task<LanguageSearchModel> PrepareLanguageSearchModelAsync(
        LanguageSearchModel? searchModel = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Prepares a <see cref="LanguageListModel"/> with paged language rows.
    /// Called by the Languages list page POST request to return server-side DataTables JSON.
    /// </summary>
    /// <param name="searchModel">The search model carrying paging and filter criteria.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A populated <see cref="LanguageListModel"/> ready for JSON serialisation.</returns>
    Task<LanguageListModel> PrepareLanguageListModelAsync(
        LanguageSearchModel searchModel,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Prepares a new <see cref="LanguageModel"/> for language creation.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An initialized <see cref="LanguageModel"/> ready for view rendering.</returns>
    Task<LanguageModel> PrepareLanguageCreateModelAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Prepares an existing <see cref="LanguageModel"/> for language editing.
    /// </summary>
    /// <param name="id">The language ID to edit.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A populated <see cref="LanguageModel"/>, or null if not found.</returns>
    Task<LanguageModel?> PrepareLanguageEditModelAsync(
        int id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a new language.
    /// </summary>
    /// <param name="model">The language model to save.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A completed task.</returns>
    Task SaveLanguageCreateAsync(
        LanguageModel model,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing language.
    /// </summary>
    /// <param name="model">The language model to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A completed task.</returns>
    Task SaveLanguageEditAsync(
        LanguageModel model,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a language by ID.
    /// </summary>
    /// <param name="id">The language ID to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A completed task.</returns>
    Task DeleteLanguageAsync(
        int id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Prepares a <see cref="LocaleResourceSearchModel"/> with initial search and paging configuration.
    /// Called by the Resources list page GET request to initialize the search view.
    /// </summary>
    /// <param name="languageId">The ID of the language for which to retrieve resources.</param>
    /// <param name="searchModel">Optional pre-populated search model; if null, a new one is created.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An initialized <see cref="LocaleResourceSearchModel"/> ready for view rendering.</returns>
    Task<LocaleResourceSearchModel> PrepareLocaleResourceSearchModelAsync(
        int languageId,
        LocaleResourceSearchModel? searchModel = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Prepares a <see cref="LocaleResourceListModel"/> with paged locale resource rows.
    /// Called by the Resources list page POST request to return server-side DataTables JSON.
    /// </summary>
    /// <param name="searchModel">The search model carrying language ID, paging, and filter criteria.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A populated <see cref="LocaleResourceListModel"/> ready for JSON serialisation.</returns>
    Task<LocaleResourceListModel> PrepareLocaleResourceListModelAsync(
        LocaleResourceSearchModel searchModel,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a localized resource string.
    /// </summary>
    /// <param name="resourceKey">The resource key to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The localized resource value.</returns>
    Task<string> GetLocalizationResourceAsync(
        string resourceKey,
        CancellationToken cancellationToken = default);
}
