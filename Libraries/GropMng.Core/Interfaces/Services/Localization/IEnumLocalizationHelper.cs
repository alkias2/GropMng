namespace GropMng.Core.Interfaces.Services.Localization;

/// <summary>
/// Provides convention-based localization for enum values.
/// </summary>
public interface IEnumLocalizationHelper
{
    /// <summary>
    /// Gets a localized display value for the provided enum using the convention:
    /// <c>enum.{enumtype}.{member}</c> (lowercase).
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <param name="enumValue">The enum value to localize.</param>
    /// <param name="languageId">
    /// Optional language identifier. When <see langword="null"/>, the current request UI culture is resolved.
    /// </param>
    /// <returns>The localized value when found; otherwise the enum member name.</returns>
    Task<string> GetLocalizedNameAsync<TEnum>(TEnum enumValue, int? languageId = null)
        where TEnum : struct, Enum;
}