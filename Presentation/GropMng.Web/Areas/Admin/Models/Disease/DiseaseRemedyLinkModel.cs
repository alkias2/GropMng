using GropMng.Core.Domain.Garden.Enums;
using GropMng.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GropMng.Web.Areas.Admin.Models.Disease;

/// <summary>
/// Represents the create/edit model for a disease remedy link (Disease ↔ Pesticide).
/// </summary>
public class DiseaseRemedyLinkModel
{
    public int Id { get; set; }

    public int DiseaseId { get; set; }

    [GropResourceDisplayName("admin.disease.remedylink.fields.pesticide")]
    public int PesticideId { get; set; }

    public string? PesticideName { get; set; }

    [GropResourceDisplayName("admin.disease.remedylink.fields.treatmenttype")]
    public RemedyTreatmentType TreatmentType { get; set; } = RemedyTreatmentType.Curative;

    [GropResourceDisplayName("admin.disease.remedylink.fields.dosage")]
    public string? Dosage { get; set; }

    [GropResourceDisplayName("admin.disease.remedylink.fields.frequency")]
    public string? Frequency { get; set; }

    [GropResourceDisplayName("admin.disease.remedylink.fields.notes")]
    public string? Notes { get; set; }

    // Dropdown support
    public List<SelectListItem> AvailablePesticides { get; set; } = new();

    public List<SelectListItem> AvailableTreatmentTypes { get; set; } = new();
}
