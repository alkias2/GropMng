using System.Globalization;
using GropMng.Core.Domain.Localization;
using GropMng.Core.Interfaces.Services.Localization;

namespace GropMng.Services.Services.Localization;

/// <summary>
/// Resolves the current working language based on request UI culture.
/// </summary>
public class CurrentLanguageContext : ICurrentLanguageContext
{
    private readonly ILanguageService _languageService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrentLanguageContext"/> class.
    /// </summary>
    /// <param name="languageService">Language service.</param>
    public CurrentLanguageContext(ILanguageService languageService)
    {
        _languageService = languageService ?? throw new ArgumentNullException(nameof(languageService));
    }

    /// <inheritdoc />
    public async Task<Language> GetCurrentLanguageAsync(CancellationToken cancellationToken = default)
    {
        var currentCulture = CultureInfo.CurrentUICulture;
        var languages = await _languageService.GetAllLanguagesAsync(showHidden: false, cancellationToken: cancellationToken);

        var exactCultureMatch = languages.FirstOrDefault(x =>
            string.Equals(x.LanguageCulture, currentCulture.Name, StringComparison.OrdinalIgnoreCase));
        if (exactCultureMatch is not null)
            return exactCultureMatch;

        var twoLetterCode = currentCulture.TwoLetterISOLanguageName;
        var seoCodeMatch = languages.FirstOrDefault(x =>
            string.Equals(x.UniqueSeoCode, twoLetterCode, StringComparison.OrdinalIgnoreCase));
        if (seoCodeMatch is not null)
            return seoCodeMatch;

        return await _languageService.GetDefaultLanguageAsync(cancellationToken);
    }
}
