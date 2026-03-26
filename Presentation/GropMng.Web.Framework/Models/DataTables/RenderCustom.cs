namespace GropMng.Web.Framework.Models.DataTables;

/// <summary>
/// Render strategy for delegating to a custom JavaScript render function.
/// Used when a column requires custom rendering logic not covered by built-in renderers.
/// </summary>
public class RenderCustom : IGropRender
{
    #region Ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="RenderCustom"/> class.
    /// </summary>
    /// <param name="functionName">The name of the JavaScript function to call for rendering.</param>
    public RenderCustom(string functionName)
    {
        FunctionName = functionName;
    }

    #endregion

    #region Properties

    /// <inheritdoc/>
    public string RenderType => "custom";

    /// <summary>
    /// Gets or sets the name of the JavaScript function to call for rendering.
    /// The function signature should match DataTables render callbacks: function(data, type, row, meta).
    /// </summary>
    public string FunctionName { get; set; }

    #endregion
}
