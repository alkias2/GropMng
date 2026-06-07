using GropMng.Core.Domain.Garden.Enums;

namespace GropMng.Core.Domain.Garden.Plants;

public partial class Container : AuditableEntity
{
    public Guid OwnerId { get; set; }

    public int? PlantInstanceId { get; set; }

    public GardenContainerType ContainerType { get; set; } = GardenContainerType.Pot;

    public string? Material { get; set; }

    // Circumference of the base opening (cm) — as measured directly from the pot
    public decimal? BaseCircumferenceCm { get; set; }

    // Circumference of the rim/top opening (cm) — as measured directly from the pot
    public decimal? RimCircumferenceCm { get; set; }

    // Height of the pot / depth of the bed (cm)
    public decimal? HeightCm { get; set; }

    // For rectangular beds: length and width (cm)
    public decimal? LengthCm { get; set; }

    public decimal? WidthCm { get; set; }

    public decimal? VolumeL { get; set; }

    public string? Color { get; set; }

    public bool HasDrainageHole { get; set; } = true;

    public string? Notes { get; set; }

    public PlantInstance? PlantInstance { get; set; }
}
