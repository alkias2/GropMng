using System.ComponentModel.DataAnnotations;
using GropMng.Core.Domain.Garden.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GropMng.Web.Models.Garden;

/// <summary>
/// Form model for creating or editing a repotting log entry.
/// </summary>
public class RepottingModel
{
    public int Id { get; set; }

    public int? NewContainerId { get; set; }

    public int? NewSoilMixId { get; set; }

    [Required]
    public DateOnly RepottedOn { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [MaxLength(500)]
    public string? Notes { get; set; }
}

/// <summary>
/// Read-only row model for repotting history table.
/// </summary>
public class RepottingLogRowModel
{
    public int Id { get; set; }
    public int? PreviousContainerId { get; set; }
    public string PreviousContainerName { get; set; } = "-";
    public int? NewContainerId { get; set; }
    public string NewContainerName { get; set; } = "-";
    public int? PreviousSoilMixId { get; set; }
    public string PreviousSoilMixName { get; set; } = "-";
    public int? NewSoilMixId { get; set; }
    public string NewSoilMixName { get; set; } = "-";
    public DateOnly RepottedOn { get; set; }
    public bool ContainerChanged { get; set; }
    public bool SoilMixChanged { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Summary model for the currently assigned container and soil mix.
/// </summary>
public class ContainerInfoModel
{
    public int? ContainerId { get; set; }
    public string ContainerName { get; set; } = "-";
    public string? Material { get; set; }
    public string? Color { get; set; }
    public decimal? BaseCircumferenceCm { get; set; }
    public decimal? RimCircumferenceCm { get; set; }
    public decimal? HeightCm { get; set; }
    public decimal? LengthCm { get; set; }
    public decimal? WidthCm { get; set; }
    public decimal? VolumeL { get; set; }
    public bool? HasDrainageHole { get; set; }
    public string? Notes { get; set; }
    public DateTime? ContainerCreatedOnUtc { get; set; }
    public DateTime? ContainerUpdatedOnUtc { get; set; }
    public DateTime? CurrentContainerSinceUtc { get; set; }
    public int? SoilMixId { get; set; }
    public string SoilMixName { get; set; } = "-";
    public IReadOnlyList<SoilMixIngredientInfoModel> SoilMixIngredients { get; set; } = Array.Empty<SoilMixIngredientInfoModel>();
}

/// <summary>
/// Read-only ingredient line used in container current soil mix composition view.
/// </summary>
public class SoilMixIngredientInfoModel
{
    public string IngredientName { get; set; } = "-";
    public decimal PercentageByVolume { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Nested modal form model for quick container creation from the repotting tab.
/// </summary>
public class QuickContainerCreateModel
{
    [Required]
    public GardenContainerType ContainerType { get; set; } = GardenContainerType.Pot;

    [MaxLength(100)]
    public string? Material { get; set; }

    [Range(0, 9999)]
    public decimal? BaseCircumferenceCm { get; set; }

    [Range(0, 9999)]
    public decimal? RimCircumferenceCm { get; set; }

    [Range(0, 9999)]
    public decimal? HeightCm { get; set; }

    [Range(0, 9999)]
    public decimal? LengthCm { get; set; }

    [Range(0, 9999)]
    public decimal? WidthCm { get; set; }

    [Range(0, 9999)]
    public decimal? VolumeL { get; set; }

    [MaxLength(50)]
    public string? Color { get; set; }

    public bool HasDrainageHole { get; set; } = true;

    [MaxLength(500)]
    public string? Notes { get; set; }
}

/// <summary>
/// Composite model for container and repotting tab rendering.
/// </summary>
public class RepottingTabModel
{
    public int PlantInstanceId { get; set; }
    public ContainerInfoModel CurrentContainer { get; set; } = new();
    public IReadOnlyList<RepottingLogRowModel> Logs { get; set; } = Array.Empty<RepottingLogRowModel>();
    public IReadOnlyList<SelectListItem> AvailableContainers { get; set; } = Array.Empty<SelectListItem>();
    public IReadOnlyList<SelectListItem> AvailableSoilMixes { get; set; } = Array.Empty<SelectListItem>();
    public IReadOnlyList<SelectListItem> AvailableContainerTypes { get; set; } = Array.Empty<SelectListItem>();
}

/// <summary>
/// Model for repotting modal form.
/// </summary>
public class RepottingFormModel
{
    public int PlantInstanceId { get; set; }
    public IReadOnlyList<SelectListItem> AvailableContainers { get; set; } = Array.Empty<SelectListItem>();
    public IReadOnlyList<SelectListItem> AvailableSoilMixes { get; set; } = Array.Empty<SelectListItem>();
}

/// <summary>
/// Model for quick container create modal.
/// </summary>
public class QuickContainerCreateFormModel
{
    public int PlantInstanceId { get; set; }
    public IReadOnlyList<SelectListItem> AvailableContainerTypes { get; set; } = Array.Empty<SelectListItem>();
}
