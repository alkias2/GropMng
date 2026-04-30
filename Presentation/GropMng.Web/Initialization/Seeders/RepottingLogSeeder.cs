using GropMng.Core.Domain.Garden.Care;
using GropMng.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace GropMng.Web.Initialization.Seeders;

internal sealed class RepottingLogSeeder
{
    private static readonly Guid DemoOwnerBusinessId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    // 4 records from Plant-Repoting.csv
    private static readonly RepottingEntry[] Entries =
    [
        new(1,  new DateOnly(2026,4,22), soilMixChanged: true,  "Μεταφορά σε μεγαλύτερη γλάστρα"),
        new(3,  new DateOnly(2026,4,15), soilMixChanged: true,  "Μεταφορά σε μεγαλύτερη γλάστρα"),
        new(11, new DateOnly(2026,4,16), soilMixChanged: true,  "Μεταφορά σε μεγαλύτερη γλάστρα"),
        new(24, new DateOnly(2025,8,19), soilMixChanged: true,  "Μεταφορά σε μεγαλύτερη γλάστρα")
    ];

    private readonly GropContext _dbContext;

    public RepottingLogSeeder(GropContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedAsync(
        IReadOnlyList<int> plantInstanceIdsOrderedByTempId,
        CancellationToken cancellationToken = default)
    {
        var anyExisting = await _dbContext.RepottingLogs
            .AnyAsync(r => r.OwnerId == DemoOwnerBusinessId, cancellationToken);

        if (anyExisting)
            return;

        var now = DateTime.UtcNow;
        var records = new List<RepottingLog>();

        foreach (var entry in Entries)
        {
            var instanceId = plantInstanceIdsOrderedByTempId[entry.TempId - 1];

            records.Add(new RepottingLog
            {
                OwnerId = DemoOwnerBusinessId,
                PlantInstanceId = instanceId,
                RepottedAtUtc = entry.RepottingDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                SoilMixChanged = entry.soilMixChanged,
                ContainerChanged = true, // all 4 were moved to larger containers
                Notes = entry.Notes,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            });
        }

        _dbContext.RepottingLogs.AddRange(records);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private sealed record RepottingEntry(int TempId, DateOnly RepottingDate, bool soilMixChanged, string? Notes = null);
}
