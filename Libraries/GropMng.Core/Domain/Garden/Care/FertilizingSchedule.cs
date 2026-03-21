using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Plants;

namespace GropMng.Core.Domain.Garden.Care;

public partial class FertilizingSchedule : AuditableEntity
{
    public Guid OwnerId { get; set; }

    public int PlantInstanceId { get; set; }

    public int FertilizerId { get; set; }

    public GardenSeason Season { get; set; } = GardenSeason.Spring;

    public byte FrequencyDays { get; set; } = 14;

    public decimal? Quantity { get; set; }

    public FertilizerQuantityUnit? Unit { get; set; } = FertilizerQuantityUnit.Gram;

    public string? Notes { get; set; }

    public PlantInstance PlantInstance { get; set; } = null!;

    public Fertilizer Fertilizer { get; set; } = null!;
}
