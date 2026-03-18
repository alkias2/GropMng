using GropMng.Core.Domain.Garden.Enums;

namespace GropMng.Core.Domain.Garden.Health;

public partial class DiseaseRemedyLink : AuditableEntity
{
    public int DiseaseId { get; set; }

    public int PesticideId { get; set; }

    public RemedyTreatmentType TreatmentType { get; set; } = RemedyTreatmentType.Curative;

    public string? Dosage { get; set; }

    public string? Frequency { get; set; }

    public string? Notes { get; set; }

    public Disease Disease { get; set; } = null!;

    public Pesticide Pesticide { get; set; } = null!;
}