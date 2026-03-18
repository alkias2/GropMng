namespace GropMng.Core.Domain.Garden.Plants;

public partial class PlantNote : AuditableEntity
{
    public required string OwnerId { get; set; }

    public int PlantInstanceId { get; set; }

    public string? Title { get; set; }

    public required string RichHtmlContent { get; set; }

    public string? Tags { get; set; }

    public PlantInstance PlantInstance { get; set; } = null!;
}