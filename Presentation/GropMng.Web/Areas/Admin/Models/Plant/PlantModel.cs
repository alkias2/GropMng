using GropMng.Core.Domain.Garden.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GropMng.Web.Areas.Admin.Models.Plant;

/// <summary>
/// Create/edit model for plant catalog entries.
/// </summary>
public class PlantModel
{
    public int Id { get; set; }

    public string CommonName { get; set; } = string.Empty;

    public string ScientificName { get; set; } = string.Empty;

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

    public IList<SelectListItem> AvailableCategories { get; set; } = new List<SelectListItem>();

    public IList<SelectListItem> AvailableGrowthTypes { get; set; } = new List<SelectListItem>();

    public IList<SelectListItem> AvailableSunRequirements { get; set; } = new List<SelectListItem>();

    public IList<SelectListItem> AvailableWaterRequirements { get; set; } = new List<SelectListItem>();
}
