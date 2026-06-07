namespace GropMng.Core.Caching;

/// <summary>
/// Represents a cache key and its invalidation prefixes.
/// </summary>
public class GropCacheKey
{
    public GropCacheKey(string key, params string[] prefixes)
    {
        Key = key;
        Prefixes.AddRange(prefixes.Where(prefix => !string.IsNullOrWhiteSpace(prefix)));
    }

    public string Key { get; protected set; }

    public List<string> Prefixes { get; protected set; } = [];

    public int CacheTime { get; set; } = 60;

    public virtual GropCacheKey Create(Func<object?, object?> createCacheKeyParameters, params object[] keyObjects)
    {
        var cacheKey = new GropCacheKey(Key, Prefixes.ToArray())
        {
            CacheTime = CacheTime
        };

        if (!keyObjects.Any())
            return cacheKey;

        var parameters = keyObjects.Select(createCacheKeyParameters).ToArray();
        cacheKey.Key = string.Format(cacheKey.Key, parameters);

        for (var index = 0; index < cacheKey.Prefixes.Count; index++)
            cacheKey.Prefixes[index] = string.Format(cacheKey.Prefixes[index], parameters);

        return cacheKey;
    }
}