using System.Collections.Concurrent;
using System.Threading;
using GropMng.Core.Caching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace GropMng.Services.Caching;

/// <summary>
/// Shared in-memory cache manager with prefix invalidation and single-flight acquisition.
/// </summary>
public class GropMemoryCacheManager : GropCacheKeyService, IGropStaticCacheManager
{
    private readonly ConcurrentDictionary<string, Lazy<Task<object?>>> _ongoing = new(StringComparer.Ordinal);
    private readonly IGropCacheKeyManager _keyManager;
    private readonly ILogger<GropMemoryCacheManager> _logger;
    private readonly IMemoryCache _memoryCache;
    private bool _disposed;
    private long _hitCount;
    private long _missCount;
    private long _setCount;
    private long _removeCount;
    private long _removeByPrefixCount;

    public GropMemoryCacheManager(IMemoryCache memoryCache, IGropCacheKeyManager keyManager, ILogger<GropMemoryCacheManager> logger)
    {
        _memoryCache = memoryCache;
        _keyManager = keyManager;
        _logger = logger;
    }

    public async Task<T> GetAsync<T>(GropCacheKey key, Func<Task<T>> acquire)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (key.CacheTime <= 0)
            return await acquire();

        if (_memoryCache.TryGetValue(key.Key, out Lazy<Task<object?>>? cachedLazy) && cachedLazy is not null)
        {
            Interlocked.Increment(ref _hitCount);
            _logger.LogDebug("Cache hit for key {CacheKey}. Hits={HitCount} Misses={MissCount}", key.Key, Interlocked.Read(ref _hitCount), Interlocked.Read(ref _missCount));
            return await GetValueAsync<T>(key, cachedLazy);
        }

        Interlocked.Increment(ref _missCount);
        _logger.LogDebug("Cache miss for key {CacheKey}. Hits={HitCount} Misses={MissCount}", key.Key, Interlocked.Read(ref _hitCount), Interlocked.Read(ref _missCount));

        var lazy = _ongoing.GetOrAdd(
            key.Key,
            _ => new Lazy<Task<object?>>(
                async () => await acquire(),
                LazyThreadSafetyMode.ExecutionAndPublication));

        try
        {
            var value = await lazy.Value;
            if (value is null)
            {
                await RemoveAsync(key);
                return default!;
            }

            if (!_memoryCache.TryGetValue(key.Key, out _))
            {
                _memoryCache.Set(key.Key, lazy, PrepareEntryOptions(key));
                _keyManager.AddKey(key.Key);
                Interlocked.Increment(ref _setCount);
                _logger.LogDebug("Cache set after miss for key {CacheKey}. Sets={SetCount}", key.Key, Interlocked.Read(ref _setCount));
            }

            return (T)value;
        }
        catch
        {
            await RemoveAsync(key);
            throw;
        }
        finally
        {
            _ongoing.TryRemove(new KeyValuePair<string, Lazy<Task<object?>>>(key.Key, lazy));
        }
    }

    public Task<T> GetAsync<T>(GropCacheKey key, Func<T> acquire)
    {
        return GetAsync(key, () => Task.FromResult(acquire()));
    }

    public async Task<T> GetAsync<T>(GropCacheKey key, T defaultValue = default!)
    {
        if (!_memoryCache.TryGetValue(key.Key, out Lazy<Task<object?>>? cachedLazy) || cachedLazy is null)
            return defaultValue;

        var value = await GetValueAsync<T>(key, cachedLazy);
        return value is null ? defaultValue : value;
    }

    public Task SetAsync<T>(GropCacheKey key, T data)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (data is null || key.CacheTime <= 0)
            return Task.CompletedTask;

        var lazy = new Lazy<Task<object?>>(() => Task.FromResult<object?>(data), LazyThreadSafetyMode.ExecutionAndPublication);
        _memoryCache.Set(key.Key, lazy, PrepareEntryOptions(key));
        _keyManager.AddKey(key.Key);
        Interlocked.Increment(ref _setCount);
        _logger.LogDebug("Cache set for key {CacheKey}. Sets={SetCount}", key.Key, Interlocked.Read(ref _setCount));

        return Task.CompletedTask;
    }

    public Task RemoveAsync(GropCacheKey cacheKey, params object[] cacheKeyParameters)
    {
        var key = PrepareKey(cacheKey, cacheKeyParameters).Key;
        _ongoing.TryRemove(key, out _);
        _memoryCache.Remove(key);
        _keyManager.RemoveKey(key);
        Interlocked.Increment(ref _removeCount);
        _logger.LogDebug("Cache remove for key {CacheKey}. Removes={RemoveCount}", key, Interlocked.Read(ref _removeCount));

        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix, params object[] prefixParameters)
    {
        var keyPrefix = PrepareKeyPrefix(prefix, prefixParameters);

        foreach (var key in _keyManager.RemoveByPrefix(keyPrefix))
        {
            _ongoing.TryRemove(key, out _);
            _memoryCache.Remove(key);
        }

        Interlocked.Increment(ref _removeByPrefixCount);
        _logger.LogDebug("Cache remove by prefix {CachePrefix}. RemoveByPrefixCalls={RemoveByPrefixCount}", keyPrefix, Interlocked.Read(ref _removeByPrefixCount));

        return Task.CompletedTask;
    }

    public Task ClearAsync()
    {
        foreach (var key in _keyManager.Keys.ToArray())
        {
            _ongoing.TryRemove(key, out _);
            _memoryCache.Remove(key);
        }

        _keyManager.Clear();
        _logger.LogInformation(
            "Cache clear requested. Hits={HitCount} Misses={MissCount} Sets={SetCount} Removes={RemoveCount} RemoveByPrefixCalls={RemoveByPrefixCount}",
            Interlocked.Read(ref _hitCount),
            Interlocked.Read(ref _missCount),
            Interlocked.Read(ref _setCount),
            Interlocked.Read(ref _removeCount),
            Interlocked.Read(ref _removeByPrefixCount));

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private MemoryCacheEntryOptions PrepareEntryOptions(GropCacheKey key)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(key.CacheTime)
        };

        options.RegisterPostEvictionCallback((evictedKey, _, _, _) =>
        {
            if (evictedKey is string stringKey)
                _keyManager.RemoveKey(stringKey);
        });

        return options;
    }

    private async Task<T> GetValueAsync<T>(GropCacheKey key, Lazy<Task<object?>> lazy)
    {
        try
        {
            var value = await lazy.Value;
            return value is null ? default! : (T)value;
        }
        catch
        {
            await RemoveAsync(key);
            throw;
        }
    }
}