namespace GropMng.Web.Models.ActionLog;

/// <summary>
/// Payload for recording a bulk "fertilize all" action from the dashboard.
/// </summary>
public class LogFertilizingAllRequest
{
    public List<int> PlantInstanceIds { get; set; } = new();
}