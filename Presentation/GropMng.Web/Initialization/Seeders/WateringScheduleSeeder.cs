using GropMng.Core.Domain.Garden.Care;
using GropMng.Core.Domain.Garden.Enums;
using GropMng.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace GropMng.Web.Initialization.Seeders;

internal sealed class WateringScheduleSeeder
{
    private static readonly Guid DemoOwnerBusinessId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    // 37 rows from Plants-Watering-and-Last-Watering.csv
    // (WinterFreqDays, WinterLitres, SpringFreqDays, SpringLitres, SummerFreqDays, SummerLitres, LastWatered, Notes)
    private static readonly WateringEntry[] Entries =
    [
        new(7,  0.5m, 4, 0.7m,  2,  1.0m, new DateOnly(2026,4,26), "Πολύ ευαίσθητο στην ξηρασία. Καλοκαίρι: βράδυ/πρωί. Πήλινη γλ. στεγνώνει γρήγορα."),
        new(7,  0.5m, 4, 0.7m,  2,  1.0m, new DateOnly(2026,4,26), "Ίδιο με Νο1. Παρακολούθηση ξήρανσης εδάφους λόγω πήλινης."),
        new(7,  1.5m, 5, 2.0m,  3,  2.5m, new DateOnly(2026,4,25), "Τριανταφυλλιά: αποφύγετε ύγρανση φύλλων. Ποτίστε στη βάση."),
        new(7,  1.0m, 5, 1.5m,  3,  2.0m, new DateOnly(2026,4,25), "Γιασεμί: αγαπά υγρασία αλλά όχι στράγγισμα. Πλαστική κρατά υγρασία."),
        new(7,  1.0m, 5, 1.0m,  3,  1.5m, new DateOnly(2026,4,25), "Αστεροειδές γιασεμί: ανθεκτικό, λιγότερο απαιτητικό από κοινό γιασεμί."),
        new(10, 2.0m, 6, 3.0m,  3,  4.0m, new DateOnly(2026,4,19), "Παρτέρι: ποτίστε βαθύτερα και αραιότερα. Εκτίμηση βάσει εποχής."),
        new(10, 1.5m, 7, 2.0m,  5,  2.5m, new DateOnly(2026,4,19), "Λεβάντα: απεχθάνεται συχνό πότισμα. Αφήστε να στεγνώσει μεταξύ ποτισμάτων."),
        new(14, 0.3m,10, 0.5m,  7,  0.7m, new DateOnly(2026,4,25), "Sedum: εξαιρετικά ανθεκτικό στην ξηρασία. Ελάχιστο νερό τον χειμώνα."),
        new(7,  0.5m, 5, 0.7m,  3,  1.0m, new DateOnly(2026,4,19), "Αγαπάνθος: μειώστε σημαντικά τον χειμώνα (ληθαργική φάση)."),
        new(10, 1.0m, 7, 1.5m,  4,  2.0m, new DateOnly(2026,4,19), "Πεύκο: μόλις εγκατασταθεί ανθίσταται στην ξηρασία. Νεαρό φυτό χρειάζεται τακτικό πότισμα."),
        new(10, 1.0m, 7, 1.5m,  4,  2.0m, new DateOnly(2026,4,25), "Βουκαμβίλια: ανθίζει καλύτερα με ήπιο στρες ξηρασίας. Μη υπερποτίζετε."),
        new(7,  2.0m, 5, 3.0m,  3,  4.0m, new DateOnly(2026,4,19), "Λεμονιά: απαιτεί σταθερή υγρασία. Ελέγξτε pH νερού (αποφύγετε ασβεστούχο)."),
        new(7,  0.5m, 5, 0.7m,  3,  1.0m, new DateOnly(2026,4,26), "Βασιλικός σε μερική σκιά: λίγο λιγότερο νερό από Ζώνη Α."),
        new(14, 0.3m,10, 0.4m,  7,  0.5m, new DateOnly(2026,4,19), "Crown of Thorns: παχύφυτο, αντέχει ξηρασία. Χειμώνας: ελάχιστο νερό."),
        new(5,  0.5m, 3, 0.7m,  2,  1.0m, new DateOnly(2026,4,26), "Μέντα: υγρόφιλη. Ελέγχετε τακτικά το έδαφος. Καλοκαίρι: καθημερινό έλεγχο."),
        new(5,  0.4m, 3, 0.6m,  2,  0.8m, new DateOnly(2026,4,26), "Δυόσμος: ίδια λογική με μέντα. Αποφύγετε στράγγισμα."),
        new(7,  1.0m, 5, 1.5m,  3,  2.0m, new DateOnly(2026,4,26), "Τριανταφυλλιά σε μερική σκιά: λίγο λιγότερο νερό, ίδια προσοχή στα φύλλα."),
        new(14, 0.3m,10, 0.4m, 14,  0.5m, new DateOnly(2026,4,5),  "Αλόη: παχύφυτο. Καλοκαίρι ΔΕΝ αυξάνετε συχνότητα — κίνδυνος σήψης ρίζας."),
        new(14, 0.3m,10, 0.4m, 14,  0.5m, new DateOnly(2026,4,5),  "Αλόη maculata (μεγάλη γλ.): ίδιο με Νο18."),
        new(10, 0.5m, 7, 0.7m,  5,  1.0m, new DateOnly(2026,4,5),  "Sago Palm: αργή ανάπτυξη, ευαίσθητη στην υπερποτιστική. Αφήστε να στεγνώσει."),
        new(7,  0.4m, 5, 0.5m,  4,  0.7m, new DateOnly(2026,4,19), "Πελαργόνιο: μέτρια υγρασία. Αποφύγετε ύγρανση φύλλων (μύκητες)."),
        new(10, 0.5m, 7, 0.8m,  5,  1.0m, new DateOnly(2026,4,26), "Ρίγανη: μεσογειακό βότανο, ανθεκτικό στην ξηρασία. Ελάχιστο νερό χειμώνα."),
        new(5,  0.7m, 4, 1.0m,  3,  1.2m, new DateOnly(2026,4,26), "Χρυσάνθεμο: χρειάζεται σταθερή υγρασία ιδίως κατά την άνθηση."),
        new(14, 0.5m,10, 0.7m, 14,  0.8m, new DateOnly(2026,4,5),  "Αλόη vera: παχύφυτο. Χειμώνας: σχεδόν μηδέν νερό. Εποχιακή μείωση."),
        new(7,  2.0m, 5, 2.5m,  3,  3.5m, new DateOnly(2026,4,26), "Κουμκουάτ: μεγάλη γλάστρα, απαιτεί τακτικό πότισμα. Ζόνη Β ευνοεί υγρασία."),
        new(5,  1.5m, 4, 2.0m,  3,  2.5m, new DateOnly(2026,4,25), "Γαρδένια: αγαπά υγρασία αλλά όχι στράγγισμα. Νερό χωρίς ασβέστιο."),
        new(7,  3.0m, 5, 5.0m,  3,  7.0m, new DateOnly(2026,3,29), "Ροδακινιά (παρτέρι): βαθύ πότισμα. Σε ζώνη Γ: προσοχή στην υπερυγρασία."),
        new(10, 1.0m, 7, 1.5m,  5,  2.0m, new DateOnly(2026,4,25), "Δρακαίνα: ανθεκτική. Χειμώνας: αραιά. Αποφύγετε κρύο νερό."),
        new(7,  0.5m, 5, 0.7m,  4,  0.8m, new DateOnly(2026,4,25), "Κλίβια: μειώστε πολύ τον χειμώνα (ανάπαυση). Ευαίσθητη στη σήψη ρίζας."),
        new(7,  0.5m, 5, 0.7m,  4,  0.8m, new DateOnly(2026,4,25), "Κλίβια: ίδιο με Νο29."),
        new(14, 0.3m,10, 0.4m, 14,  0.5m, new DateOnly(2026,4,12), "Κράσουλα: παχύφυτο. Ζώνη Γ με υψηλή υγρασία — ελάχιστο νερό ολοχρονίς."),
        new(14, 0.2m,10, 0.3m, 14,  0.4m, new DateOnly(2026,4,12), "String-of-Pearls: εξαιρετικά ευαίσθητο στην υπερποτιστική. Ελάχιστη ποσότητα."),
        new(21, 0.2m,14, 0.3m, 21,  0.4m, new DateOnly(2026,4,5),  "San Pedro Κάκτος: αντέχει ξηρασία. Ζώνη Γ: μεγαλύτερος κίνδυνος σήψης."),
        new(21, 0.2m,14, 0.3m, 21,  0.4m, new DateOnly(2026,4,5),  "Spear Sansevieria: σχεδόν αμελητέα ανάγκη νερού. Αποφύγετε ύγρανση κέντρου."),
        new(21, 0.5m,14, 0.7m, 21,  0.9m, new DateOnly(2026,4,5),  "Prickly Pear: ανθεκτικός κάκτος. Ζώνη Γ: πολύ αραιό πότισμα."),
        new(21, 0.5m,14, 0.7m, 21,  0.9m, new DateOnly(2026,4,5),  "Barrel Cactus: ίδιο με Νο35. Χειμώνας: μηδέν έως ελάχιστο."),
        new(10, 0.8m, 7, 1.0m,  5,  1.5m, new DateOnly(2026,4,25), "Δρακαίνα (Ζώνη Γ): ίδια λογική με Νο28 αλλά μικρότερη γλάστρα.")
    ];

    private readonly GropContext _dbContext;

    public WateringScheduleSeeder(GropContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedAsync(
        IReadOnlyList<int> plantInstanceIdsOrderedByTempId,
        CancellationToken cancellationToken = default)
    {
        var anyExisting = await _dbContext.WateringSchedules
            .AnyAsync(ws => ws.OwnerId == DemoOwnerBusinessId, cancellationToken);

        if (anyExisting)
            return;

        var now = DateTime.UtcNow;
        var (schedules, logs) = BuildEntities(plantInstanceIdsOrderedByTempId, now);

        _dbContext.WateringSchedules.AddRange(schedules);
        _dbContext.WateringLogs.AddRange(logs);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private (List<WateringSchedule> schedules, List<WateringLog> logs) BuildEntities(
        IReadOnlyList<int> instanceIds, DateTime now)
    {
        var schedules = new List<WateringSchedule>();
        var logs = new List<WateringLog>();

        (GardenSeason Season, Func<WateringEntry, byte> GetFreq, Func<WateringEntry, decimal> GetAmt)[] seasons =
        [
            (GardenSeason.Winter, e => e.WinterFreqDays, e => e.WinterLitres),
            (GardenSeason.Spring, e => e.SpringFreqDays, e => e.SpringLitres),
            (GardenSeason.Summer, e => e.SummerFreqDays, e => e.SummerLitres)
        ];

        for (var i = 0; i < Entries.Length; i++)
        {
            var entry = Entries[i];
            var instanceId = instanceIds[i];

            foreach (var (season, getFreq, getAmt) in seasons)
            {
                schedules.Add(new WateringSchedule
                {
                    OwnerId = DemoOwnerBusinessId,
                    PlantInstanceId = instanceId,
                    Season = season,
                    FrequencyDays = (byte)getFreq(entry),
                    WaterAmountL = getAmt(entry),
                    TimeOfDay = GardenTimeOfDay.Morning,
                    Notes = entry.Notes,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                });
            }

            // Last watering as the initial WateringLog entry
            logs.Add(new WateringLog
            {
                OwnerId = DemoOwnerBusinessId,
                PlantInstanceId = instanceId,
                WateredAtUtc = entry.LastWatered.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                WaterAmountL = entry.SpringLitres, // use spring amount as proxy
                Notes = "Τελευταίο πότισμα (initial seed)",
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            });
        }

        return (schedules, logs);
    }

    private sealed record WateringEntry(
        byte WinterFreqDays, decimal WinterLitres,
        byte SpringFreqDays, decimal SpringLitres,
        byte SummerFreqDays, decimal SummerLitres,
        DateOnly LastWatered,
        string? Notes = null);
}
