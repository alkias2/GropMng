namespace GropMng.Core.Caching;

/// <summary>
/// Prepares deterministic cache keys from templates and parameters.
/// </summary>
public interface IGropCacheKeyService
{
    GropCacheKey PrepareKey(GropCacheKey cacheKey, params object[] cacheKeyParameters);

    GropCacheKey PrepareKeyForDefaultCache(GropCacheKey cacheKey, params object[] cacheKeyParameters);
}