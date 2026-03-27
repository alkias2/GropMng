using GropMng.Core.Domain.Localization;

namespace GropMng.Core.Interfaces.Services.Localization;

/// <summary>
/// Resolves the current request language selected by the user.
/// </summary>
public interface ICurrentLanguageContext
{
    /// <summary>
    /// Gets the currently active language for the request.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The resolved language.</returns>
    Task<Language> GetCurrentLanguageAsync(CancellationToken cancellationToken = default);
}
