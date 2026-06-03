using System.Threading;
using GropMng.Core.Caching;
using GropMng.Services.Caching;

namespace GropMng.Tests.Caching;

public class GropPerRequestCacheManagerTests
{
    [Fact]
    public async Task GetAsync_WhenCalledWithSameKey_UsesPerRequestCache()
    {
        var cacheManager = new GropPerRequestCacheManager();
        var key = new GropCacheKey("Grop.tests.request.same.v1") { CacheTime = 10 };
        var acquireCalls = 0;

        var first = await cacheManager.GetAsync(async () =>
        {
            Interlocked.Increment(ref acquireCalls);
            await Task.Yield();
            return "cached";
        }, key);

        var second = await cacheManager.GetAsync(async () =>
        {
            Interlocked.Increment(ref acquireCalls);
            await Task.Yield();
            return "cached";
        }, key);

        Assert.Equal("cached", first);
        Assert.Equal("cached", second);
        Assert.Equal(1, acquireCalls);
    }

    [Fact]
    public async Task Remove_WhenCalled_RemovesSpecificKey()
    {
        var cacheManager = new GropPerRequestCacheManager();
        var key = new GropCacheKey("Grop.tests.request.remove.v1") { CacheTime = 10 };
        var acquireCalls = 0;

        await cacheManager.GetAsync(() =>
        {
            Interlocked.Increment(ref acquireCalls);
            return Task.FromResult("value");
        }, key);

        cacheManager.Remove("Grop.tests.request.remove.v1");

        await cacheManager.GetAsync(() =>
        {
            Interlocked.Increment(ref acquireCalls);
            return Task.FromResult("value");
        }, key);

        Assert.Equal(2, acquireCalls);
    }

    [Fact]
    public async Task RemoveByPrefix_WhenCalled_RemovesMatchingEntriesOnly()
    {
        var cacheManager = new GropPerRequestCacheManager();
        var alphaKey = new GropCacheKey("Grop.tests.request.alpha.1") { CacheTime = 10 };
        var betaKey = new GropCacheKey("Grop.tests.request.beta.1") { CacheTime = 10 };
        var alphaCalls = 0;
        var betaCalls = 0;

        await cacheManager.GetAsync(() =>
        {
            Interlocked.Increment(ref alphaCalls);
            return Task.FromResult("alpha");
        }, alphaKey);

        await cacheManager.GetAsync(() =>
        {
            Interlocked.Increment(ref betaCalls);
            return Task.FromResult("beta");
        }, betaKey);

        cacheManager.RemoveByPrefix("Grop.tests.request.alpha.");

        await cacheManager.GetAsync(() =>
        {
            Interlocked.Increment(ref alphaCalls);
            return Task.FromResult("alpha");
        }, alphaKey);

        await cacheManager.GetAsync(() =>
        {
            Interlocked.Increment(ref betaCalls);
            return Task.FromResult("beta");
        }, betaKey);

        Assert.Equal(2, alphaCalls);
        Assert.Equal(1, betaCalls);
    }

    [Fact]
    public async Task DifferentInstances_DoNotShareEntries()
    {
        var firstScope = new GropPerRequestCacheManager();
        var secondScope = new GropPerRequestCacheManager();
        var key = new GropCacheKey("Grop.tests.request.scope.v1") { CacheTime = 10 };
        var firstCalls = 0;
        var secondCalls = 0;

        await firstScope.GetAsync(() =>
        {
            Interlocked.Increment(ref firstCalls);
            return Task.FromResult("scope-1");
        }, key);

        await firstScope.GetAsync(() =>
        {
            Interlocked.Increment(ref firstCalls);
            return Task.FromResult("scope-1");
        }, key);

        await secondScope.GetAsync(() =>
        {
            Interlocked.Increment(ref secondCalls);
            return Task.FromResult("scope-2");
        }, key);

        Assert.Equal(1, firstCalls);
        Assert.Equal(1, secondCalls);
    }
}
