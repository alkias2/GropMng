using GropMng.Core.Caching;
using GropMng.Core.Domain.Garden.Locations;

namespace GropMng.Services.Caching.Garden;

/// <summary>
/// Invalidates related caches when garden spots change.
/// </summary>
public sealed class GardenSpotCacheEventConsumer : BaseCacheEventConsumer<GardenSpot>
{
    public GardenSpotCacheEventConsumer(IGropStaticCacheManager cacheManager)
        : base(cacheManager)
    {
    }

    protected override async Task ClearCacheAsync(GardenSpot entity, EntityEventType eventType, CancellationToken cancellationToken)
    {
        await CacheManager.RemoveByPrefixAsync(LocationCacheDefaults.GardenSpotPrefix);
        await CacheManager.RemoveByPrefixAsync(GropCacheDefaults.DashboardPrefix);
    }
}