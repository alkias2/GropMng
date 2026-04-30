using GropMng.Core.Domain.Garden.Enums;

namespace GropMng.Core.Domain.Garden.Care;

/// <summary>
/// Records a single fertilizer application event for a plant instance.
/// </summary>
public partial class FertilizingLog : AuditableEntity
{
    public Guid OwnerId { get; set; }

    public int PlantInstanceId { get; set; }

    public int FertilizerId { get; set; }

    public DateTime AppliedAtUtc { get; set; }

    public decimal? Quantity { get; set; }

    public FertilizerQuantityUnit? Unit { get; set; }

    public string? Notes { get; set; }

    public Plants.PlantInstance PlantInstance { get; set; } = null!;

    public Fertilizer Fertilizer { get; set; } = null!;
}
