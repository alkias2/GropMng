using GropMng.Core.Caching;

namespace GropMng.Services.Caching.Garden;

/// <summary>
/// Cache prefixes for soil mix and ingredient entities.
/// </summary>
public static class SoilMixCacheDefaults
{
    public static string SoilMixPrefix => "Grop.soil-mix.";

    public static string SoilIngredientPrefix => "Grop.soil-ingredient.";

    // -- Service-level keys

    public static GropCacheKey AllSoilMixesKey =>
        new("Grop.soil-mix.all.v1", SoilMixPrefix);

    public static GropCacheKey SoilMixByIdKey =>
        new("Grop.soil-mix.by-id.v1.{0}", SoilMixPrefix);
}
