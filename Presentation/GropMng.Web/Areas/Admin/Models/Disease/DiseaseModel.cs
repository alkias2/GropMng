using GropMng.Core.Domain.Garden.Enums;
using GropMng.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GropMng.Web.Areas.Admin.Models.Disease;

/// <summary>
/// Represents the create/edit model for a plant disease catalog entry.
/// </summary>
public class DiseaseModel
{
    public int Id { get; set; }

    [GropResourceDisplayName("admin.disease.fields.name")]
    public string Name { get; set; } = string.Empty;

    [GropResourceDisplayName("admin.disease.fields.diseasetype")]
    public PlantDiseaseType DiseaseType { get; set; } = PlantDiseaseType.Other;

    [GropResourceDisplayName("admin.disease.fields.symptoms")]
    public string? Symptoms { get; set; }

    [GropResourceDisplayName("admin.disease.fields.affectedparts")]
    public string? AffectedParts { get; set; }

    [GropResourceDisplayName("admin.disease.fields.preventionnotes")]
    public string? PreventionNotes { get; set; }

    [GropResourceDisplayName("admin.disease.fields.notes")]
    public string? Notes { get; set; }

    // Dropdown support
    public List<SelectListItem> AvailableDiseaseTypes { get; set; } = new();
}
