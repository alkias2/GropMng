using GropMng.Core.Domain.Localization;

namespace GropMng.Core.Interfaces.Services.Localization;

/// <summary>
/// Provides language management and language resolution operations.
/// </summary>
public interface ILanguageService
{
    /// <summary>
    /// Gets all languages ordered by display order.
    /// </summary>
    /// <param name="showHidden">A value indicating whether unpublished languages should be returned.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of languages.</returns>
    Task<IReadOnlyList<Language>> GetAllLanguagesAsync(bool showHidden = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a language by identifier.
    /// </summary>
    /// <param name="languageId">The language identifier.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The language when found; otherwise, <see langword="null"/>.</returns>
    Task<Language?> GetLanguageByIdAsync(int languageId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the default language for the application.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The default published language.</returns>
    Task<Language> GetDefaultLanguageAsync(CancellationToken cancellationToken = default);
   
}
