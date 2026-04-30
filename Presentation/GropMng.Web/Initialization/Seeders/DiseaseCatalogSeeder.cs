using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Health;
using GropMng.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace GropMng.Web.Initialization.Seeders;

internal sealed class DiseaseCatalogSeeder
{
    // 6 unique diseases from Plant diseases.csv
    private static readonly DiseaseEntry[] Diseases =
    [
        new("Τετράνυχος (Tetranychus urticae)", PlantDiseaseType.Pest,
            "Spider mite — fine webs on underside of leaves, leaf stippling, yellowing"),
        new("Hyphantria cunea", PlantDiseaseType.Pest,
            "Fall webworm — silk nests, mass defoliation of branches"),
        new("leaf spot / Cercospora – Alternaria", PlantDiseaseType.Fungal,
            "Fungal leaf spot — brown/black spots with yellow halo, premature leaf drop"),
        new("Αφροψύλλα (cuckoo spit / Philaenus spumarius)", PlantDiseaseType.Pest,
            "Spittlebug — white frothy masses on stems, sucking sap, stunted growth"),
        new("Φυλλοκνίστης (Leaf miner)", PlantDiseaseType.Pest,
            "Leaf miner larvae — serpentine tunnels inside leaf tissue, distorted leaves"),
        new("broad mite / eriophyid mites", PlantDiseaseType.Pest,
            "Broad mite / eriophyid mite — distorted new growth, bronzing, flower deformation")
    ];

    private readonly GropContext _dbContext;

    public DiseaseCatalogSeeder(GropContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyDictionary<string, int>> SeedAsync(CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Diseases
            .Where(e => !e.IsDeleted)
            .ToDictionaryAsync(e => e.Name, e => e.Id, cancellationToken);

        var now = DateTime.UtcNow;
        foreach (var d in Diseases)
        {
            if (existing.ContainsKey(d.Name))
                continue;

            _dbContext.Diseases.Add(new Disease
            {
                Name = d.Name,
                DiseaseType = d.DiseaseType,
                Symptoms = d.Symptoms,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await _dbContext.Diseases
            .Where(e => !e.IsDeleted)
            .ToDictionaryAsync(e => e.Name, e => e.Id, cancellationToken);
    }

    private sealed record DiseaseEntry(string Name, PlantDiseaseType DiseaseType, string? Symptoms = null);
}
