using GropMng.Core.Interfaces.Services.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GropMng.Web.Extensions;

/// <summary>
/// Provides convenience extensions on <see cref="ILocalizationService"/> for building
/// localized <see cref="SelectListItem"/> collections from enum values.
/// </summary>
public static class LocalizationServiceExtensions
{
    /// <summary>
    /// Builds a localized <see cref="SelectListItem"/> list for every value of <typeparamref name="TEnum"/>.
    /// Uses the resource key convention: <c>{keyPrefix}.{valueName.ToLowerInvariant()}</c>.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <param name="localizationService">The localization service.</param>
    /// <param name="keyPrefix">
    /// The resource key prefix applied to each enum value (e.g. <c>"admin.plant.category"</c>
    /// produces keys like <c>"admin.plant.category.shrub"</c>).
    /// </param>
    /// <param name="emptyOptionKey">
    /// Optional resource key for an empty "All / None" option prepended to the list.
    /// When <see langword="null"/>, no empty option is added.
    /// </param>
    /// <returns>A list of <see cref="SelectListItem"/> entries with localized display text.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="localizationService"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="keyPrefix"/> is null or whitespace.</exception>
    public static async Task<IList<SelectListItem>> GetLocalizedEnumSelectListAsync<TEnum>(
        this ILocalizationService localizationService,
        string keyPrefix,
        string? emptyOptionKey = null) where TEnum : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(localizationService);
        ArgumentException.ThrowIfNullOrWhiteSpace(keyPrefix);

        var items = new List<SelectListItem>();

        if (!string.IsNullOrWhiteSpace(emptyOptionKey))
        {
            items.Add(new SelectListItem
            {
                Value = string.Empty,
                Text = await localizationService.GetResourceAsync(emptyOptionKey)
            });
        }

        foreach (var value in Enum.GetValues<TEnum>())
        {
            var key = $"{keyPrefix}.{value.ToString().ToLowerInvariant()}";
            var text = await localizationService.GetResourceAsync(key);
            items.Add(new SelectListItem
            {
                Value = value.ToString(),
                Text = text
            });
        }

        return items;
    }
}
