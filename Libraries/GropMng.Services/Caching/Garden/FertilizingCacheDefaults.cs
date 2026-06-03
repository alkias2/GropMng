using GropMng.Core.Caching;

namespace GropMng.Services.Caching.Garden;

/// <summary>
/// Cache keys and prefixes used by fertilizing-related features.
/// </summary>
public static class FertilizingCacheDefaults
{
    public static string SchedulePrefix => "Grop.fertilizing-schedule.";

    public static string LogPrefix => "Grop.fertilizing-log.";

    public static GropCacheKey DashboardSchedulesCacheKey =>
        new("Grop.dashboard.owner.fertilizing-schedules.v1.{0}.{1}",
            GropCacheDefaults.DashboardPrefix,
            SchedulePrefix);

    public static GropCacheKey DashboardLogsCacheKey =>
        new("Grop.dashboard.owner.fertilizing-logs.v1.{0}",
            GropCacheDefaults.DashboardPrefix,
            LogPrefix);
}