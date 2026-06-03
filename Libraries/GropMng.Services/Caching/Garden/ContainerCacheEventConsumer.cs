using GropMng.Core.Caching;
using GropMng.Core.Domain.Garden.Plants;

namespace GropMng.Services.Caching.Garden;

/// <summary>
/// Invalidates container and dependent plant instance caches when containers change.
/// </summary>
public sealed class ContainerCacheEventConsumer : BaseCacheEventConsumer<Container>
{
    public ContainerCacheEventConsumer(IGropStaticCacheManager cacheManager)
        : base(cacheManager)
    {
    }

    protected override async Task ClearCacheAsync(Container entity, EntityEventType eventType, CancellationToken cancellationToken)
    {
        await CacheManager.RemoveByPrefixAsync(PlantCacheDefaults.ContainerPrefix);
        await CacheManager.RemoveByPrefixAsync(PlantCacheDefaults.PlantInstancePrefix);
        await CacheManager.RemoveByPrefixAsync(GropCacheDefaults.DashboardPrefix);
    }
}