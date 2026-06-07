using System.ComponentModel.DataAnnotations;
using GropMng.Core.Domain.Garden.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GropMng.Web.Models.Garden;

/// <summary>
/// Form model for creating or editing a disease record.
/// </summary>
public class DiseaseRecordModel
{
    public int Id { get; set; }

    [Required]
    public int DiseaseId { get; set; }

    [Required]
    public DateOnly DetectedDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public DateOnly? ResolvedDate { get; set; }

    public PlantDiseaseSeverity? Severity { get; set; } = PlantDiseaseSeverity.Moderate;

    [MaxLength(500)]
    public string? TreatmentUsed { get; set; }

    public PlantDiseaseOutcome? Outcome { get; set; } = PlantDiseaseOutcome.Ongoing;

    [MaxLength(1000)]
    public string? Notes { get; set; }
}

/// <summary>
/// Read-only row model for disease record listing.
/// </summary>
public class DiseaseRecordRowModel
{
    public int Id { get; set; }
    public int DiseaseId { get; set; }
    public string DiseaseName { get; set; } = string.Empty;
    public DateOnly DetectedDate { get; set; }
    public DateOnly? ResolvedDate { get; set; }
    public PlantDiseaseSeverity? Severity { get; set; }
    public PlantDiseaseOutcome? Outcome { get; set; }
    public string? TreatmentUsed { get; set; }
    public string? Notes { get; set; }
    public int PhotoCount { get; set; }
}

/// <summary>
/// Form model for adding a photo under a disease record.
/// </summary>
public class DiseasePhotoModel
{
    public int Id { get; set; }

    [Required]
    public int PictureId { get; set; }

    public DateOnly TakenDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [MaxLength(500)]
    public string? Notes { get; set; }

    [Range(0, 9999)]
    public int DisplayOrder { get; set; }
}

/// <summary>
/// Read-only row model for disease photo listing.
/// </summary>
public class DiseasePhotoRowModel
{
    public int Id { get; set; }
    public int PictureId { get; set; }
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateOnly TakenDate { get; set; }
    public int DisplayOrder { get; set; }
}

/// <summary>
/// Composite model for disease tab rendering.
/// </summary>
public class DiseaseRecordTabModel
{
    public int PlantInstanceId { get; set; }
    public IReadOnlyList<DiseaseRecordRowModel> Records { get; set; } = Array.Empty<DiseaseRecordRowModel>();
    public IReadOnlyDictionary<int, IReadOnlyList<DiseasePhotoRowModel>> PhotosByRecordId { get; set; } =
        new Dictionary<int, IReadOnlyList<DiseasePhotoRowModel>>();
    public IReadOnlyList<SelectListItem> AvailableDiseases { get; set; } = Array.Empty<SelectListItem>();
}

/// <summary>
/// Model for disease record modal form.
/// </summary>
public class DiseaseRecordFormModel
{
    public int PlantInstanceId { get; set; }
    public IReadOnlyList<SelectListItem> AvailableDiseases { get; set; } = Array.Empty<SelectListItem>();
}

/// <summary>
/// Model for rendering the photo management section of a disease record.
/// </summary>
public class DiseasePhotoListSectionModel
{
    public int PlantInstanceId { get; set; }
    public int RecordId { get; set; }
    public IReadOnlyList<DiseasePhotoRowModel> Photos { get; set; } = Array.Empty<DiseasePhotoRowModel>();
}
