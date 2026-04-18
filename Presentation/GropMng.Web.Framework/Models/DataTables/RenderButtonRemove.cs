namespace GropMng.Web.Framework.Models.DataTables;

/// <summary>
/// Render strategy for displaying a "Delete" or "Remove" action button.
/// </summary>
public class RenderButtonRemove : IGropRender
{
    #region Ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="RenderButtonRemove"/> class.
    /// </summary>
    /// <param name="dataUrl">The delete action endpoint.</param>
    public RenderButtonRemove(GropDataUrl dataUrl)
    {
        DataUrl = dataUrl;
    }

    #endregion

    #region Properties

    /// <inheritdoc/>
    public string RenderType => "button-remove";

    /// <summary>
    /// Gets or sets the delete action endpoint.
    /// </summary>
    public GropDataUrl DataUrl { get; set; }

    #endregion
}
