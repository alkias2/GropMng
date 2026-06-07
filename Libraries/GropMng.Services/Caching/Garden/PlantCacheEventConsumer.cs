using GropMng.Core.Caching;
using GropMng.Core.Domain.Garden.Plants;

namespace GropMng.Services.Caching.Garden;

/// <summary>
/// Invalidates related caches when plant definitions change.
/// </summary>
public sealed class PlantCacheEventConsumer : BaseCacheEventConsumer<Plant>
{
    public PlantCacheEventConsumer(IGropStaticCacheManager cacheManager)
        : base(cacheManager)
    {
    }

    protected override async Task ClearCacheAsync(Plant entity, EntityEventType eventType, CancellationToken cancellationToken)
    {
        await CacheManager.RemoveByPrefixAsync(PlantCacheDefaults.PlantPrefix);
        await CacheManager.RemoveByPrefixAsync(PlantCacheDefaults.PlantInstancePrefix);
        await CacheManager.RemoveByPrefixAsync(GropCacheDefaults.DashboardPrefix);
    }
}