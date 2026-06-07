using GropMng.Core.Caching;
using GropMng.Core.Domain.Media;

namespace GropMng.Services.Caching.System;

/// <summary>
/// Invalidates picture and dashboard caches when pictures change.
/// </summary>
public sealed class PictureCacheEventConsumer : BaseCacheEventConsumer<Picture>
{
    public PictureCacheEventConsumer(IGropStaticCacheManager cacheManager)
        : base(cacheManager)
    {
    }

    protected override async Task ClearCacheAsync(Picture entity, EntityEventType eventType, CancellationToken cancellationToken)
    {
        await CacheManager.RemoveByPrefixAsync(SystemCacheDefaults.PicturePrefix);
        await CacheManager.RemoveByPrefixAsync(GropCacheDefaults.DashboardPrefix);
    }
}