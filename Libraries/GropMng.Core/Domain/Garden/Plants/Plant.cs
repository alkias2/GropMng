using System.ComponentModel.DataAnnotations;
using GropMng.Core.Domain.Garden.Enums;

namespace GropMng.Core.Domain.Garden.Plants;

public partial class Plant : AuditableEntity
{
    public required string CommonName { get; set; }

    public required string ScientificName { get; set; }

    public string? Family { get; set; }

    public PlantCategory Category { get; set; } = PlantCategory.Other;

    public PlantGrowthType? GrowthType { get; set; }

    public PlantSunRequirement? SunRequirement { get; set; }

    public PlantWaterRequirement? WaterRequirement { get; set; }

    public decimal? MinTempCelsius { get; set; }

    public decimal? MaxTempCelsius { get; set; }

    public bool IsEdible { get; set; }

    public bool IsMedicinal { get; set; }

    public bool IsToxic { get; set; }

    public string? GeneralNotes { get; set; }

    [UIHint("Picture")]
    public int PictureId { get; set; }

    public IList<PlantInstance> PlantInstances { get; set; } = new List<PlantInstance>();
}