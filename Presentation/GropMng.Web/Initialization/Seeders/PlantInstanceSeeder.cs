using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Plants;
using GropMng.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace GropMng.Web.Initialization.Seeders;

internal sealed class PlantInstanceSeeder
{
    private static readonly Guid DemoOwnerBusinessId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    // 37 plant instances from Plants-Data.csv
    // (TempId, ScientificName, CommonName, GardenSpotKey, hasContainer)
    // GardenSpotKey maps to "Zone A — South Yard", "Zone B — East Corridor", "Zone C — North Yard"
    private static readonly InstanceEntry[] Instances =
    [
        new(1,  "Ocimum tenuiflorum",          "Holy Basil (Tulsi) - Βασιλικός Αγιορίτικος", "Zone A"),
        new(2,  "Ocimum tenuiflorum",          "Holy Basil (Tulsi) - Βασιλικός Αγιορίτικος", "Zone A"),
        new(3,  "Rosa 'Variegata di Bologna'", "Variegata di Bologna - Τριανταφυλλιά", "Zone A"),
        new(4,  "Jasminum polyanthum",         "Jasmine - Γιασεμί", "Zone A"),
        new(5,  "Trachelospermum jasminoides", "Star jasmine - αστεροειδές γιασεμί ή ρυγχόσπερμο", "Zone A"),
        new(6,  "Duranta spp.",                "Duranta - Δουράντα", "Zone A", groundPlanted: true),
        new(7,  "Lavandula angustifolia",      "Lavender - Λεβάντα", "Zone A"),
        new(8,  "Sedum rupestre",              "Creeping Sedum - Μπούζι", "Zone A"),
        new(9,  "Agapanthus africanus",        "Dwarf White Agapanthus - Κρίνος της Θάλασσας - Νάνος Κρίνος του Νείλου", "Zone A"),
        new(10, "Pinus pinaster",              "Maritime Pine - Πεύκο", "Zone A"),
        new(11, "Bougainvillea spp.",          "Bougainvillea - Βουκαμβίλια", "Zone A"),
        new(12, "Citrus × limon",              "Lemon - Λεμονιά", "Zone A"),
        new(13, "Ocimum tenuiflorum",          "Holy Basil (Tulsi) - Βασιλικός Αγιορίτικος", "Zone B"),
        new(14, "Euphorbia milii",             "Crown of Thorns", "Zone B"),
        new(15, "Mentha spp.",                 "Mint -  Μέντα", "Zone B"),
        new(16, "Mentha spicata",              "Mint - Δυόσμος", "Zone B"),
        new(17, "Rosa spp.",                   "Rosa × odorata - Τριανταφυλλιά", "Zone B"),
        new(18, "Aloe maculata",               "Aloe maculata", "Zone B"),
        new(19, "Aloe maculata",               "Aloe maculata", "Zone B"),
        new(20, "Cycas revoluta",              "Sago Palm", "Zone B"),
        new(21, "Pelargonium odoratissimum",   "Pelargonium Odoratissimum - Πελαργόνιο ή αρμπαρόριζα", "Zone B"),
        new(22, "Origanum vulgare",            "Oregano - Ρίγανη", "Zone B"),
        new(23, "Chrysanthemum spp.",          "Chrysanthemum - Χρυσάνθεμο", "Zone B"),
        new(24, "Aloe vera",                   "Aloe vera -  Αλόη", "Zone B"),
        new(25, "Citrus japonica",             "Kumquat", "Zone B"),
        new(26, "Gardenia jasminoides",        "Gardenia - Γαρδένια", "Zone C"),
        new(27, "Prunus persica",              "Peach Tree - Ροδακινιά", "Zone C", groundPlanted: true),
        new(28, "Dracaena marginata",          "Dracaena Sticky", "Zone C"),
        new(29, "Clivia miniata",              "Kaffir Lily / Bush Lily - Κλίβια", "Zone C"),
        new(30, "Clivia miniata",              "Kaffir Lily / Bush Lily - Κλίβια", "Zone C"),
        new(31, "Crassula ovata",              "Jade Plant - Κράσουλα", "Zone C"),
        new(32, "Senecio rowleyanus",          "Senecio rowleyanus - String-of-Pearls - Μαργαριταράκι", "Zone C"),
        new(33, "Echinopsis pachanoi",         "San Pedro Cactus", "Zone C"),
        new(34, "Dracaena angolensis",         "African Spear, Spear Sansevieria", "Zone C"),
        new(35, "Opuntia spp.",                "Prickly Pear Cactus (Bunny Ears)", "Zone C"),
        new(36, "Ferocactus spp.",             "Barrel Cactus", "Zone C"),
        new(37, "Dracaena marginata",          "Dracaena Sticky", "Zone C")
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
            await BackfillMissingNicknamesAsync(cancellationToken);

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
                Nickname = inst.CommonName,
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

    private async Task BackfillMissingNicknamesAsync(CancellationToken cancellationToken)
    {
        var instancesWithMissingNickname = await _dbContext.PlantInstances
            .Where(pi => pi.OwnerId == DemoOwnerBusinessId
                && !pi.IsDeleted
                && (pi.Nickname == null || pi.Nickname == string.Empty))
            .ToListAsync(cancellationToken);

        if (instancesWithMissingNickname.Count == 0)
            return;

        var entriesByTempId = Instances.ToDictionary(i => i.TempId);
        var plantNamesById = await _dbContext.Plants
            .Where(p => !p.IsDeleted)
            .ToDictionaryAsync(p => p.Id, p => p.CommonName, cancellationToken);

        foreach (var instance in instancesWithMissingNickname)
        {
            var tempId = TryExtractSeedId(instance.Notes);

            if (tempId.HasValue && entriesByTempId.TryGetValue(tempId.Value, out var entry))
            {
                instance.Nickname = entry.CommonName;
            }
            else if (plantNamesById.TryGetValue(instance.PlantId, out var commonName))
            {
                instance.Nickname = commonName;
            }

            instance.UpdatedAtUtc = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static int? TryExtractSeedId(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
            return null;

        const string prefix = "[seed-id:";
        var startIndex = notes.IndexOf(prefix, StringComparison.Ordinal);
        if (startIndex < 0)
            return null;

        startIndex += prefix.Length;
        var endIndex = notes.IndexOf(']', startIndex);
        if (endIndex < 0)
            return null;

        var idText = notes.Substring(startIndex, endIndex - startIndex);
        return int.TryParse(idText, out var parsedId) ? parsedId : null;
    }

    private sealed record InstanceEntry(int TempId, string ScientificName, string CommonName, string ZoneKey, bool groundPlanted = false);
}
