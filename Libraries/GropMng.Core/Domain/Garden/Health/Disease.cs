using GropMng.Core.Domain.Garden.Enums;

namespace GropMng.Core.Domain.Garden.Health;

public partial class Disease : AuditableEntity
{
    public required string Name { get; set; }

    public PlantDiseaseType DiseaseType { get; set; } = PlantDiseaseType.Other;

    public string? Symptoms { get; set; }

    public string? PreventionNotes { get; set; }

    public string? AffectedParts { get; set; }

    public string? Notes { get; set; }

    public IList<DiseaseRemedyLink> RemedyLinks { get; set; } = new List<DiseaseRemedyLink>();

    public IList<PlantDiseaseRecord> PlantDiseaseRecords { get; set; } = new List<PlantDiseaseRecord>();
}