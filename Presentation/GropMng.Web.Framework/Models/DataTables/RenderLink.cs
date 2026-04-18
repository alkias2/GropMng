namespace GropMng.Web.Framework.Models.DataTables;

/// <summary>
/// Render strategy for displaying a hyperlink.
/// </summary>
public class RenderLink : IGropRender
{
    #region Ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="RenderLink"/> class.
    /// </summary>
    /// <param name="routeName">The route name or action/controller path.</param>
    public RenderLink(string routeName)
    {
        RouteName = routeName;
    }

    #endregion

    #region Properties

    /// <inheritdoc/>
    public string RenderType => "link";

    /// <summary>
    /// Gets or sets the route name or action/controller path for the link.
    /// </summary>
    public string RouteName { get; set; }

    #endregion
}
