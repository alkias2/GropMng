using GropMng.Core.Domain.Garden.Owners;
using GropMng.Core.Domain.Garden.Plants;

namespace GropMng.Core.Domain.Garden.Health;

public partial class AdminNotification : AuditableEntity
{
    public Guid OwnerId { get; set; }

    public int PlantInstanceId { get; set; }

    public required string ProblemName { get; set; }

    public bool IsResolved { get; set; }

    public DateTime? ResolvedAtUtc { get; set; }

    /// <summary>
    /// The DiseaseKnowledge entry that was created from this notification.
    /// Allows navigation from a resolved notification to edit the knowledge entry.
    /// </summary>
    public int? DiseaseKnowledgeId { get; set; }

    public Owner Owner { get; set; } = null!;

    public PlantInstance PlantInstance { get; set; } = null!;

    public DiseaseKnowledge? DiseaseKnowledge { get; set; }
}