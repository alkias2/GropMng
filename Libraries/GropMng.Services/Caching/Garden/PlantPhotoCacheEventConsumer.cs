using GropMng.Core.Caching;
using GropMng.Core.Domain.Garden.Plants;

namespace GropMng.Services.Caching.Garden;

/// <summary>
/// Invalidates related caches when plant photos change.
/// </summary>
public sealed class PlantPhotoCacheEventConsumer : BaseCacheEventConsumer<PlantPhoto>
{
    public PlantPhotoCacheEventConsumer(IGropStaticCacheManager cacheManager)
        : base(cacheManager)
    {
    }

    protected override async Task ClearCacheAsync(PlantPhoto entity, EntityEventType eventType, CancellationToken cancellationToken)
    {
        await CacheManager.RemoveByPrefixAsync(PlantCacheDefaults.PlantPhotoPrefix);
        await CacheManager.RemoveByPrefixAsync(GropCacheDefaults.DashboardPrefix);
    }
}