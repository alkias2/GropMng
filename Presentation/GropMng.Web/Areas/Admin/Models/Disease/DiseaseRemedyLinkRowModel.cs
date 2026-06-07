namespace GropMng.Web.Areas.Admin.Models.Disease;

/// <summary>
/// Represents a single row in the remedy links sub-grid on the disease edit page.
/// </summary>
public class DiseaseRemedyLinkRowModel
{
    public int Id { get; set; }

    public int DiseaseId { get; set; }

    public string PesticideName { get; set; } = string.Empty;

    public string TreatmentTypeLocalized { get; set; } = string.Empty;

    public string? Dosage { get; set; }

    public string? Frequency { get; set; }
}
