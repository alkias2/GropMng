using GropMng.Core.Caching;
using GropMng.Core.Domain.Garden.Plants;

namespace GropMng.Services.Caching.Garden;

/// <summary>
/// Invalidates soil mix caches when soil mixes change.
/// </summary>
public sealed class SoilMixCacheEventConsumer : BaseCacheEventConsumer<SoilMix>
{
    public SoilMixCacheEventConsumer(IGropStaticCacheManager cacheManager)
        : base(cacheManager)
    {
    }

    protected override async Task ClearCacheAsync(SoilMix entity, EntityEventType eventType, CancellationToken cancellationToken)
    {
        await CacheManager.RemoveByPrefixAsync(SoilMixCacheDefaults.SoilMixPrefix);
    }
}