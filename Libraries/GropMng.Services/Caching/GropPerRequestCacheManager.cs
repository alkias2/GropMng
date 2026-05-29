using System.Collections.Concurrent;
using GropMng.Core.Caching;

namespace GropMng.Services.Caching;

/// <summary>
/// Request-scope cache used to avoid duplicate read work inside a single request.
/// </summary>
public class GropPerRequestCacheManager : GropCacheKeyService, IGropShortTermCacheManager
{
    private readonly ConcurrentDictionary<string, object> _entries = new(StringComparer.Ordinal);

    public async Task<T> GetAsync<T>(Func<Task<T>> acquire, GropCacheKey cacheKey, params object[] cacheKeyParameters)
    {
        var key = PrepareKey(cacheKey, cacheKeyParameters).Key;

        if (_entries.TryGetValue(key, out var cached) && cached is T typedValue)
            return typedValue;

        var result = await acquire();
        if (result is not null)
            _entries[key] = result!;

        return result;
    }

    public void Remove(string cacheKey, params object[] cacheKeyParameters)
    {
        var key = PrepareKey(new GropCacheKey(cacheKey), cacheKeyParameters).Key;
        _entries.TryRemove(key, out _);
    }

    public void RemoveByPrefix(string prefix, params object[] prefixParameters)
    {
        var keyPrefix = PrepareKeyPrefix(prefix, prefixParameters);
        foreach (var key in _entries.Keys.Where(entryKey => entryKey.StartsWith(keyPrefix, StringComparison.Ordinal)).ToArray())
            _entries.TryRemove(key, out _);
    }
}