using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Plants;

namespace GropMng.Core.Domain.Garden.Health;

public partial class PlantDiseaseRecord : AuditableEntity
{
    public required string OwnerId { get; set; }

    public int PlantInstanceId { get; set; }

    public int DiseaseId { get; set; }

    public DateOnly DetectedDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public DateOnly? ResolvedDate { get; set; }

    public PlantDiseaseSeverity? Severity { get; set; } = PlantDiseaseSeverity.Moderate;

    public string? TreatmentUsed { get; set; }

    public PlantDiseaseOutcome? Outcome { get; set; } = PlantDiseaseOutcome.Ongoing;

    public string? Notes { get; set; }

    public PlantInstance PlantInstance { get; set; } = null!;

    public Disease Disease { get; set; } = null!;

    public IList<DiseasePhoto> Photos { get; set; } = new List<DiseasePhoto>();
}