using GropMng.Core.Configuration;

namespace GropMng.Core.Domain.Media;

public partial class MediaSettings : ISettings
{
    /// <summary>Maximum pixel dimension (width or height) when saving an uploaded image. Default: 2000.</summary>
    public int MaximumImageSize { get; set; } = 2000;

    /// <summary>JPEG compression quality (0-100). Default: 90.</summary>
    public int DefaultImageQuality { get; set; } = 90;

    /// <summary>Thumbnail size for PlantInstance dashboard/action cards. Default: 500.</summary>
    public int PlantInstanceCardThumbSize { get; set; } = 500;

    /// <summary>Thumbnail size for the plant catalog listing. Default: 150.</summary>
    public int PlantCatalogThumbSize { get; set; } = 150;

    /// <summary>Thumbnail size used in the EditorTemplate upload preview widget. Default: 100.</summary>
    public int EditorPreviewSize { get; set; } = 100;

    /// <summary>Reserved for future owner avatar support. Default: 100.</summary>
    public int AvatarPictureSize { get; set; } = 100;

    /// <summary>Relative URL of the default image shown when no picture is available. Default: /images/default-plant.png.</summary>
    public string DefaultPlantImageUrl { get; set; } = "/images/default-plant.png";
}
