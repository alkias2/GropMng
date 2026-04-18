namespace GropMng.Web.Framework.Models.DataTables;

/// <summary>
/// Render strategy for displaying images in a column.
/// </summary>
public class RenderPicture : IGropRender
{
    #region Properties

    /// <inheritdoc/>
    public string RenderType => "picture";

    /// <summary>
    /// Gets or sets the CSS class for the image element.
    /// Example: "img-thumbnail img-sm"
    /// </summary>
    public string ImageClass { get; set; }

    /// <summary>
    /// Gets or sets the maximum width of the image in pixels.
    /// If specified, a style attribute will be added to the image element.
    /// </summary>
    public int? MaxWidth { get; set; }

    /// <summary>
    /// Gets or sets the maximum height of the image in pixels.
    /// If specified, a style attribute will be added to the image element.
    /// </summary>
    public int? MaxHeight { get; set; }

    #endregion
}
