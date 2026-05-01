namespace GropMng.Web.Models.ActionLog;

/// <summary>
/// Payload for recording a quick fertilizing action from the dashboard.
/// </summary>
public class LogFertilizingRequest
{
    public int PlantInstanceId { get; set; }
}
