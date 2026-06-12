using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Plants;

namespace GropMng.Core.Domain.Garden.Health;

public partial class PlantProblemRecord : AuditableEntity
{
    public Guid OwnerId { get; set; }

    public int PlantInstanceId { get; set; }

    public int? DiseaseKnowledgeId { get; set; }

    public required string ProblemName { get; set; }

    public DateOnly DetectedDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public Severity Severity { get; set; } = Severity.Medium;

    public ProblemStatus ProblemStatus { get; set; } = ProblemStatus.Active;

    public InfoSource InfoSource { get; set; } = InfoSource.OwnKnowledge;

    public string? Notes { get; set; }

    public DateOnly? ResolvedDate { get; set; }

    public bool NotifyAdmin { get; set; }

    public PlantInstance PlantInstance { get; set; } = null!;

    public DiseaseKnowledge? DiseaseKnowledge { get; set; }

    public IList<PlantProblemSchedule> Schedules { get; set; } = new List<PlantProblemSchedule>();
}