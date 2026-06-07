using GropMng.Core.Caching;
using GropMng.Core.Domain.Garden.Plants;

namespace GropMng.Services.Caching.Garden;

/// <summary>
/// Invalidates related caches when plant instances change.
/// </summary>
public sealed class PlantInstanceCacheEventConsumer : BaseCacheEventConsumer<PlantInstance>
{
    public PlantInstanceCacheEventConsumer(IGropStaticCacheManager cacheManager)
        : base(cacheManager)
    {
    }

    protected override async Task ClearCacheAsync(PlantInstance entity, EntityEventType eventType, CancellationToken cancellationToken)
    {
        await CacheManager.RemoveByPrefixAsync(PlantCacheDefaults.PlantInstancePrefix);
        await CacheManager.RemoveByPrefixAsync(WateringCacheDefaults.SchedulePrefix);
        await CacheManager.RemoveByPrefixAsync(FertilizingCacheDefaults.SchedulePrefix);
        await CacheManager.RemoveByPrefixAsync(ActionSkipCacheDefaults.Prefix);
        await CacheManager.RemoveByPrefixAsync(GropCacheDefaults.DashboardPrefix);
    }
}