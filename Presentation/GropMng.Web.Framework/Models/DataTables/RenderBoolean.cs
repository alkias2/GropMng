namespace GropMng.Web.Framework.Models.DataTables;

/// <summary>
/// Render strategy for displaying boolean values as visual indicators (badges, icons, or text).
/// </summary>
public class RenderBoolean : IGropRender
{
    #region Properties

    /// <inheritdoc/>
    public string RenderType => "boolean";

    /// <summary>
    /// Gets or sets the text or HTML to display for true values.
    /// Default: "Yes"
    /// </summary>
    public string TrueValue { get; set; } = "Yes";

    /// <summary>
    /// Gets or sets the text or HTML to display for false values.
    /// Default: "No"
    /// </summary>
    public string FalseValue { get; set; } = "No";

    /// <summary>
    /// Gets or sets the CSS class for true value styling.
    /// Example: "badge bg-success"
    /// </summary>
    public string TrueClass { get; set; }

    /// <summary>
    /// Gets or sets the CSS class for false value styling.
    /// Example: "badge bg-danger"
    /// </summary>
    public string FalseClass { get; set; }

    #endregion
}
