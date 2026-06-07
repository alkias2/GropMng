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

    /// <summary>Circumference of the base opening (cm), as measured from the physical pot.</summary>
    [Range(0, 9999)]
    public decimal? BaseCircumferenceCm { get; set; }

    /// <summary>Circumference of the rim/top opening (cm), as measured from the physical pot.</summary>
    [Range(0, 9999)]
    public decimal? RimCircumferenceCm { get; set; }

    /// <summary>Height of the pot, or depth of the bed (cm).</summary>
    [Range(0, 9999)]
    public decimal? HeightCm { get; set; }

    /// <summary>Length of rectangular bed (cm).</summary>
    [Range(0, 9999)]
    public decimal? LengthCm { get; set; }

    /// <summary>Width of rectangular bed (cm).</summary>
    [Range(0, 9999)]
    public decimal? WidthCm { get; set; }

    [Range(0, 9999)]
    public decimal? VolumeL { get; set; }

    [MaxLength(50)]
    public string? Color { get; set; }

    public bool HasDrainageHole { get; set; } = true;

    public string? Notes { get; set; }

    /// <summary>Display name of the linked plant instance (shown on edit).</summary>
    public string? PlantInstanceNickname { get; set; }

    public IList<SelectListItem> AvailableContainerTypes { get; set; } = new List<SelectListItem>();
}
