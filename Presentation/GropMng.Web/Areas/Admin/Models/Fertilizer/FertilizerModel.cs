using GropMng.Core.Domain.Garden.Enums;
using GropMng.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GropMng.Web.Areas.Admin.Models.Fertilizer;

/// <summary>
/// Create/edit model for fertilizer catalog entries.
/// </summary>
public class FertilizerModel
{
    public int Id { get; set; }

    [GropResourceDisplayName("admin.fertilizer.fields.name")]
    public string Name { get; set; } = string.Empty;

    [GropResourceDisplayName("admin.fertilizer.fields.brand")]
    public string? Brand { get; set; }

    [GropResourceDisplayName("admin.fertilizer.fields.fertilizertype")]
    public FertilizerKind? FertilizerType { get; set; }

    [GropResourceDisplayName("admin.fertilizer.fields.npkratio")]
    public string? NpkRatio { get; set; }

    [GropResourceDisplayName("admin.fertilizer.fields.applicationmethod")]
    public FertilizerApplicationMethod? ApplicationMethod { get; set; }

    [GropResourceDisplayName("admin.fertilizer.fields.isorganic")]
    public bool IsOrganic { get; set; }

    [GropResourceDisplayName("admin.fertilizer.fields.notes")]
    public string? Notes { get; set; }

    public IList<SelectListItem> AvailableFertilizerTypes { get; set; } = new List<SelectListItem>();
    public IList<SelectListItem> AvailableApplicationMethods { get; set; } = new List<SelectListItem>();
}
