using GropMng.Core.Domain.Garden.Plants;
using GropMng.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace GropMng.Web.Initialization.Seeders;

internal sealed class SoilIngredientSeeder
{
    private static readonly (string Name, string Description)[] Ingredients =
    [
        ("Ελαφρόπετρα", "Pumice / volcanic rock — improves drainage and aeration"),
        ("Περλίτης", "Perlite — lightweight volcanic glass, drainage and aeration"),
        ("Κοκοφοίνικας Coco Coir", "Coconut coir — moisture retention and aeration"),
        ("Φλοιός πεύκου μέτριου μεγέθους", "Medium-grade pine bark — structure and drainage"),
        ("Κομπόστ ώριμο", "Mature compost — nutrients and water retention"),
        ("Χούμος γαιοσκωλήκων", "Worm castings — rich organic nutrient source"),
        ("Ζεόλιθος", "Zeolite — ion exchange, slow-release minerals, moisture regulation")
    ];

    private readonly GropContext _dbContext;

    public SoilIngredientSeeder(GropContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyDictionary<string, int>> SeedAsync(CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.SoilIngredients
            .Where(e => !e.IsDeleted)
            .ToDictionaryAsync(e => e.Name, e => e.Id, cancellationToken);

        var now = DateTime.UtcNow;
        foreach (var (name, description) in Ingredients)
        {
            if (existing.ContainsKey(name))
                continue;

            var entity = new SoilIngredient
            {
                Name = name,
                Description = description,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };
            _dbContext.SoilIngredients.Add(entity);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await _dbContext.SoilIngredients
            .Where(e => !e.IsDeleted)
            .ToDictionaryAsync(e => e.Name, e => e.Id, cancellationToken);
    }
}
