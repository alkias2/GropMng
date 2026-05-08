namespace GropMng.Web.Models.ActionLog;

/// <summary>
/// Payload for recording a quick fertilizing action from the dashboard.
/// </summary>
public class LogFertilizingRequest
{
    public int PlantInstanceId { get; set; }

    public decimal? Quantity { get; set; }
    public GropMng.Core.Domain.Garden.Enums.FertilizerQuantityUnit? Unit { get; set; }
}
