using System.ComponentModel.DataAnnotations;
using GropMng.Core.Domain.Garden.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GropMng.Web.Models.Garden;

/// <summary>
/// View model for creating or editing a GardenSpot under a Location.
/// </summary>
public class GardenSpotModel
{
    public int Id { get; set; }

    public int LocationId { get; set; }

    /// <summary>Parent location name, shown in breadcrumbs.</summary>
    public string LocationName { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public GardenOrientation? Orientation { get; set; }

    public GardenCoverType? CoverType { get; set; }

    [Range(0, 24)]
    public byte? SunHoursPerDay { get; set; }

    [MaxLength(500)]
    public string? Surroundings { get; set; }

    public string? Notes { get; set; }

    public IList<SelectListItem> AvailableOrientations { get; set; } = new List<SelectListItem>();
    public IList<SelectListItem> AvailableCoverTypes { get; set; } = new List<SelectListItem>();
}
