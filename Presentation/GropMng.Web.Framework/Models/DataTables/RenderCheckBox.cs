namespace GropMng.Web.Framework.Models.DataTables;

/// <summary>
/// Render strategy for displaying a checkbox (typically for row selection).
/// </summary>
public class RenderCheckBox : IGropRender
{
    #region Properties

    /// <inheritdoc/>
    public string RenderType => "checkbox";

    #endregion
}
