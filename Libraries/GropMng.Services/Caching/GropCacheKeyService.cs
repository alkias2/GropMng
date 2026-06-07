using System.Globalization;
using GropMng.Core.Caching;

namespace GropMng.Services.Caching;

/// <summary>
/// Default cache key preparation logic for GropMng services.
/// </summary>
public abstract class GropCacheKeyService : IGropCacheKeyService
{
    private const int DefaultCacheTimeMinutes = 60;

    protected virtual object? CreateCacheKeyParameter(object? parameter)
    {
        return parameter switch
        {
            null => "null",
            decimal decimalValue => decimalValue.ToString(CultureInfo.InvariantCulture),
            DateTime dateTimeValue => dateTimeValue.ToString("O", CultureInfo.InvariantCulture),
            _ => parameter
        };
    }

    protected virtual string PrepareKeyPrefix(string prefix, params object[] prefixParameters)
    {
        return prefixParameters.Any()
            ? string.Format(prefix, prefixParameters.Select(CreateCacheKeyParameter).ToArray())
            : prefix;
    }

    public virtual GropCacheKey PrepareKey(GropCacheKey cacheKey, params object[] cacheKeyParameters)
    {
        return cacheKey.Create(CreateCacheKeyParameter, cacheKeyParameters);
    }

    public virtual GropCacheKey PrepareKeyForDefaultCache(GropCacheKey cacheKey, params object[] cacheKeyParameters)
    {
        var key = PrepareKey(cacheKey, cacheKeyParameters);
        key.CacheTime = DefaultCacheTimeMinutes;

        return key;
    }
}