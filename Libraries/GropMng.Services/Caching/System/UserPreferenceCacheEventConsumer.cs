using GropMng.Core.Caching;
using GropMng.Core.Domain.Garden.Preferences;

namespace GropMng.Services.Caching.System;

/// <summary>
/// Invalidates preference and dashboard caches when user preferences change.
/// </summary>
public sealed class UserPreferenceCacheEventConsumer : BaseCacheEventConsumer<UserPreference>
{
    public UserPreferenceCacheEventConsumer(IGropStaticCacheManager cacheManager)
        : base(cacheManager)
    {
    }

    protected override async Task ClearCacheAsync(UserPreference entity, EntityEventType eventType, CancellationToken cancellationToken)
    {
        await CacheManager.RemoveByPrefixAsync(SystemCacheDefaults.UserPreferencePrefix);
        await CacheManager.RemoveByPrefixAsync(GropCacheDefaults.DashboardPrefix);
    }
}