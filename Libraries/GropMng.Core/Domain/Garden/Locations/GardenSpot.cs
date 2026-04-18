using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Plants;

namespace GropMng.Core.Domain.Garden.Locations;

public partial class GardenSpot : AuditableEntity
{
    public Guid OwnerId { get; set; }

    public int LocationId { get; set; }

    public required string Name { get; set; }

    public GardenOrientation? Orientation { get; set; }

    public GardenCoverType? CoverType { get; set; }

    public byte? SunHoursPerDay { get; set; }

    public string? Surroundings { get; set; }

    public string? Notes { get; set; }

    public Location Location { get; set; } = null!;

    public IList<PlantInstance> PlantInstances { get; set; } = new List<PlantInstance>();
}
