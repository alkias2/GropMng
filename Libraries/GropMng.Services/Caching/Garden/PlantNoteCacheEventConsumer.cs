using GropMng.Core.Caching;
using GropMng.Core.Domain.Garden.Plants;

namespace GropMng.Services.Caching.Garden;

/// <summary>
/// Invalidates note caches when plant notes change.
/// </summary>
public sealed class PlantNoteCacheEventConsumer : BaseCacheEventConsumer<PlantNote>
{
    public PlantNoteCacheEventConsumer(IGropStaticCacheManager cacheManager)
        : base(cacheManager)
    {
    }

    protected override async Task ClearCacheAsync(PlantNote entity, EntityEventType eventType, CancellationToken cancellationToken)
    {
        await CacheManager.RemoveByPrefixAsync(PlantCacheDefaults.PlantNotePrefix);
    }
}