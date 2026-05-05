using System.ComponentModel.DataAnnotations;

namespace GropMng.Web.Models.Garden;

/// <summary>
/// View model used when adding or updating a plant instance photo.
/// </summary>
public class PlantInstancePhotoModel
{
    public int Id { get; set; }

    public int PlantInstanceId { get; set; }

    [Required]
    public int PictureId { get; set; }

    [MaxLength(200)]
    public string? Caption { get; set; }

    public DateOnly TakenDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Range(0, 9999)]
    public int DisplayOrder { get; set; }
}

/// <summary>
/// Lightweight row model returned by the PlantPhotoList JSON endpoint for DataTable rendering.
/// </summary>
public class PlantInstancePhotoRowModel
{
    public int Id { get; set; }
    public int PictureId { get; set; }
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public string TakenDate { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
