using GropMng.Core.Domain.Garden.Care;
using GropMng.Core.Domain.Garden.Enums;
using GropMng.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace GropMng.Web.Initialization.Seeders;

internal sealed class FertilizingScheduleSeeder
{
    private static readonly Guid DemoOwnerBusinessId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    // 37 rows from Plant-Fertilize.csv
    // FertilizerKey = "Name|Brand" to look up fertilizer ID
    // Frequency pattern: (winterDays, springDays, summerDays, autumnDays) — 0 = skip season
    private static readonly FertilizerEntry[] Entries =
    [
        new("Βιολογικό Υγρό Λίπασμα Γενικής Χρήσης|COMPO BIO Universal",    0, 14, 14, 21),
        new("Βιολογικό Υγρό Λίπασμα Γενικής Χρήσης|COMPO BIO Universal",    0, 14, 14, 21),
        new("Λίπασμα Τριανταφυλλιάς|COMPO Rose Spezial",                     0, 14, 14,  0),
        new("Γενικό Ισορροπημένο Υγρό Λίπασμα|COMPO Universal",             0, 14, 21,  0),
        new("Γενικό Ισορροπημένο Υγρό Λίπασμα|COMPO Universal",             0, 14, 21,  0),
        new("Γενικό Κοκκώδες Λίπασμα Παρτεριού|COMPO Blaukorn Classic",     0, 30, 30,  0),
        new("Λίπασμα Μεσογειακών Φυτών|Substral Naturen Citrus & Mediterranean", 0, 30,  0,  0),
        new("Λίπασμα Παχύφυτων και Κάκτων|COMPO Kaktus und Sukkulenten",    0, 30,  0,  0),
        new("Γενικό Ισορροπημένο Υγρό Λίπασμα|COMPO Universal",             0, 14, 14, 21),
        new("Λίπασμα Κωνοφόρων Βραδείας Αποδέσμευσης|COMPO Slow Release Conifer", 0, 60,  0,  0),
        new("Λίπασμα Ανθοφόρων Χαμηλό Ν|COMPO Blühpflanzen",               0, 14, 14,  0),
        new("Λίπασμα Εσπεριδοειδών|COMPO Citrus Dünger",                    0, 14, 14, 30),
        new("Βιολογικό Υγρό Λίπασμα Γενικής Χρήσης|COMPO BIO Universal",    0, 14, 14, 21),
        new("Γενικό Ισορροπημένο Υγρό Λίπασμα|COMPO Universal",             0, 30, 30,  0),
        new("Βιολογικό Υγρό Λίπασμα Γενικής Χρήσης|COMPO BIO Universal",    0, 14, 14, 21),
        new("Βιολογικό Υγρό Λίπασμα Γενικής Χρήσης|COMPO BIO Universal",    0, 14, 14, 21),
        new("Λίπασμα Τριανταφυλλιάς|COMPO Rose Spezial",                     0, 14, 14,  0),
        new("Λίπασμα Παχύφυτων και Κάκτων|COMPO Kaktus und Sukkulenten",    0, 45, 45,  0),
        new("Λίπασμα Παχύφυτων και Κάκτων|COMPO Kaktus und Sukkulenten",    0, 45, 45,  0),
        new("Λίπασμα Φοινίκων|COMPO Palm Dünger",                           0, 45, 45,  0),
        new("Λίπασμα Γερανίων και Πελαργονίων|COMPO Geranien Dünger",       0, 14, 14, 14),
        new("Λίπασμα Αρωματικών Βοτάνων Κοκκώδες|COMPO BIO Universal",      0, 30, 30,  0),
        new("Λίπασμα Ανθοφόρων Γλάστρας|COMPO Blühpflanzen",               0, 14, 14, 14),
        new("Λίπασμα Παχύφυτων και Κάκτων|COMPO Kaktus und Sukkulenten",    0, 45, 45,  0),
        new("Λίπασμα Εσπεριδοειδών|COMPO Citrus Dünger",                    0, 14, 14, 30),
        new("Λίπασμα Οξύφιλων Φυτών|COMPO Azalee-Rhododendron",            0, 14, 14,  0),
        new("Λίπασμα Οπωροφόρων Δέντρων|COMPO Obstbaum Dünger",            0, 30, 45,  0),
        new("Λίπασμα Πράσινων Φυτών|COMPO Green Plants",                    0, 30, 30,  0),
        new("Γενικό Ισορροπημένο Υγρό Λίπασμα|COMPO Universal",             0, 21, 21, 30),
        new("Γενικό Ισορροπημένο Υγρό Λίπασμα|COMPO Universal",             0, 21, 21, 30),
        new("Λίπασμα Παχύφυτων και Κάκτων|COMPO Kaktus und Sukkulenten",    0, 45, 45,  0),
        new("Λίπασμα Παχύφυτων και Κάκτων|COMPO Kaktus und Sukkulenten",    0, 45,  0,  0),
        new("Λίπασμα Κάκτων|COMPO Kaktus Dünger",                           0, 45, 60,  0),
        new("Λίπασμα Πράσινων Φυτών|COMPO Green Plants",                    0, 45, 60,  0),
        new("Λίπασμα Κάκτων|COMPO Kaktus Dünger",                           0, 45, 60,  0),
        new("Λίπασμα Κάκτων|COMPO Kaktus Dünger",                           0, 45, 60,  0),
        new("Λίπασμα Πράσινων Φυτών|COMPO Green Plants",                    0, 30, 30,  0)
    ];

    private readonly GropContext _dbContext;

    public FertilizingScheduleSeeder(GropContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedAsync(
        IReadOnlyList<int> plantInstanceIdsOrderedByTempId,
        IReadOnlyDictionary<string, int> fertilizerIdsByKey,
        CancellationToken cancellationToken = default)
    {
        var anyExisting = await _dbContext.FertilizingSchedules
            .AnyAsync(fs => fs.OwnerId == DemoOwnerBusinessId, cancellationToken);

        if (anyExisting)
            return;

        var now = DateTime.UtcNow;
        var records = new List<FertilizingSchedule>();

        (GardenSeason Season, Func<FertilizerEntry, int> GetDays)[] seasonMap =
        [
            (GardenSeason.Winter, e => e.WinterDays),
            (GardenSeason.Spring, e => e.SpringDays),
            (GardenSeason.Summer, e => e.SummerDays),
            (GardenSeason.Autumn, e => e.AutumnDays)
        ];

        for (var i = 0; i < Entries.Length; i++)
        {
            var entry = Entries[i];
            var instanceId = plantInstanceIdsOrderedByTempId[i];

            if (!fertilizerIdsByKey.TryGetValue(entry.FertilizerKey, out var fertilizerId))
                continue;

            foreach (var (season, getDays) in seasonMap)
            {
                var days = getDays(entry);
                if (days == 0)
                    continue;

                records.Add(new FertilizingSchedule
                {
                    OwnerId = DemoOwnerBusinessId,
                    PlantInstanceId = instanceId,
                    FertilizerId = fertilizerId,
                    Season = season,
                    FrequencyDays = (byte)days,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                });
            }
        }

        _dbContext.FertilizingSchedules.AddRange(records);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private sealed record FertilizerEntry(
        string FertilizerKey,
        int WinterDays,
        int SpringDays,
        int SummerDays,
        int AutumnDays);
}
