namespace GropMng.Core.Caching;

/// <summary>
/// Tracks active cache keys for prefix invalidation.
/// </summary>
public interface IGropCacheKeyManager
{
    IEnumerable<string> Keys { get; }

    void AddKey(string key);

    void RemoveKey(string key);

    IEnumerable<string> RemoveByPrefix(string prefix);

    void Clear();
}