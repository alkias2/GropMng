namespace GropMng.Core.Domain.Garden.Care;

/// <summary>
/// Records a single watering event for a plant instance.
/// </summary>
public partial class WateringLog : AuditableEntity
{
    public Guid OwnerId { get; set; }

    public int PlantInstanceId { get; set; }

    public DateTime WateredAtUtc { get; set; }

    public decimal? WaterAmountL { get; set; }

    public string? Notes { get; set; }

    public Plants.PlantInstance PlantInstance { get; set; } = null!;
}
