using GropMng.Core.Domain.Garden.Plants;

namespace GropMng.Core.Domain.Garden.Health;

public partial class DiseaseKnowledgePlant : AuditableEntity
{
    public int DiseaseKnowledgeId { get; set; }

    public int PlantId { get; set; }

    public DiseaseKnowledge DiseaseKnowledge { get; set; } = null!;

    public Plant Plant { get; set; } = null!;
}