using System.ComponentModel.DataAnnotations;
using GropMng.Core.Domain.Garden.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GropMng.Web.Models.Garden;

/// <summary>
/// Form model for creating or editing a watering schedule entry.
/// </summary>
public class WateringScheduleModel
{
    public int Id { get; set; }

    [Required]
    public GardenSeason Season { get; set; }

    [Required, Range(1, 365)]
    public byte FrequencyDays { get; set; } = 3;

    [Range(0, 999)]
    public decimal? WaterAmountL { get; set; }

    public GardenTimeOfDay? TimeOfDay { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}

/// <summary>
/// Read-only row model for displaying a watering schedule in the tab list.
/// </summary>
public class WateringScheduleRowModel
{
    public int Id { get; set; }
    public GardenSeason Season { get; set; }
    public string SeasonKey { get; set; } = string.Empty;  // locale key suffix e.g. "spring"
    public string FrequencyLabel { get; set; } = string.Empty;
    public string? WaterAmountLabel { get; set; }
    public GardenTimeOfDay? TimeOfDay { get; set; }
    public string? TimeOfDayKey { get; set; }  // locale key suffix e.g. "morning"
    public string? Notes { get; set; }
}

/// <summary>
/// Read-only row model for displaying a recent watering log entry.
/// </summary>
public class WateringLogRowModel
{
    public int Id { get; set; }
    public DateTime WateredAtUtc { get; set; }
    public string? WaterAmountLabel { get; set; }
    public string? Notes { get; set; }
}
