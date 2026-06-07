namespace GropMng.Web.Models.ActionLog;

/// <summary>
/// Payload for recording a skip decision for a scheduled care action.
/// </summary>
public class SkipActionRequest
{
    public int PlantInstanceId { get; set; }

    /// <summary>
    /// "watering" or "fertilizing"
    /// </summary>
    public string ActionType { get; set; } = string.Empty;

    /// <summary>
    /// "today"  → suppress until end of today (reappears tomorrow)
    /// "next"   → suppress until today + FrequencyDays - 1 (reappears on next due date)
    /// </summary>
    public string SkipMode { get; set; } = string.Empty;

    /// <summary>
    /// Frequency in days from the active schedule — required when SkipMode = "next".
    /// </summary>
    public byte FrequencyDays { get; set; }
}
