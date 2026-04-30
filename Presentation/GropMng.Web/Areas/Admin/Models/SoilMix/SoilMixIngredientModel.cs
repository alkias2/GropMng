using GropMng.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GropMng.Web.Areas.Admin.Models.SoilMix;

/// <summary>
/// Input model for adding a SoilMix ingredient row (either existing or newly created SoilIngredient).
/// </summary>
public class SoilMixIngredientModel
{
    public int Id { get; set; }

    public int SoilMixId { get; set; }

    /// <summary>
    /// When true, creates a new SoilIngredient before linking it.
    /// When false, picks an existing SoilIngredient by <see cref="SoilIngredientId"/>.
    /// </summary>
    public bool IsNew { get; set; }

    // --- Existing ingredient path ---

    [GropResourceDisplayName("admin.soilmix.ingredient.fields.soilingredient")]
    public int SoilIngredientId { get; set; }

    // --- New ingredient path ---

    [GropResourceDisplayName("admin.soilmix.ingredient.fields.ingredientname")]
    public string? NewIngredientName { get; set; }

    [GropResourceDisplayName("admin.soilmix.ingredient.fields.ingredientdescription")]
    public string? NewIngredientDescription { get; set; }

    // --- Common fields ---

    [GropResourceDisplayName("admin.soilmix.ingredient.fields.percentage")]
    public decimal PercentageByVolume { get; set; }

    [GropResourceDisplayName("admin.soilmix.ingredient.fields.notes")]
    public string? Notes { get; set; }

    public IList<SelectListItem> AvailableSoilIngredients { get; set; } = new List<SelectListItem>();
}
