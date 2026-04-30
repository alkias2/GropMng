namespace GropMng.Core.Domain.Garden.Care;

/// <summary>
/// Records a repotting event — change of container and/or soil mix for a plant instance.
/// </summary>
public partial class RepottingLog : AuditableEntity
{
    public Guid OwnerId { get; set; }

    public int PlantInstanceId { get; set; }

    public int? PreviousContainerId { get; set; }

    public int? NewContainerId { get; set; }

    public int? PreviousSoilMixId { get; set; }

    public int? NewSoilMixId { get; set; }

    public DateTime RepottedAtUtc { get; set; }

    public bool SoilMixChanged { get; set; }

    public bool ContainerChanged { get; set; }

    public string? Notes { get; set; }

    public Plants.PlantInstance PlantInstance { get; set; } = null!;
}
