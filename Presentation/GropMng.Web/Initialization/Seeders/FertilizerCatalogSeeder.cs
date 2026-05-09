using GropMng.Core.Domain.Garden.Care;
using GropMng.Core.Domain.Garden.Enums;
using GropMng.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace GropMng.Web.Initialization.Seeders;

internal sealed class FertilizerCatalogSeeder
{
    // 17 unique products (by Name+Brand) derived from Plant-Fertilize.csv
    private static readonly FertilizerEntry[] Fertilizers =
    [
        new("Βιολογικό Υγρό Λίπασμα Γενικής Χρήσης", "COMPO BIO Universal", "7-3-6", FertilizerKind.Organic, FertilizerApplicationMethod.Diluted, isOrganic: true),
        new("Λίπασμα Τριανταφυλλιάς", "COMPO Rose Spezial", "6-14-10+2MgO", FertilizerKind.Mineral, FertilizerApplicationMethod.Soil),
        new("Γενικό Ισορροπημένο Υγρό Λίπασμα", "COMPO Universal", "7-3-7", FertilizerKind.Chemical, FertilizerApplicationMethod.Diluted),
        new("Γενικό Κοκκώδες Λίπασμα Παρτεριού", "COMPO Blaukorn Classic", "12-8-16+3MgO", FertilizerKind.Chemical, FertilizerApplicationMethod.Soil),
        new("Λίπασμα Μεσογειακών Φυτών", "Substral Naturen Citrus & Mediterranean", "5-5-10", FertilizerKind.Organic, FertilizerApplicationMethod.Diluted, isOrganic: true),
        new("Λίπασμα Παχύφυτων και Κάκτων", "COMPO Kaktus und Sukkulenten", "5-10-5", FertilizerKind.Chemical, FertilizerApplicationMethod.Diluted),
        new("Λίπασμα Κωνοφόρων Βραδείας Αποδέσμευσης", "COMPO Slow Release Conifer", "14-3-8+2MgO", FertilizerKind.Mineral, FertilizerApplicationMethod.Soil),
        new("Λίπασμα Ανθοφόρων Χαμηλό Ν", "COMPO Blühpflanzen", "3-12-12", FertilizerKind.Chemical, FertilizerApplicationMethod.Diluted),
        new("Λίπασμα Εσπεριδοειδών", "COMPO Citrus Dünger", "8-4-8+2MgO", FertilizerKind.Mineral, FertilizerApplicationMethod.Soil),
        new("Λίπασμα Φοινίκων", "COMPO Palm Dünger", "12-4-12+Mn+Mg", FertilizerKind.Mineral, FertilizerApplicationMethod.Soil),
        new("Λίπασμα Γερανίων και Πελαργονίων", "COMPO Geranien Dünger", "5-5-10+3MgO", FertilizerKind.Chemical, FertilizerApplicationMethod.Diluted),
        new("Λίπασμα Αρωματικών Βοτάνων Κοκκώδες", "COMPO BIO Universal", "5-5-5", FertilizerKind.Organic, FertilizerApplicationMethod.Soil, isOrganic: true),
        new("Λίπασμα Ανθοφόρων Γλάστρας", "COMPO Blühpflanzen", "5-10-10", FertilizerKind.Chemical, FertilizerApplicationMethod.Diluted),
        new("Λίπασμα Οξύφιλων Φυτών", "COMPO Azalee-Rhododendron", "7-3-7 (όξινο)", FertilizerKind.Mineral, FertilizerApplicationMethod.Diluted),
        new("Λίπασμα Οπωροφόρων Δέντρων", "COMPO Obstbaum Dünger", "8-8-8", FertilizerKind.Mineral, FertilizerApplicationMethod.Soil),
        new("Λίπασμα Πράσινων Φυτών", "COMPO Green Plants", "7-3-7", FertilizerKind.Chemical, FertilizerApplicationMethod.Diluted),
        new("Λίπασμα Κάκτων", "COMPO Kaktus Dünger", "5-10-5", FertilizerKind.Chemical, FertilizerApplicationMethod.Diluted),
        new("Gemma Οργανικό για Τριανταφυλλιές & Ανθοφόρα", "Gemma", "Κοκκώδες βραδ. αποδ.", FertilizerKind.Organic, FertilizerApplicationMethod.Soil, isOrganic: true),
        new("Gemma Οργανικό Εσπεριδοειδή & Καρποφόρα", "Gemma", "N6-P3-K18+Mg κοκκώδες", FertilizerKind.Organic, FertilizerApplicationMethod.Soil, isOrganic: true),
        new("Gemma Βιολογική Ακτιβοζίνη για Οξύφιλα", "Gemma", "#NAME?", FertilizerKind.Organic, FertilizerApplicationMethod.Soil, isOrganic: true),
        new("Compo Υγρό για Κάκτους & Παχύφυτα", "Compo", "NPK 5-5-7+ΙΧΝ υγρό", FertilizerKind.Chemical, FertilizerApplicationMethod.Diluted)
    ];

    private readonly GropContext _dbContext;

    public FertilizerCatalogSeeder(GropContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyDictionary<string, int>> SeedAsync(CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Fertilizers
            .Where(e => !e.IsDeleted)
            .ToDictionaryAsync(e => $"{e.Name}|{e.Brand}", e => e.Id, cancellationToken);

        var now = DateTime.UtcNow;
        foreach (var f in Fertilizers)
        {
            var key = $"{f.Name}|{f.Brand}";
            if (existing.ContainsKey(key))
                continue;

            _dbContext.Fertilizers.Add(new Fertilizer
            {
                Name = f.Name,
                Brand = f.Brand,
                NpkRatio = f.NpkRatio,
                FertilizerType = f.Kind,
                ApplicationMethod = f.ApplicationMethod,
                IsOrganic = f.isOrganic,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await _dbContext.Fertilizers
            .Where(e => !e.IsDeleted)
            .ToDictionaryAsync(e => $"{e.Name}|{e.Brand}", e => e.Id, cancellationToken);
    }

    private sealed record FertilizerEntry(
        string Name,
        string Brand,
        string NpkRatio,
        FertilizerKind Kind,
        FertilizerApplicationMethod ApplicationMethod,
        bool isOrganic = false);
}
