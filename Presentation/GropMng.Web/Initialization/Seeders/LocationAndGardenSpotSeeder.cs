using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Locations;
using GropMng.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace GropMng.Web.Initialization.Seeders;

internal sealed class LocationAndGardenSpotSeeder
{
    private static readonly Guid DemoOwnerBusinessId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly GardenSpotEntry[] GardenSpots =
    [
        new("Zone A — South Yard", GardenOrientation.South, 8,
            "Faces wide public road; 5 m mandatory green setback. " +
            "Intense summer heat, wind from road, reflected heat from asphalt."),
        new("Zone B — East Corridor", GardenOrientation.East, 4,
            "9 m × 2.5 m corridor opening into neighbour's corridor. " +
            "Shaded afternoon by 3-story building. Morning dew, fungal pressure in cool months."),
        new("Zone C — North Yard", GardenOrientation.North, 2,
            "5.5 m × 5 m yard closed on West and North by neighbouring house walls. " +
            "High humidity, poor airflow, mold and fungal disease risk.")
    ];

    private readonly GropContext _dbContext;

    public LocationAndGardenSpotSeeder(GropContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LocationGardenSpotResult> SeedAsync(CancellationToken cancellationToken = default)
    {
        var location = await _dbContext.Locations
            .Include(l => l.GardenSpots)
            .FirstOrDefaultAsync(l => l.OwnerId == DemoOwnerBusinessId && !l.IsDeleted, cancellationToken);

        var now = DateTime.UtcNow;

        if (location is null)
        {
            location = new Location
            {
                OwnerId = DemoOwnerBusinessId,
                Name = "Demo Garden — Western Attica",
                City = "Western Attica",
                Country = "Greece",
                ClimateZone = "Mediterranean",
                Notes = "Hot dry summers, mild wet winters, high UV, occasional strong winds. North–South oriented detached house.",
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };
            _dbContext.Locations.Add(location);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        foreach (var spot in GardenSpots)
        {
            var exists = location.GardenSpots.Any(s => s.Name == spot.Name && !s.IsDeleted);
            if (exists)
                continue;

            _dbContext.GardenSpots.Add(new GardenSpot
            {
                OwnerId = DemoOwnerBusinessId,
                LocationId = location.Id,
                Name = spot.Name,
                Orientation = spot.Orientation,
                SunHoursPerDay = spot.SunHoursPerDay,
                Notes = spot.Notes,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var spotLookup = await _dbContext.GardenSpots
            .Where(s => s.OwnerId == DemoOwnerBusinessId && !s.IsDeleted)
            .ToDictionaryAsync(s => s.Name, s => s.Id, cancellationToken);

        return new LocationGardenSpotResult(location.Id, spotLookup);
    }

    private sealed record GardenSpotEntry(string Name, GardenOrientation Orientation, byte SunHoursPerDay, string Notes);
}

internal sealed record LocationGardenSpotResult(int LocationId, IReadOnlyDictionary<string, int> SpotsByName);
