using GropMng.Core.Domain.Garden.Enums;

namespace GropMng.Core.Domain.Garden.Plants;

public partial class Container : AuditableEntity
{
    public Guid OwnerId { get; set; }

    public GardenContainerType ContainerType { get; set; } = GardenContainerType.Pot;

    public string? Material { get; set; }

    public decimal? LengthCm { get; set; }

    public decimal? WidthCm { get; set; }

    public decimal? DepthCm { get; set; }

    public decimal? DiameterCm { get; set; }

    public decimal? VolumeL { get; set; }

    public string? Color { get; set; }

    public bool HasDrainageHole { get; set; } = true;

    public string? Notes { get; set; }

    public IList<PlantInstance> PlantInstances { get; set; } = new List<PlantInstance>();
}
