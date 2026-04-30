namespace GropMng.Core.Domain.Garden.Plants;

/// <summary>
/// A single ingredient line within a SoilMix recipe, with its percentage by volume.
/// </summary>
public partial class SoilMixIngredient : AuditableEntity
{
    public int SoilMixId { get; set; }

    public int SoilIngredientId { get; set; }

    /// <summary>
    /// Percentage by volume (0–100). Sum of all ingredients for a given SoilMix should equal 100.
    /// </summary>
    public decimal PercentageByVolume { get; set; }

    public string? Notes { get; set; }

    public SoilMix SoilMix { get; set; } = null!;

    public SoilIngredient SoilIngredient { get; set; } = null!;
}
