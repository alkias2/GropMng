using GropMng.Core.Domain.Garden.Plants;

namespace GropMng.Core.Domain.Garden.Locations;

public partial class Location : AuditableEntity
{
    public required string OwnerId { get; set; }

    public required string Name { get; set; }

    public required string City { get; set; }

    public string Country { get; set; } = "Greece";

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public string? ClimateZone { get; set; }

    public string? Notes { get; set; }

    public IList<GardenSpot> GardenSpots { get; set; } = new List<GardenSpot>();
}