namespace GropMng.Core.Domain.Garden.Plants;

/// <summary>
/// A catalog entry for a single soil ingredient (e.g. Perlite, Pumice, Coco Coir).
/// Shared across all soil mix recipes.
/// </summary>
public partial class SoilIngredient : AuditableEntity
{
    public required string Name { get; set; }

    public string? Description { get; set; }

    public IList<SoilMixIngredient> SoilMixIngredients { get; set; } = new List<SoilMixIngredient>();
}
