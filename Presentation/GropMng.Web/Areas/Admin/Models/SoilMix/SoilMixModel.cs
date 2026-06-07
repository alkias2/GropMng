using GropMng.Core.Domain.Garden.Enums;
using GropMng.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GropMng.Web.Areas.Admin.Models.SoilMix;

/// <summary>
/// Create/edit model for SoilMix catalog entries.
/// </summary>
public class SoilMixModel
{
    public int Id { get; set; }

    [GropResourceDisplayName("admin.soilmix.fields.name")]
    public string Name { get; set; } = string.Empty;

    [GropResourceDisplayName("admin.soilmix.fields.composition")]
    public string? Composition { get; set; }

    [GropResourceDisplayName("admin.soilmix.fields.phmin")]
    public decimal? PhMin { get; set; }

    [GropResourceDisplayName("admin.soilmix.fields.phmax")]
    public decimal? PhMax { get; set; }

    [GropResourceDisplayName("admin.soilmix.fields.texture")]
    public SoilTextureType? Texture { get; set; }

    [GropResourceDisplayName("admin.soilmix.fields.drainage")]
    public SoilDrainageType? Drainage { get; set; }

    [GropResourceDisplayName("admin.soilmix.fields.notes")]
    public string? Notes { get; set; }

    public IList<SelectListItem> AvailableTextures { get; set; } = new List<SelectListItem>();
    public IList<SelectListItem> AvailableDrainages { get; set; } = new List<SelectListItem>();
}
