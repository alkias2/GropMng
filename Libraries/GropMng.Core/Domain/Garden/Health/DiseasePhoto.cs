namespace GropMng.Core.Domain.Garden.Health;

public partial class DiseasePhoto : AuditableEntity
{
    public Guid OwnerId { get; set; }

    public int PlantDiseaseRecordId { get; set; }

    public required string FilePath { get; set; }

    public string? ThumbnailPath { get; set; }

    public DateOnly TakenDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public string? Notes { get; set; }

    public PlantDiseaseRecord PlantDiseaseRecord { get; set; } = null!;
}
