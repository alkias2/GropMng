using GropMng.Core.Caching;
using GropMng.Core.Domain.Garden.Care;

namespace GropMng.Services.Caching.Garden;

/// <summary>
/// Invalidates dashboard and action skip caches when dashboard suppression changes.
/// </summary>
public sealed class ActionSkipCacheEventConsumer : BaseCacheEventConsumer<ActionSkip>
{
    public ActionSkipCacheEventConsumer(IGropStaticCacheManager cacheManager)
        : base(cacheManager)
    {
    }

    protected override async Task ClearCacheAsync(ActionSkip entity, EntityEventType eventType, CancellationToken cancellationToken)
    {
        await CacheManager.RemoveByPrefixAsync(ActionSkipCacheDefaults.Prefix);
        await CacheManager.RemoveByPrefixAsync(GropCacheDefaults.DashboardPrefix);
    }
}