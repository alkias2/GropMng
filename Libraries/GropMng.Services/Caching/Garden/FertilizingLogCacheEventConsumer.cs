using GropMng.Core.Caching;
using GropMng.Core.Domain.Garden.Care;

namespace GropMng.Services.Caching.Garden;

/// <summary>
/// Invalidates dashboard and fertilizing log caches when fertilizing logs change.
/// </summary>
public sealed class FertilizingLogCacheEventConsumer : BaseCacheEventConsumer<FertilizingLog>
{
    public FertilizingLogCacheEventConsumer(IGropStaticCacheManager cacheManager)
        : base(cacheManager)
    {
    }

    protected override async Task ClearCacheAsync(FertilizingLog entity, EntityEventType eventType, CancellationToken cancellationToken)
    {
        await CacheManager.RemoveByPrefixAsync(FertilizingCacheDefaults.LogPrefix);
        await CacheManager.RemoveByPrefixAsync(GropCacheDefaults.DashboardPrefix);
    }
}