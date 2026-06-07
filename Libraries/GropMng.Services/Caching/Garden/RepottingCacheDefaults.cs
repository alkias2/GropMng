using GropMng.Core.Caching;

namespace GropMng.Services.Caching.Garden;

/// <summary>
/// Cache prefixes for repotting log entities.
/// </summary>
public static class RepottingCacheDefaults
{
    public static string Prefix => "Grop.repotting-log.";

    public static GropCacheKey LogsByInstanceKey =>
        new("Grop.repotting-log.by-instance.v1.{0}.{1}", Prefix);
}