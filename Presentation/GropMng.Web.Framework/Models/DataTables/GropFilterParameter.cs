namespace GropMng.Web.Framework.Models.DataTables;

/// <summary>
/// Represents a search/filter parameter configuration for a DataTables grid.
/// </summary>
public class GropFilterParameter
{
    #region Ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="GropFilterParameter"/> class.
    /// </summary>
    /// <param name="name">The filter parameter name (typically matches the search model property name).</param>
    public GropFilterParameter(string name)
    {
        Name = name;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the filter parameter name.
    /// This name is used to bind the filter value from the view to the search model.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets a human-readable label for the filter (used in UI).
    /// If not set, the Name is used as the label.
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    /// Gets or sets the HTML element ID to bind the filter control.
    /// </summary>
    public string ElementId { get; set; }

    #endregion
}
