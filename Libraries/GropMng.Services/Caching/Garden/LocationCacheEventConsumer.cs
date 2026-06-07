using GropMng.Core.Caching;
using GropMng.Core.Domain.Garden.Locations;

namespace GropMng.Services.Caching.Garden;

/// <summary>
/// Invalidates related caches when locations change.
/// </summary>
public sealed class LocationCacheEventConsumer : BaseCacheEventConsumer<Location>
{
    public LocationCacheEventConsumer(IGropStaticCacheManager cacheManager)
        : base(cacheManager)
    {
    }

    protected override async Task ClearCacheAsync(Location entity, EntityEventType eventType, CancellationToken cancellationToken)
    {
        await CacheManager.RemoveByPrefixAsync(LocationCacheDefaults.LocationPrefix);
        await CacheManager.RemoveByPrefixAsync(LocationCacheDefaults.GardenSpotPrefix);
        await CacheManager.RemoveByPrefixAsync(GropCacheDefaults.DashboardPrefix);
    }
}