namespace GropMng.Web.Models.ActionLog;

/// <summary>
/// Payload for recording a bulk "water all" action from the dashboard.
/// </summary>
public class LogWateringAllRequest
{
    public List<int> PlantInstanceIds { get; set; } = new();
}
