using GropMng.Core.Caching;
using GropMng.Core.Domain.Garden.Care;

namespace GropMng.Services.Caching.Garden;

/// <summary>
/// Invalidates dashboard and fertilizing schedule caches when fertilizing schedules change.
/// </summary>
public sealed class FertilizingScheduleCacheEventConsumer : BaseCacheEventConsumer<FertilizingSchedule>
{
    public FertilizingScheduleCacheEventConsumer(IGropStaticCacheManager cacheManager)
        : base(cacheManager)
    {
    }

    protected override async Task ClearCacheAsync(FertilizingSchedule entity, EntityEventType eventType, CancellationToken cancellationToken)
    {
        await CacheManager.RemoveByPrefixAsync(FertilizingCacheDefaults.SchedulePrefix);
        await CacheManager.RemoveByPrefixAsync(GropCacheDefaults.DashboardPrefix);
    }
}