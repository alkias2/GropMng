namespace GropMng.Web.Framework.Models.DataTables;

/// <summary>
/// Render strategy for displaying an "Edit" action button.
/// </summary>
public class RenderButtonEdit : IGropRender
{
    #region Ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="RenderButtonEdit"/> class.
    /// </summary>
    /// <param name="dataUrl">The navigation URL for the edit action.</param>
    public RenderButtonEdit(GropDataUrl dataUrl)
    {
        DataUrl = dataUrl;
    }

    #endregion

    #region Properties

    /// <inheritdoc/>
    public string RenderType => "button-edit";

    /// <summary>
    /// Gets or sets the data URL for the edit action.
    /// </summary>
    public GropDataUrl DataUrl { get; set; }

    #endregion
}
