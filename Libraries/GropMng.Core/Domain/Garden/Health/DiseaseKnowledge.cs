namespace GropMng.Core.Domain.Garden.Health;

public partial class DiseaseKnowledge : AuditableEntity
{
    public required string CommonName { get; set; }

    public string? ScientificName { get; set; }

    public required string Description { get; set; }

    public required string TreatmentGuidelines { get; set; }

    public IList<DiseaseKnowledgePhoto> Photos { get; set; } = new List<DiseaseKnowledgePhoto>();

    public IList<DiseaseKnowledgePlant> PlantLinks { get; set; } = new List<DiseaseKnowledgePlant>();
}