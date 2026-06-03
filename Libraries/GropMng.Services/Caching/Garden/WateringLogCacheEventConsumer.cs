using GropMng.Core.Caching;
using GropMng.Core.Domain.Garden.Care;

namespace GropMng.Services.Caching.Garden;

/// <summary>
/// Invalidates dashboard and watering log caches when watering logs change.
/// </summary>
public sealed class WateringLogCacheEventConsumer : BaseCacheEventConsumer<WateringLog>
{
    public WateringLogCacheEventConsumer(IGropStaticCacheManager cacheManager)
        : base(cacheManager)
    {
    }

    protected override async Task ClearCacheAsync(WateringLog entity, EntityEventType eventType, CancellationToken cancellationToken)
    {
        await CacheManager.RemoveByPrefixAsync(WateringCacheDefaults.LogPrefix);
        await CacheManager.RemoveByPrefixAsync(GropCacheDefaults.DashboardPrefix);
    }
}