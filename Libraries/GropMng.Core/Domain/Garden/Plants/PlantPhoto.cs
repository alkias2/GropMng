namespace GropMng.Core.Domain.Garden.Plants;

public partial class PlantPhoto : AuditableEntity
{
    public Guid OwnerId { get; set; }

    public int PlantInstanceId { get; set; }

    public int PictureId { get; set; }

    public DateOnly TakenDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public string? Caption { get; set; }

    public int DisplayOrder { get; set; }

    public PlantInstance PlantInstance { get; set; } = null!;
}
