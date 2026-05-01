using GropMng.Core.Domain.Garden.Enums;

namespace GropMng.Core.Domain.Garden.Care;

/// <summary>
/// Records an owner's decision to skip a scheduled care action (watering or fertilizing)
/// for a plant instance until a specific date.
/// </summary>
public partial class ActionSkip : AuditableEntity
{
    public Guid OwnerId { get; set; }

    public int PlantInstanceId { get; set; }

    /// <summary>
    /// Type of care action that was skipped (Watering = 0, Fertilizing = 1).
    /// </summary>
    public ActionSkipType ActionType { get; set; }

    /// <summary>
    /// UTC timestamp when the skip was recorded.
    /// </summary>
    public DateTime SkippedAtUtc { get; set; }

    /// <summary>
    /// The skip suppresses the action row until this date (inclusive).
    /// After this date the row reappears normally.
    ///   "Skip for today"            → today
    ///   "Notify at next scheduled"  → today + FrequencyDays - 1
    /// </summary>
    public DateOnly ActiveUntilDate { get; set; }

    public Plants.PlantInstance PlantInstance { get; set; } = null!;
}
