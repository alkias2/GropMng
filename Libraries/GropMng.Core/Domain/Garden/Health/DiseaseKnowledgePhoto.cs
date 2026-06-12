namespace GropMng.Core.Domain.Garden.Health;

public partial class DiseaseKnowledgePhoto : AuditableEntity
{
    public int DiseaseKnowledgeId { get; set; }

    public int PictureId { get; set; }

    public int DisplayOrder { get; set; }

    public string? Caption { get; set; }

    public DiseaseKnowledge DiseaseKnowledge { get; set; } = null!;
}