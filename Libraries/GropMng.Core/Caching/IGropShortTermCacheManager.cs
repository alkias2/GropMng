namespace GropMng.Core.Caching;

/// <summary>
/// Represents request-scope caching for repeated reads inside a single request.
/// </summary>
public interface IGropShortTermCacheManager : IGropCacheKeyService
{
    Task<T> GetAsync<T>(Func<Task<T>> acquire, GropCacheKey cacheKey, params object[] cacheKeyParameters);

    void Remove(string cacheKey, params object[] cacheKeyParameters);

    void RemoveByPrefix(string prefix, params object[] prefixParameters);
}