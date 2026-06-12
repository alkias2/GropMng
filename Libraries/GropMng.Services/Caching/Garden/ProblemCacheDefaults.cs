using GropMng.Core.Caching;

namespace GropMng.Services.Caching;

/// <summary>
/// Cache key constants for plant problem records and schedules.
/// </summary>
public static class ProblemCacheDefaults
{
    public const string ProblemRecordPrefix = "Grop.problem-record.";
    public const string ProblemSchedulePrefix = "Grop.problem-schedule.";
}