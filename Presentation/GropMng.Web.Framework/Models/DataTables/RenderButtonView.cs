namespace GropMng.Web.Framework.Models.DataTables;

/// <summary>
/// Render strategy for displaying a "View" action button.
/// </summary>
public class RenderButtonView : IGropRender
{
    #region Ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="RenderButtonView"/> class.
    /// </summary>
    /// <param name="dataUrl">The navigation URL for the view action.</param>
    public RenderButtonView(GropDataUrl dataUrl)
    {
        DataUrl = dataUrl;
    }

    #endregion

    #region Properties

    /// <inheritdoc/>
    public string RenderType => "button-view";

    /// <summary>
    /// Gets or sets the data URL for the view action.
    /// </summary>
    public GropDataUrl DataUrl { get; set; }

    #endregion
}
