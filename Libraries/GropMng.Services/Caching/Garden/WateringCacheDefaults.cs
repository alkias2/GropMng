using GropMng.Core.Caching;

namespace GropMng.Services.Caching.Garden;

/// <summary>
/// Cache keys and prefixes used by watering-related features.
/// </summary>
public static class WateringCacheDefaults
{
    public static string SchedulePrefix => "Grop.watering-schedule.";

    public static string LogPrefix => "Grop.watering-log.";

    public static GropCacheKey DashboardSchedulesCacheKey =>
        new("Grop.dashboard.owner.watering-schedules.v1.{0}.{1}",
            GropCacheDefaults.DashboardPrefix,
            SchedulePrefix);

    public static GropCacheKey DashboardLogsCacheKey =>
        new("Grop.dashboard.owner.watering-logs.v1.{0}",
            GropCacheDefaults.DashboardPrefix,
            LogPrefix);

    // -- Service-level keys

    public static GropCacheKey SchedulesByInstanceKey =>
        new("Grop.watering-schedule.by-instance.v1.{0}.{1}", SchedulePrefix);

    public static GropCacheKey LogsByInstanceKey =>
        new("Grop.watering-log.by-instance.v1.{0}.{1}", LogPrefix);
}
