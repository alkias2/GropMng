using GropMng.Core.Caching;
using GropMng.Core.Domain.Garden.Health;

namespace GropMng.Services.Caching.Garden;

/// <summary>
/// Invalidates related caches when plant disease records change.
/// </summary>
public sealed class PlantDiseaseRecordCacheEventConsumer : BaseCacheEventConsumer<PlantDiseaseRecord>
{
    public PlantDiseaseRecordCacheEventConsumer(IGropStaticCacheManager cacheManager)
        : base(cacheManager)
    {
    }

    protected override async Task ClearCacheAsync(PlantDiseaseRecord entity, EntityEventType eventType, CancellationToken cancellationToken)
    {
        await CacheManager.RemoveByPrefixAsync(DiseaseCacheDefaults.PlantDiseaseRecordPrefix);
        await CacheManager.RemoveByPrefixAsync(GropCacheDefaults.DashboardPrefix);
    }
}