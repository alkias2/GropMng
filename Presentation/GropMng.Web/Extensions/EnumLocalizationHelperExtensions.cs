using GropMng.Core.Interfaces.Services.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GropMng.Web.Extensions;

/// <summary>
/// Provides MVC-friendly helpers for building localized enum select lists.
/// </summary>
public static class EnumLocalizationHelperExtensions
{
    /// <summary>
    /// Builds a localized <see cref="SelectListItem"/> collection for all values of <typeparamref name="TEnum"/>.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <param name="enumLocalizationHelper">The enum localization helper.</param>
    /// <param name="selectedValue">Optional selected value.</param>
    /// <param name="emptyOptionText">Optional text for a leading empty option.</param>
    /// <returns>A localized select list for the enum type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="enumLocalizationHelper"/> is null.</exception>
    public static async Task<IList<SelectListItem>> ToLocalizedSelectListAsync<TEnum>(
        this IEnumLocalizationHelper enumLocalizationHelper,
        TEnum? selectedValue = null,
        string? emptyOptionText = null)
        where TEnum : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(enumLocalizationHelper);

        var items = new List<SelectListItem>();
        if (!string.IsNullOrWhiteSpace(emptyOptionText))
        {
            items.Add(new SelectListItem
            {
                Value = string.Empty,
                Text = emptyOptionText
            });
        }

        foreach (var enumValue in Enum.GetValues<TEnum>())
        {
            items.Add(new SelectListItem
            {
                Value = enumValue.ToString(),
                Text = await enumLocalizationHelper.GetLocalizedNameAsync(enumValue),
                Selected = selectedValue.HasValue && EqualityComparer<TEnum>.Default.Equals(enumValue, selectedValue.Value)
            });
        }

        return items;
    }
}