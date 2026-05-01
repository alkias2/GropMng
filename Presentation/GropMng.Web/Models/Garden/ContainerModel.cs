using System.ComponentModel.DataAnnotations;
using GropMng.Core.Domain.Garden.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GropMng.Web.Models.Garden;

/// <summary>
/// View model for creating or editing an owner Container.
/// </summary>
public class ContainerModel
{
    public int Id { get; set; }

    public GardenContainerType ContainerType { get; set; } = GardenContainerType.Pot;

    [MaxLength(100)]
    public string? Material { get; set; }

    [Range(0, 9999)]
    public decimal? LengthCm { get; set; }

    [Range(0, 9999)]
    public decimal? WidthCm { get; set; }

    [Range(0, 9999)]
    public decimal? DepthCm { get; set; }

    [Range(0, 9999)]
    public decimal? DiameterCm { get; set; }

    [Range(0, 9999)]
    public decimal? VolumeL { get; set; }

    [MaxLength(50)]
    public string? Color { get; set; }

    public bool HasDrainageHole { get; set; } = true;

    public string? Notes { get; set; }

    /// <summary>How many plant instances currently use this container (shown on edit/delete).</summary>
    public int PlantInstanceCount { get; set; }

    public IList<SelectListItem> AvailableContainerTypes { get; set; } = new List<SelectListItem>();
}
