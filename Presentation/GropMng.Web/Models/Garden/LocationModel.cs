using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GropMng.Web.Models.Garden;

/// <summary>
/// View model for creating or editing an owner Location.
/// </summary>
public class LocationModel
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string City { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Country { get; set; } = "Greece";

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    [MaxLength(100)]
    public string? ClimateZone { get; set; }

    public string? Notes { get; set; }

    /// <summary>Garden spots shown on the edit page.</summary>
    public IList<GardenSpotRowModel> GardenSpots { get; set; } = new List<GardenSpotRowModel>();
}

/// <summary>
/// Lightweight row model for garden spots displayed on a location's edit page.
/// </summary>
public class GardenSpotRowModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? OrientationDisplay { get; set; }
    public string? CoverTypeDisplay { get; set; }
    public byte? SunHoursPerDay { get; set; }
}
