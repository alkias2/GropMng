using GropMng.Core.Caching;

namespace GropMng.Services.Caching.Garden;

/// <summary>
/// Cache keys and prefixes used by dashboard action suppression.
/// </summary>
public static class ActionSkipCacheDefaults
{
    public static string Prefix => "Grop.action-skip.";

    public static GropCacheKey DashboardActiveSkipsCacheKey =>
        new("Grop.dashboard.owner.active-skips.v1.{0}.{1}",
            GropCacheDefaults.DashboardPrefix,
            Prefix);
}