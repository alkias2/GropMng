using System.ComponentModel.DataAnnotations;
using GropMng.Core.Domain.Garden.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GropMng.Web.Models.Garden;

/// <summary>
/// View model for creating or editing a plant instance.
/// </summary>
public class PlantInstanceModel
{
    public int Id { get; set; }

    [Required]
    public int PlantId { get; set; }

    public string PlantName { get; set; } = string.Empty;

    [Required]
    public int GardenSpotId { get; set; }

    public string GardenSpotName { get; set; } = string.Empty;

    public string LocationName { get; set; } = string.Empty;

    public int? ContainerId { get; set; }

    public string? ContainerInfo { get; set; }

    public int? SoilMixId { get; set; }

    public string? SoilMixName { get; set; }

    [MaxLength(100)]
    public string? Nickname { get; set; }

    public DateOnly? PlantedDate { get; set; }

    public int? AgeYears { get; set; }

    public PlantSizeCategory? SizeCategory { get; set; }

    [Range(0, 1000)]
    public decimal? HeightCm { get; set; }

    [Range(0, 1000)]
    public decimal? SpreadCm { get; set; }

    public PlantHealthStatus HealthStatus { get; set; } = PlantHealthStatus.Good;

    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string? Notes { get; set; }

    public IList<SelectListItem> AvailablePlants { get; set; } = new List<SelectListItem>();
    public IList<SelectListItem> AvailableGardenSpots { get; set; } = new List<SelectListItem>();
    public IList<SelectListItem> AvailableContainers { get; set; } = new List<SelectListItem>();
    public IList<SelectListItem> AvailableSoilMixes { get; set; } = new List<SelectListItem>();
    public IList<SelectListItem> AvailableSizeCategories { get; set; } = new List<SelectListItem>();
    public IList<SelectListItem> AvailableHealthStatuses { get; set; } = new List<SelectListItem>();
}

/// <summary>
/// Lightweight list model for plant instance row display with filtering.
/// </summary>
public class PlantInstanceListRowModel
{
    public int Id { get; set; }
    public string PlantName { get; set; } = string.Empty;
    public string? Nickname { get; set; }
    public string GardenSpotName { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public string? ContainerInfo { get; set; }
    public PlantHealthStatus HealthStatus { get; set; }
    public int? AgeYears { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// List filter model for plant instance search.
/// </summary>
public class PlantInstanceListFilterModel
{
    public int? PlantId { get; set; }
    public int? GardenSpotId { get; set; }
    public int? LocationId { get; set; }
    public bool ActiveOnly { get; set; } = true;

    public IList<SelectListItem> AvailablePlants { get; set; } = new List<SelectListItem>();
    public IList<SelectListItem> AvailableGardenSpots { get; set; } = new List<SelectListItem>();
    public IList<SelectListItem> AvailableLocations { get; set; } = new List<SelectListItem>();
}
