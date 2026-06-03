using GropMng.Core.Caching;
using GropMng.Core.Domain.Localization;

namespace GropMng.Services.Caching.System;

/// <summary>
/// Invalidates language caches when languages change.
/// </summary>
public sealed class LanguageCacheEventConsumer : BaseCacheEventConsumer<Language>
{
    public LanguageCacheEventConsumer(IGropStaticCacheManager cacheManager)
        : base(cacheManager)
    {
    }

    protected override async Task ClearCacheAsync(Language entity, EntityEventType eventType, CancellationToken cancellationToken)
    {
        await CacheManager.RemoveByPrefixAsync(SystemCacheDefaults.LanguagePrefix);
    }
}