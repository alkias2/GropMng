using GropMng.Core.Caching;
using GropMng.Core.Domain.Security;

namespace GropMng.Services.Caching.System;

/// <summary>
/// Invalidates permission caches when permission records change.
/// </summary>
public sealed class PermissionRecordCacheEventConsumer : BaseCacheEventConsumer<PermissionRecord>
{
    public PermissionRecordCacheEventConsumer(IGropStaticCacheManager cacheManager)
        : base(cacheManager)
    {
    }

    protected override async Task ClearCacheAsync(PermissionRecord entity, EntityEventType eventType, CancellationToken cancellationToken)
    {
        await CacheManager.RemoveByPrefixAsync(SystemCacheDefaults.PermissionPrefix);
    }
}