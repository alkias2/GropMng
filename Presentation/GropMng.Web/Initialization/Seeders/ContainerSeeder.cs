using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Plants;
using GropMng.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace GropMng.Web.Initialization.Seeders;

internal sealed class ContainerSeeder
{
    private static readonly Guid DemoOwnerBusinessId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    // 37 containers from Plant-Containers.csv
    // Columns: (ContainerType, Material, BaseCircumferenceCm, RimCircumferenceCm, HeightCm, VolumeL)
    // TempId index matches position in array (0-based = TempId - 1).
    // Entries 6 and 27 are ground-planted (Παρτέρι) — no real pot dimensions.
    private static readonly ContainerEntry[] Containers =
    [
        new(GardenContainerType.Pot, "Πήλινη",   67,   95,  28, 14.8m),  // 1
        new(GardenContainerType.Pot, "Πήλινη",   60,   95,  24, 11.7m),  // 2
        new(GardenContainerType.Pot, "Πήλινη",   74,  125,  35, 28.2m),  // 3
        new(GardenContainerType.Pot, "Πλαστική",  80,  107,  30, 21.0m),  // 4
        new(GardenContainerType.Pot, "Πλαστική",  78,  100,  25, 15.8m),  // 5
        new(GardenContainerType.Bed, "Παρτέρι",  null, null, null, 0m),   // 6 — ground
        new(GardenContainerType.Pot, "Πλαστική",  93,  130,  32, 31.9m),  // 7
        new(GardenContainerType.Pot, "Πήλινη",   80,  100,  15,  9.7m),  // 8
        new(GardenContainerType.Pot, "Πλαστική",  54,   70,  20,  6.2m),  // 9
        new(GardenContainerType.Pot, "Πλαστική",  80,  104,  27, 18.3m),  // 10
        new(GardenContainerType.Pot, "Πήλινη",   80,  107,  30, 21.0m),  // 11
        new(GardenContainerType.Pot, "Πήλινη",   80,  124,  40, 33.6m),  // 12
        new(GardenContainerType.Pot, "Πήλινη",   72,   94,  25, 13.8m),  // 13
        new(GardenContainerType.Pot, "Πλαστική",  55,   67,  20,  5.9m),  // 14
        new(GardenContainerType.Pot, "Πήλινη",   62,   83,  23,  9.7m),  // 15
        new(GardenContainerType.Pot, "Πήλινη",   54,   74,  20,  6.6m),  // 16
        new(GardenContainerType.Pot, "Πήλινη",   72,  100,  27, 16.0m),  // 17
        new(GardenContainerType.Pot, "Πήλινη",   54,   74,  20,  6.6m),  // 18
        new(GardenContainerType.Pot, "Πήλινη",   70,  107,  12,  7.6m),  // 19
        new(GardenContainerType.Pot, "Πλαστική",  45,   63,  16,  3.7m),  // 20
        new(GardenContainerType.Pot, "Πλαστική",  54,   66,  20,  5.7m),  // 21
        new(GardenContainerType.Pot, "Πήλινη",   72,   87,  20, 10.1m),  // 22
        new(GardenContainerType.Pot, "Πλαστική",  70,   92,  22, 11.6m),  // 23
        new(GardenContainerType.Pot, "Πήλινη",   75,   86,  22, 11.4m),  // 24
        new(GardenContainerType.Pot, "Πήλινη",   90,  125,  40, 37.1m),  // 25
        new(GardenContainerType.Pot, "Πήλινη",   80,  120,  34, 27.4m),  // 26
        new(GardenContainerType.Bed, "Παρτέρι",  null, null, null, 0m),   // 27 — ground
        new(GardenContainerType.Pot, "Πήλινη",   72,  144,  25, 24.1m),  // 28
        new(GardenContainerType.Pot, "Πλαστική",  55,   64,  20,  5.6m),  // 29
        new(GardenContainerType.Pot, "Πλαστική",  55,   64,  20,  5.6m),  // 30
        new(GardenContainerType.Pot, "Πήλινη",   50,   75,  22,  6.9m),  // 31
        new(GardenContainerType.Pot, "Πήλινη",   70,   86,  11,  5.3m),  // 32
        new(GardenContainerType.Pot, "Πλαστική",  45,   65,  18,  4.4m),  // 33
        new(GardenContainerType.Pot, "Πλαστική",  45,   65,  18,  4.4m),  // 34
        new(GardenContainerType.Pot, "Πήλινη",   80,  110,  18, 13.0m),  // 35
        new(GardenContainerType.Pot, "Πήλινη",   80,  110,  18, 13.0m),  // 36
        new(GardenContainerType.Pot, "Πλαστική",  70,   90,  25, 12.8m)  // 37
    ];

    private readonly GropContext _dbContext;

    public ContainerSeeder(GropContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Seeds containers linked to the supplied plant instance IDs (one container per instance, by TempId position).
    /// Ground-planted instances (TempId 6, 27) receive a Bed-type container with no circumference data.
    /// </summary>
    /// <param name="plantInstanceIdsOrderedByTempId">
    /// Ordered list of planted instance IDs (index 0 = TempId 1, ..., index 36 = TempId 37).
    /// </param>
    public async Task<IReadOnlyList<int>> SeedAsync(
        IReadOnlyList<int> plantInstanceIdsOrderedByTempId,
        CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Containers
            .Where(c => c.OwnerId == DemoOwnerBusinessId && !c.IsDeleted)
            .CountAsync(cancellationToken);

        var existingContainerInstanceIds = await _dbContext.Containers
            .Where(c => c.OwnerId == DemoOwnerBusinessId && !c.IsDeleted && c.PlantInstanceId.HasValue)
            .Select(c => c.PlantInstanceId!.Value)
            .ToListAsync(cancellationToken);

        var existingInstanceIdSet = existingContainerInstanceIds.ToHashSet();

        if (existing >= Containers.Length)
        {
            return await _dbContext.Containers
                .Where(c => c.OwnerId == DemoOwnerBusinessId && !c.IsDeleted)
                .OrderBy(c => c.Id)
                .Select(c => c.Id)
                .ToListAsync(cancellationToken);
        }

        var now = DateTime.UtcNow;
        for (var i = 0; i < Containers.Length; i++)
        {
            var c = Containers[i];
            var instanceId = i < plantInstanceIdsOrderedByTempId.Count
                ? plantInstanceIdsOrderedByTempId[i]
                : (int?)null;

            if (instanceId.HasValue && existingInstanceIdSet.Contains(instanceId.Value))
                continue;

            var container = new Container
            {
                OwnerId = DemoOwnerBusinessId,
                PlantInstanceId = instanceId,
                ContainerType = c.ContainerType,
                Material = c.MaterialLabel,
                BaseCircumferenceCm = c.BaseCircumferenceCm,
                RimCircumferenceCm = c.RimCircumferenceCm,
                HeightCm = c.HeightCm,
                VolumeL = c.ContainerType == GardenContainerType.Bed ? 0m : (c.VolumeL > 0 ? c.VolumeL : null),
                HasDrainageHole = c.ContainerType != GardenContainerType.Bed,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            };
            _dbContext.Containers.Add(container);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await _dbContext.Containers
            .Where(c => c.OwnerId == DemoOwnerBusinessId && !c.IsDeleted)
            .OrderBy(c => c.Id)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);
    }

    private sealed record ContainerEntry(
        GardenContainerType ContainerType,
        string MaterialLabel,
        decimal? BaseCircumferenceCm,
        decimal? RimCircumferenceCm,
        decimal? HeightCm,
        decimal VolumeL);
}
