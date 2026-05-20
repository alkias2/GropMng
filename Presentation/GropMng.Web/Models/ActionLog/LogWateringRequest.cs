namespace GropMng.Web.Models.ActionLog;

/// <summary>
/// Payload for recording a quick watering action from the dashboard.
/// </summary>
public class LogWateringRequest
{
    public int PlantInstanceId { get; set; }

    public decimal? WaterAmountL { get; set; }

    /// <summary>
    /// Due status from dashboard card: overdue, today, or upcoming.
    /// </summary>
    public string? DueStatus { get; set; }

    /// <summary>
    /// Original due date rendered on the dashboard card.
    /// Used to suppress reappearance when an upcoming action is completed early.
    /// </summary>
    public DateOnly? DueDate { get; set; }
}
