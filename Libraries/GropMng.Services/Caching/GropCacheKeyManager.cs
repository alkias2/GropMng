using System.Collections.Concurrent;
using GropMng.Core.Caching;

namespace GropMng.Services.Caching;

/// <summary>
/// Tracks active cache keys for coarse prefix invalidation.
/// </summary>
public class GropCacheKeyManager : IGropCacheKeyManager
{
    private readonly ConcurrentDictionary<string, byte> _keys = new(StringComparer.Ordinal);

    public IEnumerable<string> Keys => _keys.Keys;

    public void AddKey(string key)
    {
        _keys.TryAdd(key, 0);
    }

    public void RemoveKey(string key)
    {
        _keys.TryRemove(key, out _);
    }

    public IEnumerable<string> RemoveByPrefix(string prefix)
    {
        var keys = _keys.Keys
            .Where(key => key.StartsWith(prefix, StringComparison.Ordinal))
            .ToArray();

        foreach (var key in keys)
            _keys.TryRemove(key, out _);

        return keys;
    }

    public void Clear()
    {
        _keys.Clear();
    }
}