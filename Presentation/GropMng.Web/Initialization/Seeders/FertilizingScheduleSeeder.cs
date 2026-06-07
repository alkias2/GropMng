using GropMng.Core.Domain.Garden.Care;
using GropMng.Core.Domain.Garden.Enums;
using GropMng.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace GropMng.Web.Initialization.Seeders;

internal sealed class FertilizingScheduleSeeder
{
    private static readonly Guid DemoOwnerBusinessId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Dictionary<int, string> FertilizerKeyByCsvId = new()
    {
        [18] = "Gemma Οργανικό για Τριανταφυλλιές & Ανθοφόρα|Gemma",
        [19] = "Gemma Οργανικό Εσπεριδοειδή & Καρποφόρα|Gemma",
        [20] = "Gemma Βιολογική Ακτιβοζίνη για Οξύφιλα|Gemma",
        [21] = "Compo Υγρό για Κάκτους & Παχύφυτα|Compo"
    };

    // 37 rows from FertilizingSchedule-insert.csv
    // Seasonal columns map to one record per active season (Spring/Summer/Autumn).
    private static readonly FertilizerEntry[] Entries =
    [
        new(3,  18, 30, 30, 45, "Χωρίς λίπανση Χειμώνα. Ανοιξιάτικη έναρξη μαρτίου.", 60.0m, FertilizerQuantityUnit.Gram, "Ανά εφαρμογή"),
        new(4,  18, 30, 30, 45, "Χωρίς λίπανση Χειμώνα. Ανοιξιάτικη έναρξη μαρτίου.", 60.0m, FertilizerQuantityUnit.Gram, "Ανά εφαρμογή"),
        new(5,  18, 30, 30, 45, "Χωρίς λίπανση Χειμώνα. Ανοιξιάτικη έναρξη μαρτίου.", 60.0m, FertilizerQuantityUnit.Gram, "Ανά εφαρμογή"),
        new(6,  18, 30, 30, 45, "Χωρίς λίπανση Χειμώνα. Ανοιξιάτικη έναρξη μαρτίου.", 150.0m, FertilizerQuantityUnit.Gram, "Ανά εφαρμογή"),
        new(7,  18, 45, 45, 60, "Χωρίς λίπανση Χειμώνα. Ανοιξιάτικη έναρξη μαρτίου.", 60.0m, FertilizerQuantityUnit.Gram, "Ανά εφαρμογή"),
        new(11, 18, 30, 30, 45, "Χωρίς λίπανση Χειμώνα. Ανοιξιάτικη έναρξη μαρτίου.", 60.0m, FertilizerQuantityUnit.Gram, "Ανά εφαρμογή"),
        new(17, 18, 30, 30, 45, "Χωρίς λίπανση Χειμώνα. Ανοιξιάτικη έναρξη μαρτίου.", 60.0m, FertilizerQuantityUnit.Gram, "Ανά εφαρμογή"),
        new(23, 18, 30, 30, 45, "Χωρίς λίπανση Χειμώνα. Ανοιξιάτικη έναρξη μαρτίου.", 60.0m, FertilizerQuantityUnit.Gram, "Ανά εφαρμογή"),
        new(1,  19, 30, 30, 45, "Χωρίς λίπανση Χειμώνα. Εφαρμογή Φεβρ-Μαρτ & Σεπτ.", 75.0m, FertilizerQuantityUnit.Gram, "Ανά εφαρμογή"),
        new(2,  19, 30, 30, 45, "Χωρίς λίπανση Χειμώνα. Εφαρμογή Φεβρ-Μαρτ & Σεπτ.", 60.0m, FertilizerQuantityUnit.Gram, "Ανά εφαρμογή"),
        new(10, 19, 60, null, 60, "Χωρίς λίπανση Χειμώνα. Εφαρμογή Φεβρ-Μαρτ & Σεπτ.", 40.0m, FertilizerQuantityUnit.Gram, "Ανά εφαρμογή"),
        new(12, 19, 30, 30, 45, "Χωρίς λίπανση Χειμώνα. Εφαρμογή Φεβρ-Μαρτ & Σεπτ.", 80.0m, FertilizerQuantityUnit.Gram, "Ανά εφαρμογή"),
        new(13, 19, 30, 30, 45, "Χωρίς λίπανση Χειμώνα. Εφαρμογή Φεβρ-Μαρτ & Σεπτ.", 70.0m, FertilizerQuantityUnit.Gram, "Ανά εφαρμογή"),
        new(15, 19, 30, 30, 45, "Χωρίς λίπανση Χειμώνα. Εφαρμογή Φεβρ-Μαρτ & Σεπτ.", 50.0m, FertilizerQuantityUnit.Gram, "Ανά εφαρμογή"),
        new(16, 19, 30, 30, 45, "Χωρίς λίπανση Χειμώνα. Εφαρμογή Φεβρ-Μαρτ & Σεπτ.", 35.0m, FertilizerQuantityUnit.Gram, "Ανά εφαρμογή"),
        new(22, 19, 45, 45, 60, "Χωρίς λίπανση Χειμώνα. Εφαρμογή Φεβρ-Μαρτ & Σεπτ.", 50.0m, FertilizerQuantityUnit.Gram, "Ανά εφαρμογή"),
        new(25, 19, 30, 30, 45, "Χωρίς λίπανση Χειμώνα. Εφαρμογή Φεβρ-Μαρτ & Σεπτ.", 80.0m, FertilizerQuantityUnit.Gram, "Ανά εφαρμογή"),
        new(27, 19, 45, null, 60, "Χωρίς λίπανση Χειμώνα. Εφαρμογή Φεβρ-Μαρτ & Σεπτ.", 200.0m, FertilizerQuantityUnit.Gram, "Ανά εφαρμογή"),
        new(9,  20, 30, 30, 45, "Μόνο Άνοιξη-Φθινόπωρο. Καλοκαίρι μειωμένη δόση.", 25.0m, FertilizerQuantityUnit.Gram, "Ανά εφαρμογή"),
        new(20, 20, 45, 45, 60, "Μόνο Άνοιξη-Φθινόπωρο. Καλοκαίρι μειωμένη δόση.", 15.0m, FertilizerQuantityUnit.Gram, "Ανά εφαρμογή"),
        new(21, 20, 30, 30, 45, "Μόνο Άνοιξη-Φθινόπωρο. Καλοκαίρι μειωμένη δόση.", 25.0m, FertilizerQuantityUnit.Gram, "Ανά εφαρμογή"),
        new(26, 20, 30, 30, 45, "Μόνο Άνοιξη-Φθινόπωρο. Καλοκαίρι μειωμένη δόση.", 60.0m, FertilizerQuantityUnit.Gram, "Ανά εφαρμογή"),
        new(28, 20, 45, 45, 60, "Μισή δόση. Η Δρακαίνα ΔΕΝ είναι οξύφιλο — αποφύγετε οξίνιση χώματος.", 30.0m, FertilizerQuantityUnit.Gram, "Ανά εφαρμογή"),
        new(29, 20, 30, 30, 45, "Μόνο Άνοιξη-Φθινόπωρο. Καλοκαίρι μειωμένη δόση.", 20.0m, FertilizerQuantityUnit.Gram, "Ανά εφαρμογή"),
        new(30, 20, 30, 30, 45, "Μόνο Άνοιξη-Φθινόπωρο. Καλοκαίρι μειωμένη δόση.", 20.0m, FertilizerQuantityUnit.Gram, "Ανά εφαρμογή"),
        new(8,  21, 30, 30, 60, "Μόνο βλαστική περίοδος Μαρτ-Σεπτ. Χειμώνα ΠΑΥΣΗ.", 5.0m, FertilizerQuantityUnit.Millilitre, "Σε 100 ml νερό"),
        new(14, 21, 30, 30, 60, "Μόνο βλαστική περίοδος Μαρτ-Σεπτ. Χειμώνα ΠΑΥΣΗ.", 3.5m, FertilizerQuantityUnit.Millilitre, "Σε 700 ml νερό"),
        new(18, 21, 30, 30, 60, "Μόνο βλαστική περίοδος Μαρτ-Σεπτ. Χειμώνα ΠΑΥΣΗ.", 4.0m, FertilizerQuantityUnit.Millilitre, "Σε 800 ml νερό"),
        new(19, 21, 30, 30, 60, "Μόνο βλαστική περίοδος Μαρτ-Σεπτ. Χειμώνα ΠΑΥΣΗ.", 4.5m, FertilizerQuantityUnit.Millilitre, "Σε 900 ml νερό"),
        new(24, 21, 30, 30, 60, "Μόνο βλαστική περίοδος Μαρτ-Σεπτ. Χειμώνα ΠΑΥΣΗ.", 7.0m, FertilizerQuantityUnit.Gram, "Σε 1400 ml νερό"),
        new(31, 21, 30, 30, 60, "Μόνο βλαστική περίοδος Μαρτ-Σεπτ. Χειμώνα ΠΑΥΣΗ.", 4.0m, FertilizerQuantityUnit.Millilitre, "Σε 800 ml νερό"),
        new(32, 21, 45, 45, 60, "Μόνο βλαστική περίοδος Μαρτ-Σεπτ. Χειμώνα ΠΑΥΣΗ.", 3.0m, FertilizerQuantityUnit.Millilitre, "Σε 600 ml νερό"),
        new(33, 21, 30, 30, 60, "Μόνο βλαστική περίοδος Μαρτ-Σεπτ. Χειμώνα ΠΑΥΣΗ.", 2.5m, FertilizerQuantityUnit.Millilitre, "Σε 500 ml νερό"),
        new(34, 21, 45, 45, 60, "Μόνο βλαστική περίοδος Μαρτ-Σεπτ. Χειμώνα ΠΑΥΣΗ.", 2.5m, FertilizerQuantityUnit.Millilitre, "Σε 500 ml νερό"),
        new(35, 21, 30, 30, 60, "Μόνο βλαστική περίοδος Μαρτ-Σεπτ. Χειμώνα ΠΑΥΣΗ.", 6.5m, FertilizerQuantityUnit.Millilitre, "Σε 1600 ml νερό"),
        new(36, 21, 30, 30, 60, "Μόνο βλαστική περίοδος Μαρτ-Σεπτ. Χειμώνα ΠΑΥΣΗ.", 6.5m, FertilizerQuantityUnit.Millilitre, "Σε 1600 ml νερό"),
        new(37, 21, 45, 45, 60, "Μισή δόση. Η Δρακαίνα ΔΕΝ είναι οξύφιλο — αποφύγετε οξίνιση χώματος.", 25.0m, FertilizerQuantityUnit.Gram, "Ανά εφαρμογή")
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

        foreach (var entry in Entries)
        {
            if (entry.InstanceTempId <= 0 || entry.InstanceTempId > plantInstanceIdsOrderedByTempId.Count)
                continue;

            var instanceId = plantInstanceIdsOrderedByTempId[entry.InstanceTempId - 1];

            if (!FertilizerKeyByCsvId.TryGetValue(entry.FertilizerCsvId, out var fertilizerKey))
                continue;

            if (!fertilizerIdsByKey.TryGetValue(fertilizerKey, out var fertilizerId))
                continue;

            AddSeasonRecord(records, now, instanceId, fertilizerId, GardenSeason.Spring, entry.SpringDays, entry);
            AddSeasonRecord(records, now, instanceId, fertilizerId, GardenSeason.Summer, entry.SummerDays, entry);
            AddSeasonRecord(records, now, instanceId, fertilizerId, GardenSeason.Autumn, entry.AutumnDays, entry);
        }

        _dbContext.FertilizingSchedules.AddRange(records);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static void AddSeasonRecord(
        ICollection<FertilizingSchedule> records,
        DateTime now,
        int instanceId,
        int fertilizerId,
        GardenSeason season,
        byte? frequencyDays,
        FertilizerEntry entry)
    {
        if (!frequencyDays.HasValue || frequencyDays.Value == 0)
            return;

        records.Add(new FertilizingSchedule
        {
            OwnerId = DemoOwnerBusinessId,
            PlantInstanceId = instanceId,
            FertilizerId = fertilizerId,
            Season = season,
            FrequencyDays = frequencyDays.Value,
            Quantity = entry.Quantity,
            Unit = entry.Unit,
            Notes = entry.Notes,
            DilutionInstructions = entry.DilutionInstructions,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        });
    }

    private sealed record FertilizerEntry(
        int InstanceTempId,
        int FertilizerCsvId,
        byte? SpringDays,
        byte? SummerDays,
        byte? AutumnDays,
        string? Notes,
        decimal? Quantity,
        FertilizerQuantityUnit? Unit,
        string? DilutionInstructions);
}
