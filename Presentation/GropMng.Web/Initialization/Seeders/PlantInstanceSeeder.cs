using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Plants;
using GropMng.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace GropMng.Web.Initialization.Seeders;

internal sealed class PlantInstanceSeeder
{
    private static readonly Guid DemoOwnerBusinessId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    // 37 plant instances from Plants-Data.csv
    // (TempId, ScientificName, GardenSpotKey, hasContainer)
    // GardenSpotKey maps to "Zone A — South Yard", "Zone B — East Corridor", "Zone C — North Yard"
    private static readonly InstanceEntry[] Instances =
    [
        new(1,  "Ocimum tenuiflorum",         "Zone A"),
        new(2,  "Ocimum tenuiflorum",         "Zone A"),
        new(3,  "Rosa 'Variegata di Bologna'", "Zone A"),
        new(4,  "Jasminum polyanthum",        "Zone A"),
        new(5,  "Trachelospermum jasminoides", "Zone A"),
        new(6,  "Duranta spp.",               "Zone A", groundPlanted: true),
        new(7,  "Lavandula angustifolia",     "Zone A"),
        new(8,  "Sedum rupestre",             "Zone A"),
        new(9,  "Agapanthus africanus",       "Zone A"),
        new(10, "Pinus pinaster",             "Zone A"),
        new(11, "Bougainvillea spp.",         "Zone A"),
        new(12, "Citrus × limon",             "Zone A"),
        new(13, "Ocimum tenuiflorum",         "Zone B"),
        new(14, "Euphorbia milii",            "Zone B"),
        new(15, "Mentha spp.",               "Zone B"),
        new(16, "Mentha spicata",            "Zone B"),
        new(17, "Rosa spp.",                 "Zone B"),
        new(18, "Aloe maculata",             "Zone B"),
        new(19, "Aloe maculata",             "Zone B"),
        new(20, "Cycas revoluta",            "Zone B"),
        new(21, "Pelargonium odoratissimum", "Zone B"),
        new(22, "Origanum vulgare",          "Zone B"),
        new(23, "Chrysanthemum spp.",        "Zone B"),
        new(24, "Aloe vera",                 "Zone B"),
        new(25, "Citrus japonica",           "Zone B"),
        new(26, "Gardenia jasminoides",      "Zone C"),
        new(27, "Prunus persica",            "Zone C", groundPlanted: true),
        new(28, "Dracaena marginata",        "Zone C"),
        new(29, "Clivia miniata",            "Zone C"),
        new(30, "Clivia miniata",            "Zone C"),
        new(31, "Crassula ovata",            "Zone C"),
        new(32, "Senecio rowleyanus",        "Zone C"),
        new(33, "Echinopsis pachanoi",       "Zone C"),
        new(34, "Dracaena angolensis",       "Zone C"),
        new(35, "Opuntia spp.",              "Zone C"),
        new(36, "Ferocactus spp.",           "Zone C"),
        new(37, "Dracaena marginata",        "Zone C")
    ];

    private static readonly Dictionary<string, string> SpotKeyMap = new()
    {
        ["Zone A"] = "Zone A — South Yard",
        ["Zone B"] = "Zone B — East Corridor",
        ["Zone C"] = "Zone C — North Yard"
    };

    private readonly GropContext _dbContext;

    public PlantInstanceSeeder(GropContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Returns list of PlantInstance IDs ordered by TempId (1–37)
    public async Task<IReadOnlyList<int>> SeedAsync(
        IReadOnlyDictionary<string, int> plantIdsByScientificName,
        IReadOnlyDictionary<string, int> gardenSpotIdsByName,
        IReadOnlyList<int> containerIdsOrderedByTempId,     // index 0 = TempId 1
        IReadOnlyList<int> soilMixIdsOrderedByMixIndex,     // index = SoilMixSeeder.TempIdToMixIndex
        CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.PlantInstances
            .Where(pi => pi.OwnerId == DemoOwnerBusinessId && !pi.IsDeleted)
            .CountAsync(cancellationToken);

        if (existing >= Instances.Length)
        {
            return await _dbContext.PlantInstances
                .Where(pi => pi.OwnerId == DemoOwnerBusinessId && !pi.IsDeleted)
                .OrderBy(pi => pi.Id)
                .Select(pi => pi.Id)
                .ToListAsync(cancellationToken);
        }

        var now = DateTime.UtcNow;
        foreach (var inst in Instances)
        {
            var tempIdx = inst.TempId - 1; // 0-based

            if (!plantIdsByScientificName.TryGetValue(inst.ScientificName, out var plantId))
                continue;

            var spotName = SpotKeyMap[inst.ZoneKey];
            if (!gardenSpotIdsByName.TryGetValue(spotName, out var spotId))
                continue;

            int? containerId = inst.groundPlanted ? null : containerIdsOrderedByTempId[tempIdx];

            var mixIdx = SoilMixSeeder.TempIdToMixIndex[inst.TempId];
            int? soilMixId = mixIdx >= 0 && mixIdx < soilMixIdsOrderedByMixIndex.Count
                ? soilMixIdsOrderedByMixIndex[mixIdx]
                : null;

            _dbContext.PlantInstances.Add(new PlantInstance
            {
                OwnerId = DemoOwnerBusinessId,
                PlantId = plantId,
                GardenSpotId = spotId,
                ContainerId = containerId,
                SoilMixId = soilMixId,
                HealthStatus = PlantHealthStatus.Good,
                IsActive = true,
                Notes = $"[seed-id:{inst.TempId}]",
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await _dbContext.PlantInstances
            .Where(pi => pi.OwnerId == DemoOwnerBusinessId && !pi.IsDeleted)
            .OrderBy(pi => pi.Id)
            .Select(pi => pi.Id)
            .ToListAsync(cancellationToken);
    }

    private sealed record InstanceEntry(int TempId, string ScientificName, string ZoneKey, bool groundPlanted = false);
}
