using GropMng.Core.Domain.Garden.Enums;

namespace GropMng.Core.Domain.Garden.Health;

public partial class Pesticide : AuditableEntity
{
    public required string Name { get; set; }

    public string? Brand { get; set; }

    public string? ActiveIngredient { get; set; }

    public PesticideKind? PesticideType { get; set; }

    public PesticideApplicationMethod? ApplicationMethod { get; set; }

    public bool IsOrganic { get; set; }

    public byte? WithholdingDays { get; set; }

    public string? SafetyNotes { get; set; }

    public string? Notes { get; set; }

    public IList<DiseaseRemedyLink> RemedyLinks { get; set; } = new List<DiseaseRemedyLink>();
}