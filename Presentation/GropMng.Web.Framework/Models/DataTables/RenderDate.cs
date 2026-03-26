namespace GropMng.Web.Framework.Models.DataTables;

/// <summary>
/// Render strategy for displaying date values with optional formatting.
/// </summary>
public class RenderDate : IGropRender
{
    #region Properties

    /// <inheritdoc/>
    public string RenderType => "date";

    /// <summary>
    /// Gets or sets the date format string (e.g., "yyyy-MM-dd", "dd/MM/yyyy HH:mm").
    /// If not specified, the default server culture format is used.
    /// </summary>
    public string Format { get; set; }

    #endregion
}
