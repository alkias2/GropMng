using GropMng.Core.Domain.Garden.Enums;
using GropMng.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GropMng.Web.Areas.Admin.Models.Pesticide;

/// <summary>
/// Create/edit model for pesticide catalog entries.
/// </summary>
public class PesticideModel
{
    public int Id { get; set; }

    [GropResourceDisplayName("admin.pesticide.fields.name")]
    public string Name { get; set; } = string.Empty;

    [GropResourceDisplayName("admin.pesticide.fields.brand")]
    public string? Brand { get; set; }

    [GropResourceDisplayName("admin.pesticide.fields.activeingredient")]
    public string? ActiveIngredient { get; set; }

    [GropResourceDisplayName("admin.pesticide.fields.pesticidetype")]
    public PesticideKind? PesticideType { get; set; }

    [GropResourceDisplayName("admin.pesticide.fields.applicationmethod")]
    public PesticideApplicationMethod? ApplicationMethod { get; set; }

    [GropResourceDisplayName("admin.pesticide.fields.isorganic")]
    public bool IsOrganic { get; set; }

    [GropResourceDisplayName("admin.pesticide.fields.withholdingdays")]
    public byte? WithholdingDays { get; set; }

    [GropResourceDisplayName("admin.pesticide.fields.safetynotes")]
    public string? SafetyNotes { get; set; }

    [GropResourceDisplayName("admin.pesticide.fields.notes")]
    public string? Notes { get; set; }

    public IList<SelectListItem> AvailablePesticideTypes { get; set; } = new List<SelectListItem>();
    public IList<SelectListItem> AvailableApplicationMethods { get; set; } = new List<SelectListItem>();
}
