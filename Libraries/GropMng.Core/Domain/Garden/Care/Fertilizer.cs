using GropMng.Core.Domain.Garden.Enums;

namespace GropMng.Core.Domain.Garden.Care;

public partial class Fertilizer : AuditableEntity
{
    public required string Name { get; set; }

    public string? Brand { get; set; }

    public FertilizerKind? FertilizerType { get; set; }

    public string? NpkRatio { get; set; }

    public FertilizerApplicationMethod? ApplicationMethod { get; set; }

    public bool IsOrganic { get; set; }

    public string? Notes { get; set; }

    public IList<FertilizingSchedule> FertilizingSchedules { get; set; } = new List<FertilizingSchedule>();
}