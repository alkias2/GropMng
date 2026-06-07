namespace GropMng.Core.Caching;

/// <summary>
/// Represents cross-request caching with explicit invalidation support.
/// </summary>
public interface IGropStaticCacheManager : IDisposable, IGropCacheKeyService
{
    Task<T> GetAsync<T>(GropCacheKey key, Func<Task<T>> acquire);

    Task<T> GetAsync<T>(GropCacheKey key, Func<T> acquire);

    Task<T> GetAsync<T>(GropCacheKey key, T defaultValue = default!);

    Task SetAsync<T>(GropCacheKey key, T data);

    Task RemoveAsync(GropCacheKey cacheKey, params object[] cacheKeyParameters);

    Task RemoveByPrefixAsync(string prefix, params object[] prefixParameters);

    Task ClearAsync();
}