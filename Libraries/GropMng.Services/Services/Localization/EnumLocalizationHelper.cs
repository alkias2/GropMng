using System.Globalization;
using GropMng.Core.Interfaces.Services.Localization;

namespace GropMng.Services.Services.Localization;

/// <summary>
/// Resolves localized enum text using convention-based resource keys.
/// </summary>
public class EnumLocalizationHelper : IEnumLocalizationHelper
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly ILanguageService _languageService;

    #endregion

    #region Ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumLocalizationHelper"/> class.
    /// </summary>
    /// <param name="localizationService">Localization resource service.</param>
    /// <param name="languageService">Language resolution service.</param>
    public EnumLocalizationHelper(ILocalizationService localizationService, ILanguageService languageService)
    {
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _languageService = languageService ?? throw new ArgumentNullException(nameof(languageService));
    }

    #endregion

    #region Public Methods

    /// <inheritdoc />
    public async Task<string> GetLocalizedNameAsync<TEnum>(TEnum enumValue, int? languageId = null)
        where TEnum : struct, Enum
    {
        var resolvedLanguageId = await ResolveLanguageIdAsync(languageId);
        var key = BuildResourceKey(enumValue);
        var localized = await _localizationService.GetResourceAsync(
            key,
            resolvedLanguageId,
            logIfNotFound: false,
            defaultValue: string.Empty,
            returnEmptyIfNotFound: true);

        return !string.IsNullOrWhiteSpace(localized) ? localized : enumValue.ToString();
    }

    #endregion

    #region Utilities

    private static string BuildResourceKey<TEnum>(TEnum enumValue)
        where TEnum : struct, Enum
    {
        return $"enum.{typeof(TEnum).Name.ToLowerInvariant()}.{enumValue.ToString().ToLowerInvariant()}";
    }

    private async Task<int> ResolveLanguageIdAsync(int? languageId)
    {
        if (languageId.HasValue && languageId.Value > 0)
            return languageId.Value;

        var currentCulture = CultureInfo.CurrentUICulture;
        var languages = await _languageService.GetAllLanguagesAsync(showHidden: false);

        var exactCultureMatch = languages.FirstOrDefault(x =>
            string.Equals(x.LanguageCulture, currentCulture.Name, StringComparison.OrdinalIgnoreCase));
        if (exactCultureMatch is not null)
            return exactCultureMatch.Id;

        var twoLetterCode = currentCulture.TwoLetterISOLanguageName;
        var seoCodeMatch = languages.FirstOrDefault(x =>
            string.Equals(x.UniqueSeoCode, twoLetterCode, StringComparison.OrdinalIgnoreCase));
        if (seoCodeMatch is not null)
            return seoCodeMatch.Id;

        var defaultLanguage = await _languageService.GetDefaultLanguageAsync();
        return defaultLanguage.Id;
    }

    #endregion
}