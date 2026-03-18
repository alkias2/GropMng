namespace GropMng.Core.Domain.Garden.Plants;

public partial class PlantPhoto : AuditableEntity
{
    public required string OwnerId { get; set; }

    public int PlantInstanceId { get; set; }

    public required string FilePath { get; set; }

    public string? ThumbnailPath { get; set; }

    public DateOnly TakenDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public string? Caption { get; set; }

    public int SortOrder { get; set; }

    public PlantInstance PlantInstance { get; set; } = null!;
}