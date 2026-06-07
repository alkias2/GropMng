using GropMng.Core.Caching;
using GropMng.Core.Domain.Garden.Plants;

namespace GropMng.Services.Caching.Garden;

/// <summary>
/// Invalidates ingredient and soil mix caches when soil ingredients change.
/// </summary>
public sealed class SoilIngredientCacheEventConsumer : BaseCacheEventConsumer<SoilIngredient>
{
    public SoilIngredientCacheEventConsumer(IGropStaticCacheManager cacheManager)
        : base(cacheManager)
    {
    }

    protected override async Task ClearCacheAsync(SoilIngredient entity, EntityEventType eventType, CancellationToken cancellationToken)
    {
        await CacheManager.RemoveByPrefixAsync(SoilMixCacheDefaults.SoilIngredientPrefix);
        await CacheManager.RemoveByPrefixAsync(SoilMixCacheDefaults.SoilMixPrefix);
    }
}