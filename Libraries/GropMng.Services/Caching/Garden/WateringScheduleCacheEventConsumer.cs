using GropMng.Core.Caching;
using GropMng.Core.Domain.Garden.Care;

namespace GropMng.Services.Caching.Garden;

/// <summary>
/// Invalidates dashboard and watering schedule caches when watering schedules change.
/// </summary>
public sealed class WateringScheduleCacheEventConsumer : BaseCacheEventConsumer<WateringSchedule>
{
    public WateringScheduleCacheEventConsumer(IGropStaticCacheManager cacheManager)
        : base(cacheManager)
    {
    }

    protected override async Task ClearCacheAsync(WateringSchedule entity, EntityEventType eventType, CancellationToken cancellationToken)
    {
        await CacheManager.RemoveByPrefixAsync(WateringCacheDefaults.SchedulePrefix);
        await CacheManager.RemoveByPrefixAsync(GropCacheDefaults.DashboardPrefix);
    }
}