using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Plants;
using GropMng.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace GropMng.Web.Initialization.Seeders;

internal sealed class SoilMixSeeder
{
    private static readonly Guid DemoOwnerBusinessId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    // 27 unique soil compositions (Ελαφρόπετρα;Περλίτης;CocoCoir;ΦλοιόςΠεύκου;Κομπόστ;Χούμος;Ζεόλιθος)
    // Each entry: (MixName, int[7] percentages matching ingredient order in SoilIngredientSeeder)
    // TempIds that use each mix are listed in comment
    private static readonly SoilMixEntry[] Mixes =
    [
        new("Βασιλικός Αγιορίτικος — Zone A",    [15, 8, 28, 7, 25, 10, 7],  SoilDrainageType.Good),      // TempId 1,2
        new("Τριανταφυλλιά / Γιασεμί",            [18, 10, 22, 12, 20, 8, 10], SoilDrainageType.Good),     // TempId 3,4,5
        new("Duranta παρτέρι",                     [18, 10, 24, 10, 20, 8, 10], SoilDrainageType.Good),     // TempId 6
        new("Λεβάντα / Βουκαμβίλια",              [24, 14, 16, 14, 14, 6, 12], SoilDrainageType.Excellent), // TempId 7,11
        new("Creeping Sedum",                      [30, 18, 12, 10, 10, 5, 15], SoilDrainageType.Excellent), // TempId 8
        new("Αγαπάνθος Νάνος",                    [20, 12, 20, 10, 18, 8, 12], SoilDrainageType.Good),     // TempId 9
        new("Πεύκο",                               [28, 14, 14, 18, 12, 4, 10], SoilDrainageType.Excellent), // TempId 10
        new("Λεμονιά",                             [20, 12, 20, 10, 20, 8, 10], SoilDrainageType.Good),     // TempId 12
        new("Βασιλικός Αγιορίτικος — Zone B",    [20, 14, 20, 10, 18, 8, 10], SoilDrainageType.Good),     // TempId 13
        new("Crown of Thorns",                    [30, 18, 12, 12, 8, 5, 15],  SoilDrainageType.Excellent), // TempId 14
        new("Μέντα / Δυόσμος",                    [18, 12, 24, 8, 18, 10, 10], SoilDrainageType.Good),     // TempId 15,16
        new("Τριανταφυλλιά — Zone B",             [20, 12, 20, 12, 18, 8, 10], SoilDrainageType.Good),     // TempId 17
        new("Aloe maculata / Aloe vera",          [30, 20, 10, 10, 8, 4, 18],  SoilDrainageType.Excellent), // TempId 18,19,24
        new("Sago Palm",                           [28, 16, 14, 14, 10, 6, 12], SoilDrainageType.Good),     // TempId 20
        new("Pelargonium",                         [24, 14, 18, 12, 14, 8, 10], SoilDrainageType.Good),     // TempId 21
        new("Ρίγανη",                              [26, 16, 16, 12, 12, 6, 12], SoilDrainageType.Excellent), // TempId 22
        new("Χρυσάνθεμο",                         [20, 12, 22, 10, 18, 8, 10], SoilDrainageType.Good),     // TempId 23
        new("Κουμκουάτ",                           [22, 14, 18, 12, 16, 8, 10], SoilDrainageType.Good),     // TempId 25
        new("Γαρδένια",                            [24, 14, 18, 18, 12, 6, 8],  SoilDrainageType.Good),     // TempId 26
        new("Ροδακινιά παρτέρι",                  [26, 14, 16, 16, 14, 6, 8],  SoilDrainageType.Good),     // TempId 27
        new("Δρακαίνα — Zone C",                  [24, 14, 20, 14, 14, 6, 8],  SoilDrainageType.Good),     // TempId 28
        new("Κλίβια",                              [22, 12, 22, 14, 14, 8, 8],  SoilDrainageType.Good),     // TempId 29,30
        new("Jade Plant — Κράσουλα",              [32, 18, 10, 12, 8, 5, 15],  SoilDrainageType.Excellent), // TempId 31
        new("String-of-Pearls — Μαργαριταράκι",  [34, 20, 10, 10, 8, 4, 14],  SoilDrainageType.Excellent), // TempId 32
        new("Κάκτοι (San Pedro / Prickly / Barrel)", [38, 22, 8, 10, 6, 3, 13], SoilDrainageType.Excellent), // TempId 33,35,36
        new("Spear Sansevieria",                  [34, 20, 10, 12, 8, 4, 12],  SoilDrainageType.Excellent), // TempId 34
        new("Δρακαίνα — Zone C (μικρή γλ.)",     [22, 14, 22, 14, 14, 6, 8],  SoilDrainageType.Good)      // TempId 37
    ];

    // Maps TempId (1-based) to mix index (0-based) in Mixes array above
    internal static readonly int[] TempIdToMixIndex = new int[38]
    {
        -1, // 0 unused
        0,  // 1 - Holy Basil Zone A
        0,  // 2 - Holy Basil Zone A
        1,  // 3 - Rosa/Jasmine
        1,  // 4 - Jasmine
        1,  // 5 - Star Jasmine
        2,  // 6 - Duranta
        3,  // 7 - Lavender
        4,  // 8 - Sedum
        5,  // 9 - Agapanthus
        6,  // 10 - Pine
        3,  // 11 - Bougainvillea
        7,  // 12 - Lemon
        8,  // 13 - Holy Basil Zone B
        9,  // 14 - Crown of Thorns
        10, // 15 - Mint
        10, // 16 - Spearmint
        11, // 17 - Rosa odorata Zone B
        12, // 18 - Aloe maculata
        12, // 19 - Aloe maculata
        13, // 20 - Sago Palm
        14, // 21 - Pelargonium
        15, // 22 - Oregano
        16, // 23 - Chrysanthemum
        12, // 24 - Aloe vera
        17, // 25 - Kumquat
        18, // 26 - Gardenia
        19, // 27 - Peach Tree
        20, // 28 - Dracaena (large)
        21, // 29 - Clivia
        21, // 30 - Clivia
        22, // 31 - Jade Plant
        23, // 32 - String-of-Pearls
        24, // 33 - San Pedro Cactus
        25, // 34 - Spear Sansevieria
        24, // 35 - Prickly Pear
        24, // 36 - Barrel Cactus
        26  // 37 - Dracaena (small)
    };

    // Ingredient name order (matches Mixes percentage array positions)
    private static readonly string[] IngredientNames =
    [
        "Ελαφρόπετρα",
        "Περλίτης",
        "Κοκοφοίνικας Coco Coir",
        "Φλοιός πεύκου μέτριου μεγέθους",
        "Κομπόστ ώριμο",
        "Χούμος γαιοσκωλήκων",
        "Ζεόλιθος"
    ];

    private readonly GropContext _dbContext;

    public SoilMixSeeder(GropContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<int>> SeedAsync(
        IReadOnlyDictionary<string, int> ingredientIdsByName,
        CancellationToken cancellationToken = default)
    {
        // Return existing if already seeded for this owner
        var existing = await _dbContext.SoilMixes
            .Where(m => !m.IsDeleted)
            .OrderBy(m => m.Id)
            .Select(m => m.Id)
            .ToListAsync(cancellationToken);

        if (existing.Count >= Mixes.Length)
            return existing;

        var existingNames = existing.Count > 0
            ? (await _dbContext.SoilMixes
                .Where(m => !m.IsDeleted)
                .Select(m => m.Name)
                .ToListAsync(cancellationToken)).ToHashSet()
            : [];

        var now = DateTime.UtcNow;
        var seededMixes = new List<SoilMix>();

        foreach (var mix in Mixes)
        {
            if (existingNames.Contains(mix.Name))
                continue;

            var soilMix = new SoilMix
            {
                Name = mix.Name,
                Drainage = mix.Drainage,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };

            for (var i = 0; i < IngredientNames.Length; i++)
            {
                var pct = mix.Percentages[i];
                if (pct <= 0)
                    continue;

                var ingredientName = IngredientNames[i];
                if (!ingredientIdsByName.TryGetValue(ingredientName, out var ingredientId))
                    continue;

                soilMix.Ingredients.Add(new SoilMixIngredient
                {
                    SoilIngredientId = ingredientId,
                    PercentageByVolume = pct,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                });
            }

            _dbContext.SoilMixes.Add(soilMix);
            seededMixes.Add(soilMix);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await _dbContext.SoilMixes
            .Where(m => !m.IsDeleted)
            .OrderBy(m => m.Id)
            .Select(m => m.Id)
            .ToListAsync(cancellationToken);
    }

    private sealed record SoilMixEntry(string Name, int[] Percentages, SoilDrainageType Drainage);
}
