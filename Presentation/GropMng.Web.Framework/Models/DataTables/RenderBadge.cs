namespace GropMng.Web.Framework.Models.DataTables;

/// <summary>
/// Render strategy for displaying values as Bootstrap badges with optional custom styling per value.
/// </summary>
public class RenderBadge : IGropRender
{
    #region Ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="RenderBadge"/> class.
    /// </summary>
    /// <param name="defaultClass">The default CSS class for the badge (e.g., "badge bg-primary").</param>
    public RenderBadge(string defaultClass = "badge bg-primary")
    {
        DefaultClass = defaultClass;
    }

    #endregion

    #region Properties

    /// <inheritdoc/>
    public string RenderType => "badge";

    /// <summary>
    /// Gets or sets the default CSS class for the badge.
    /// Example: "badge bg-primary"
    /// </summary>
    public string DefaultClass { get; set; }

    /// <summary>
    /// Gets or sets an optional dictionary mapping values to their custom CSS classes.
    /// If a value key exists in this dictionary, its class is used instead of DefaultClass.
    /// </summary>
    public Dictionary<string, string> ClassMap { get; set; }

    #endregion
}
