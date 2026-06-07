using GropMng.Core.Caching;
using GropMng.Core.Domain.Garden.Care;

namespace GropMng.Services.Caching.Garden;

/// <summary>
/// Invalidates related caches when fertilizer master records change.
/// </summary>
public sealed class FertilizerCacheEventConsumer : BaseCacheEventConsumer<Fertilizer>
{
    public FertilizerCacheEventConsumer(IGropStaticCacheManager cacheManager)
        : base(cacheManager)
    {
    }

    protected override async Task ClearCacheAsync(Fertilizer entity, EntityEventType eventType, CancellationToken cancellationToken)
    {
        await CacheManager.RemoveByPrefixAsync(FertilizerCacheDefaults.Prefix);
        await CacheManager.RemoveByPrefixAsync(FertilizingCacheDefaults.SchedulePrefix);
        await CacheManager.RemoveByPrefixAsync(GropCacheDefaults.DashboardPrefix);
    }
}