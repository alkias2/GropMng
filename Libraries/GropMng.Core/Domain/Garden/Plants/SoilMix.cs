using GropMng.Core.Domain.Garden.Enums;

namespace GropMng.Core.Domain.Garden.Plants;

public partial class SoilMix : AuditableEntity
{
    public required string Name { get; set; }

    public string? Composition { get; set; }

    public decimal? PhMin { get; set; }

    public decimal? PhMax { get; set; }

    public SoilTextureType? Texture { get; set; }

    public SoilDrainageType? Drainage { get; set; }

    public string? Notes { get; set; }

    public IList<PlantInstance> PlantInstances { get; set; } = new List<PlantInstance>();

    public IList<SoilMixIngredient> Ingredients { get; set; } = new List<SoilMixIngredient>();
}