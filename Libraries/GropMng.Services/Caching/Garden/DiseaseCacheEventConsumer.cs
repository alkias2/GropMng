using GropMng.Core.Caching;
using GropMng.Core.Domain.Garden.Health;

namespace GropMng.Services.Caching.Garden;

/// <summary>
/// Invalidates disease-related caches when disease definitions change.
/// </summary>
public sealed class DiseaseCacheEventConsumer : BaseCacheEventConsumer<Disease>
{
    public DiseaseCacheEventConsumer(IGropStaticCacheManager cacheManager)
        : base(cacheManager)
    {
    }

    protected override async Task ClearCacheAsync(Disease entity, EntityEventType eventType, CancellationToken cancellationToken)
    {
        await CacheManager.RemoveByPrefixAsync(DiseaseCacheDefaults.DiseasePrefix);
        await CacheManager.RemoveByPrefixAsync(DiseaseCacheDefaults.PlantDiseaseRecordPrefix);
        await CacheManager.RemoveByPrefixAsync(GropCacheDefaults.DashboardPrefix);
    }
}