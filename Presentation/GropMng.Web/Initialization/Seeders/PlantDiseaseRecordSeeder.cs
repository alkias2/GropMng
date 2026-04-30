using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Health;
using GropMng.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace GropMng.Web.Initialization.Seeders;

internal sealed class PlantDiseaseRecordSeeder
{
    private static readonly Guid DemoOwnerBusinessId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    // 6 records from Plant diseases.csv
    // (TempId, DiseaseName, DetectedDate, Severity, TreatmentUsed, Outcome)
    private static readonly DiseaseRecordEntry[] Entries =
    [
        new(4,  "Τετράνυχος (Tetranychus urticae)",        new DateOnly(2026,4,26), PlantDiseaseSeverity.Mild,     "Neem Oil",                                                                          PlantDiseaseOutcome.Resolved),
        new(6,  "Hyphantria cunea",                        new DateOnly(2025,5,27), PlantDiseaseSeverity.Moderate,  "Αφαίρεση με το χέρι",                                                               PlantDiseaseOutcome.Resolved),
        new(13, "leaf spot / Cercospora – Alternaria",     new DateOnly(2026,1,10), PlantDiseaseSeverity.Moderate,  "Neem Oil",                                                                          PlantDiseaseOutcome.Resolved),
        new(16, "Αφροψύλλα (cuckoo spit / Philaenus spumarius)", new DateOnly(2026,3,9), PlantDiseaseSeverity.Severe, "Αφαίρεση με το χέρι",                                                           PlantDiseaseOutcome.Resolved),
        new(25, "Φυλλοκνίστης (Leaf miner)",              new DateOnly(2025,6,28), PlantDiseaseSeverity.Severe,    "Ψέκασμα με Shooter 200 SL 1 ml/L, επανάληψη μετά 7 ημέρες",                       PlantDiseaseOutcome.Resolved),
        new(26, "broad mite / eriophyid mites",            new DateOnly(2026,1,4),  PlantDiseaseSeverity.Moderate,  "Neem Oil",                                                                          PlantDiseaseOutcome.Resolved)
    ];

    private readonly GropContext _dbContext;

    public PlantDiseaseRecordSeeder(GropContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedAsync(
        IReadOnlyList<int> plantInstanceIdsOrderedByTempId,
        IReadOnlyDictionary<string, int> diseaseIdsByName,
        CancellationToken cancellationToken = default)
    {
        var anyExisting = await _dbContext.PlantDiseaseRecords
            .AnyAsync(r => r.OwnerId == DemoOwnerBusinessId, cancellationToken);

        if (anyExisting)
            return;

        var now = DateTime.UtcNow;
        var records = new List<PlantDiseaseRecord>();

        foreach (var entry in Entries)
        {
            var instanceId = plantInstanceIdsOrderedByTempId[entry.TempId - 1];

            if (!diseaseIdsByName.TryGetValue(entry.DiseaseName, out var diseaseId))
                continue;

            records.Add(new PlantDiseaseRecord
            {
                OwnerId = DemoOwnerBusinessId,
                PlantInstanceId = instanceId,
                DiseaseId = diseaseId,
                DetectedDate = entry.DetectedDate,
                Severity = entry.Severity,
                TreatmentUsed = entry.TreatmentUsed,
                Outcome = entry.Outcome,
                ResolvedDate = entry.Outcome == PlantDiseaseOutcome.Resolved ? entry.DetectedDate.AddDays(14) : null,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            });
        }

        _dbContext.PlantDiseaseRecords.AddRange(records);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private sealed record DiseaseRecordEntry(
        int TempId,
        string DiseaseName,
        DateOnly DetectedDate,
        PlantDiseaseSeverity Severity,
        string TreatmentUsed,
        PlantDiseaseOutcome Outcome);
}
