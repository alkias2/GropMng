using System.ComponentModel.DataAnnotations;
using GropMng.Core.Domain.Garden.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GropMng.Web.Models.Garden;

/// <summary>
/// Form model for creating or editing a fertilizing schedule entry.
/// </summary>
public class FertilizingScheduleModel
{
    public int Id { get; set; }

    [Required]
    public int FertilizerId { get; set; }

    [Required]
    public GardenSeason Season { get; set; }

    [Required, Range(1, 365)]
    public byte FrequencyDays { get; set; } = 14;

    [Range(0, 9999)]
    public decimal? Quantity { get; set; }

    public FertilizerQuantityUnit? Unit { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    [MaxLength(500)]
    public string? DilutionInstructions { get; set; }
}

/// <summary>
/// Read-only row model for displaying a fertilizing schedule in the tab list.
/// </summary>
public class FertilizingScheduleRowModel
{
    public int Id { get; set; }
    public int FertilizerId { get; set; }
    public string FertilizerName { get; set; } = string.Empty;
    public GardenSeason Season { get; set; }
    public string FrequencyLabel { get; set; } = string.Empty;
    public decimal? Quantity { get; set; }
    public FertilizerQuantityUnit? Unit { get; set; }
    public string? Notes { get; set; }
    public string? DilutionInstructions { get; set; }
}

/// <summary>
/// Read-only row model for displaying a recent fertilizing log entry.
/// </summary>
public class FertilizingLogRowModel
{
    public int Id { get; set; }
    public DateTime AppliedAtUtc { get; set; }
    public int FertilizerId { get; set; }
    public string FertilizerName { get; set; } = string.Empty;
    public decimal? Quantity { get; set; }
    public FertilizerQuantityUnit? Unit { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Composite model for fertilizing tab rendering.
/// </summary>
public class FertilizingScheduleTabModel
{
    public int PlantInstanceId { get; set; }
    public IReadOnlyList<FertilizingScheduleRowModel> Schedules { get; set; } = Array.Empty<FertilizingScheduleRowModel>();
    public IReadOnlyList<FertilizingLogRowModel> LogRows { get; set; } = Array.Empty<FertilizingLogRowModel>();
    public IReadOnlyList<SelectListItem> AvailableFertilizers { get; set; } = Array.Empty<SelectListItem>();
}

/// <summary>
/// Model for fertilizing schedule modal form.
/// </summary>
public class FertilizingScheduleFormModel
{
    public int PlantInstanceId { get; set; }
    public IReadOnlyList<SelectListItem> AvailableFertilizers { get; set; } = Array.Empty<SelectListItem>();
}
