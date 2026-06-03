using System.Collections.Concurrent;
using System.Threading;
using GropMng.Core.Caching;
using GropMng.Services.Caching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;

namespace GropMng.Tests.Caching;

public class GropMemoryCacheManagerTests
{
    [Fact]
    public async Task GetAsync_WhenCalledRepeatedly_UsesCachedValue()
    {
        var cacheManager = CreateCacheManager();
        var cacheKey = new GropCacheKey("Grop.tests.memory.repeated.v1") { CacheTime = 10 };
        var acquireCalls = 0;

        var first = await cacheManager.GetAsync(cacheKey, () =>
        {
            Interlocked.Increment(ref acquireCalls);
            return Task.FromResult("value");
        });

        var second = await cacheManager.GetAsync(cacheKey, () =>
        {
            Interlocked.Increment(ref acquireCalls);
            return Task.FromResult("value");
        });

        Assert.Equal("value", first);
        Assert.Equal("value", second);
        Assert.Equal(1, acquireCalls);
    }

    [Fact]
    public async Task GetAsync_WhenCalledConcurrently_UsesSingleFlightAcquire()
    {
        var cacheManager = CreateCacheManager();
        var cacheKey = new GropCacheKey("Grop.tests.memory.concurrent.v1") { CacheTime = 10 };
        var acquireCalls = 0;

        var tasks = Enumerable.Range(0, 10)
            .Select(_ => cacheManager.GetAsync(cacheKey, async () =>
            {
                Interlocked.Increment(ref acquireCalls);
                await Task.Delay(20);
                return 42;
            }))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        Assert.All(results, value => Assert.Equal(42, value));
        Assert.Equal(1, acquireCalls);
    }

    [Fact]
    public async Task RemoveByPrefixAsync_RemovesOnlyMatchingKeys()
    {
        var cacheManager = CreateCacheManager();

        var alphaKey = new GropCacheKey("Grop.tests.memory.alpha.1", "Grop.tests.memory.alpha.") { CacheTime = 10 };
        var betaKey = new GropCacheKey("Grop.tests.memory.beta.1", "Grop.tests.memory.beta.") { CacheTime = 10 };

        await cacheManager.SetAsync(alphaKey, "alpha");
        await cacheManager.SetAsync(betaKey, "beta");

        await cacheManager.RemoveByPrefixAsync("Grop.tests.memory.alpha.");

        var removed = await cacheManager.GetAsync(alphaKey, "missing");
        var remaining = await cacheManager.GetAsync(betaKey, "missing");

        Assert.Equal("missing", removed);
        Assert.Equal("beta", remaining);
    }

    [Fact]
    public async Task ClearAsync_RemovesAllStoredKeys()
    {
        var cacheManager = CreateCacheManager();

        var firstKey = new GropCacheKey("Grop.tests.memory.clear.1") { CacheTime = 10 };
        var secondKey = new GropCacheKey("Grop.tests.memory.clear.2") { CacheTime = 10 };

        await cacheManager.SetAsync(firstKey, "first");
        await cacheManager.SetAsync(secondKey, "second");

        await cacheManager.ClearAsync();

        var firstValue = await cacheManager.GetAsync(firstKey, "missing");
        var secondValue = await cacheManager.GetAsync(secondKey, "missing");

        Assert.Equal("missing", firstValue);
        Assert.Equal("missing", secondValue);
    }

    private static GropMemoryCacheManager CreateCacheManager()
    {
        return new GropMemoryCacheManager(
            new MemoryCache(new MemoryCacheOptions()),
            new GropCacheKeyManager(),
            NullLogger<GropMemoryCacheManager>.Instance);
    }
}
