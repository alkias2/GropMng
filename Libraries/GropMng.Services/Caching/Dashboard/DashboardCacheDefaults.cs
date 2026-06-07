using GropMng.Core.Caching;
using GropMng.Services.Caching.Garden;
using GropMng.Services.Caching.System;

namespace GropMng.Services.Caching.Dashboard;

/// <summary>
/// Centralized cache keys and prefixes used by the owner dashboard.
/// </summary>
public static class DashboardCacheDefaults
{
    public static string InstancesPrefix => GropCacheDefaults.DashboardPrefix + "instances.";

    public static string PlantsPrefix => GropCacheDefaults.DashboardPrefix + "plants.";

    public static string GardenSpotsPrefix => GropCacheDefaults.DashboardPrefix + "spots.";

    public static string LocationsPrefix => GropCacheDefaults.DashboardPrefix + "locations.";

    public static string WateringSchedulesPrefix => GropCacheDefaults.DashboardPrefix + "watering-schedules.";

    public static string FertilizingSchedulesPrefix => GropCacheDefaults.DashboardPrefix + "fertilizing-schedules.";

    public static string FertilizersPrefix => GropCacheDefaults.DashboardPrefix + "fertilizers.";

    public static string WateringLogsPrefix => GropCacheDefaults.DashboardPrefix + "watering-logs.";

    public static string FertilizingLogsPrefix => GropCacheDefaults.DashboardPrefix + "fertilizing-logs.";

    public static string ActiveSkipsPrefix => GropCacheDefaults.DashboardPrefix + "active-skips.";

    public static GropCacheKey PlantInstancesCacheKey =>
        new("Grop.dashboard.owner.instances.v1.{0}",
            GropCacheDefaults.DashboardPrefix,
            InstancesPrefix,
            PlantCacheDefaults.PlantInstancePrefix)
        {
            CacheTime = 2
        };

    public static GropCacheKey PlantsLookupCacheKey =>
        new("Grop.dashboard.owner.plants.v1.{0}",
            GropCacheDefaults.DashboardPrefix,
            PlantsPrefix,
            PlantCacheDefaults.PlantPrefix)
        {
            CacheTime = 2
        };

    public static GropCacheKey GardenSpotsLookupCacheKey =>
        new("Grop.dashboard.owner.spots.v1.{0}",
            GropCacheDefaults.DashboardPrefix,
            GardenSpotsPrefix,
            LocationCacheDefaults.GardenSpotPrefix)
        {
            CacheTime = 2
        };

    public static GropCacheKey LocationsLookupCacheKey =>
        new("Grop.dashboard.owner.locations.v1.{0}",
            GropCacheDefaults.DashboardPrefix,
            LocationsPrefix,
            LocationCacheDefaults.LocationPrefix)
        {
            CacheTime = 2
        };

    public static GropCacheKey WateringSchedulesCacheKey =>
        new("Grop.dashboard.owner.watering-schedules.v1.{0}.{1}",
            GropCacheDefaults.DashboardPrefix,
            WateringSchedulesPrefix,
            WateringCacheDefaults.SchedulePrefix)
        {
            CacheTime = 2
        };

    public static GropCacheKey FertilizingSchedulesCacheKey =>
        new("Grop.dashboard.owner.fertilizing-schedules.v1.{0}.{1}",
            GropCacheDefaults.DashboardPrefix,
            FertilizingSchedulesPrefix,
            FertilizingCacheDefaults.SchedulePrefix)
        {
            CacheTime = 2
        };

    public static GropCacheKey FertilizersLookupCacheKey =>
        new("Grop.dashboard.owner.fertilizers.v1.{0}",
            GropCacheDefaults.DashboardPrefix,
            FertilizersPrefix,
            FertilizerCacheDefaults.Prefix)
        {
            CacheTime = 2
        };

    public static GropCacheKey WateringLogsCacheKey =>
        new("Grop.dashboard.owner.watering-logs.v1.{0}",
            GropCacheDefaults.DashboardPrefix,
            WateringLogsPrefix,
            WateringCacheDefaults.LogPrefix)
        {
            CacheTime = 1
        };

    public static GropCacheKey FertilizingLogsCacheKey =>
        new("Grop.dashboard.owner.fertilizing-logs.v1.{0}",
            GropCacheDefaults.DashboardPrefix,
            FertilizingLogsPrefix,
            FertilizingCacheDefaults.LogPrefix)
        {
            CacheTime = 1
        };

    public static GropCacheKey ActiveSkipsCacheKey =>
        new("Grop.dashboard.owner.active-skips.v1.{0}.{1}",
            GropCacheDefaults.DashboardPrefix,
            ActiveSkipsPrefix,
            ActionSkipCacheDefaults.Prefix)
        {
            CacheTime = 1
        };
}