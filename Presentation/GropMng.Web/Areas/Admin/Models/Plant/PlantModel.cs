using GropMng.Core.Domain.Garden.Enums;
using GropMng.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GropMng.Web.Areas.Admin.Models.Plant;

/// <summary>
/// Create/edit model for plant catalog entries.
/// </summary>
public class PlantModel
{
    public int Id { get; set; }

    [GropResourceDisplayName("admin.plant.fields.commonname")]
    public string CommonName { get; set; } = string.Empty;

    [GropResourceDisplayName("admin.plant.fields.scientificname")]
    public string ScientificName { get; set; } = string.Empty;

    [GropResourceDisplayName("admin.plant.fields.family")]
    public string? Family { get; set; }

    [GropResourceDisplayName("admin.plant.fields.category")]
    public PlantCategory Category { get; set; } = PlantCategory.Other;

    [GropResourceDisplayName("admin.plant.fields.growthtype")]
    public PlantGrowthType? GrowthType { get; set; }

    [GropResourceDisplayName("admin.plant.fields.sunrequirement")]
    public PlantSunRequirement? SunRequirement { get; set; }

    [GropResourceDisplayName("admin.plant.fields.waterrequirement")]
    public PlantWaterRequirement? WaterRequirement { get; set; }

    [GropResourceDisplayName("admin.plant.fields.mintempcelsius")]
    public decimal? MinTempCelsius { get; set; }

    [GropResourceDisplayName("admin.plant.fields.maxtempcelsius")]
    public decimal? MaxTempCelsius { get; set; }

    [GropResourceDisplayName("admin.plant.fields.isedible")]
    public bool IsEdible { get; set; }

    [GropResourceDisplayName("admin.plant.fields.ismedicinal")]
    public bool IsMedicinal { get; set; }

    [GropResourceDisplayName("admin.plant.fields.istoxic")]
    public bool IsToxic { get; set; }

    [GropResourceDisplayName("admin.plant.fields.generalnotes")]
    public string? GeneralNotes { get; set; }

    public IList<SelectListItem> AvailableCategories { get; set; } = new List<SelectListItem>();

    public IList<SelectListItem> AvailableGrowthTypes { get; set; } = new List<SelectListItem>();

    public IList<SelectListItem> AvailableSunRequirements { get; set; } = new List<SelectListItem>();

    public IList<SelectListItem> AvailableWaterRequirements { get; set; } = new List<SelectListItem>();
}
