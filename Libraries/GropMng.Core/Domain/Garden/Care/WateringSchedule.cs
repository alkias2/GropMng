using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Plants;

namespace GropMng.Core.Domain.Garden.Care;

public partial class WateringSchedule : AuditableEntity
{
    public Guid OwnerId { get; set; }

    public int PlantInstanceId { get; set; }

    public GardenSeason Season { get; set; } = GardenSeason.Spring;

    public byte FrequencyDays { get; set; } = 3;

    public decimal? WaterAmountL { get; set; }

    public GardenTimeOfDay? TimeOfDay { get; set; } = GardenTimeOfDay.Morning;

    public string? Notes { get; set; }

    public PlantInstance PlantInstance { get; set; } = null!;
}
