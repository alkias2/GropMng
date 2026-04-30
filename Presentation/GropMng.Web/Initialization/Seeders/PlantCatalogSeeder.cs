using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Plants;
using GropMng.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace GropMng.Web.Initialization.Seeders;

internal sealed class PlantCatalogSeeder
{
    // 32 unique species (by ScientificName) from the 37 PlantInstance CSV records
    private static readonly PlantEntry[] Plants =
    [
        new("Ocimum tenuiflorum", "Holy Basil (Tulsi) — Βασιλικός Αγιορίτικος", "Lamiaceae", PlantCategory.Aromatic, isEdible: true, isMedicinal: true),
        new("Rosa 'Variegata di Bologna'", "Variegata di Bologna — Τριανταφυλλιά", "Rosaceae", PlantCategory.Ornamental),
        new("Jasminum polyanthum", "Jasmine — Γιασεμί", "Oleaceae", PlantCategory.Climber),
        new("Trachelospermum jasminoides", "Star Jasmine — Ρυγχόσπερμο", "Apocynaceae", PlantCategory.Climber),
        new("Duranta spp.", "Duranta — Δουράντα", "Verbenaceae", PlantCategory.Shrub),
        new("Lavandula angustifolia", "Lavender — Λεβάντα", "Lamiaceae", PlantCategory.Aromatic, isMedicinal: true),
        new("Sedum rupestre", "Creeping Sedum — Μπούζι", "Crassulaceae", PlantCategory.Succulent),
        new("Agapanthus africanus", "Dwarf White Agapanthus — Νάνος Κρίνος Νείλου", "Amaryllidaceae", PlantCategory.Ornamental),
        new("Pinus pinaster", "Maritime Pine — Πεύκο", "Pinaceae", PlantCategory.Tree),
        new("Bougainvillea spp.", "Bougainvillea — Βουκαμβίλια", "Nyctaginaceae", PlantCategory.Climber),
        new("Citrus × limon", "Lemon — Λεμονιά", "Rutaceae", PlantCategory.Tree, isEdible: true),
        new("Euphorbia milii", "Crown of Thorns — Ακανθόστεφανος", "Euphorbiaceae", PlantCategory.Succulent, isToxic: true),
        new("Mentha spp.", "Mint — Μέντα", "Lamiaceae", PlantCategory.Aromatic, isEdible: true, isMedicinal: true),
        new("Mentha spicata", "Spearmint — Δυόσμος", "Lamiaceae", PlantCategory.Aromatic, isEdible: true, isMedicinal: true),
        new("Rosa spp.", "Rose — Τριανταφυλλιά", "Rosaceae", PlantCategory.Ornamental),
        new("Aloe maculata", "Aloe Maculata — Αλόη Μακουλάτα", "Asphodelaceae", PlantCategory.Succulent, isMedicinal: true),
        new("Cycas revoluta", "Sago Palm — Κυκάς", "Cycadaceae", PlantCategory.Ornamental, isToxic: true),
        new("Pelargonium odoratissimum", "Pelargonium — Αρμπαρόριζα", "Geraniaceae", PlantCategory.Aromatic),
        new("Origanum vulgare", "Oregano — Ρίγανη", "Lamiaceae", PlantCategory.Aromatic, isEdible: true, isMedicinal: true),
        new("Chrysanthemum spp.", "Chrysanthemum — Χρυσάνθεμο", "Asteraceae", PlantCategory.Ornamental),
        new("Aloe vera", "Aloe Vera — Αλόη", "Asphodelaceae", PlantCategory.Succulent, isMedicinal: true),
        new("Citrus japonica", "Kumquat — Κουμκουάτ", "Rutaceae", PlantCategory.Tree, isEdible: true),
        new("Gardenia jasminoides", "Gardenia — Γαρδένια", "Rubiaceae", PlantCategory.Ornamental),
        new("Prunus persica", "Peach Tree — Ροδακινιά", "Rosaceae", PlantCategory.Tree, isEdible: true),
        new("Dracaena marginata", "Dracaena Sticky — Δρακαίνα", "Asparagaceae", PlantCategory.Ornamental),
        new("Clivia miniata", "Kaffir Lily — Κλίβια", "Amaryllidaceae", PlantCategory.Ornamental, isToxic: true),
        new("Crassula ovata", "Jade Plant — Κράσουλα", "Crassulaceae", PlantCategory.Succulent),
        new("Senecio rowleyanus", "String-of-Pearls — Μαργαριταράκι", "Asteraceae", PlantCategory.Succulent, isToxic: true),
        new("Echinopsis pachanoi", "San Pedro Cactus — Κάκτος Σαν Πέδρο", "Cactaceae", PlantCategory.Succulent),
        new("Dracaena angolensis", "African Spear Sansevieria", "Asparagaceae", PlantCategory.Ornamental),
        new("Opuntia spp.", "Prickly Pear Cactus (Bunny Ears) — Φραγκοσυκιά", "Cactaceae", PlantCategory.Succulent, isEdible: true),
        new("Ferocactus spp.", "Barrel Cactus — Βαρελόσχημος Κάκτος", "Cactaceae", PlantCategory.Succulent)
    ];

    private readonly GropContext _dbContext;

    public PlantCatalogSeeder(GropContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyDictionary<string, int>> SeedAsync(CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Plants
            .Where(e => !e.IsDeleted)
            .ToDictionaryAsync(e => e.ScientificName, e => e.Id, cancellationToken);

        var now = DateTime.UtcNow;
        foreach (var p in Plants)
        {
            if (existing.ContainsKey(p.ScientificName))
                continue;

            _dbContext.Plants.Add(new Plant
            {
                CommonName = p.CommonName,
                ScientificName = p.ScientificName,
                Family = p.Family,
                Category = p.Category,
                IsEdible = p.isEdible,
                IsMedicinal = p.isMedicinal,
                IsToxic = p.isToxic,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await _dbContext.Plants
            .Where(e => !e.IsDeleted)
            .ToDictionaryAsync(e => e.ScientificName, e => e.Id, cancellationToken);
    }

    private sealed record PlantEntry(
        string ScientificName,
        string CommonName,
        string Family,
        PlantCategory Category,
        bool isEdible = false,
        bool isMedicinal = false,
        bool isToxic = false);
}
