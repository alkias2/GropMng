namespace GropMng.Web.Models.ActionLog;

/// <summary>
/// Payload for recording a quick watering action from the dashboard.
/// </summary>
public class LogWateringRequest
{
    public int PlantInstanceId { get; set; }

    public decimal? WaterAmountL { get; set; }
}
