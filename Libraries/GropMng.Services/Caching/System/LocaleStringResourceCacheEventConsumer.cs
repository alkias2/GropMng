using GropMng.Core.Caching;
using GropMng.Core.Domain.Localization;

namespace GropMng.Services.Caching.System;

/// <summary>
/// Invalidates locale and dashboard caches when localization resources change.
/// </summary>
public sealed class LocaleStringResourceCacheEventConsumer : BaseCacheEventConsumer<LocaleStringResource>
{
    public LocaleStringResourceCacheEventConsumer(IGropStaticCacheManager cacheManager)
        : base(cacheManager)
    {
    }

    protected override async Task ClearCacheAsync(LocaleStringResource entity, EntityEventType eventType, CancellationToken cancellationToken)
    {
        await CacheManager.RemoveByPrefixAsync(SystemCacheDefaults.LocalePrefix);
        await CacheManager.RemoveByPrefixAsync(GropCacheDefaults.DashboardPrefix);
    }
}