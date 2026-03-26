namespace GropMng.Web.Framework.Models.DataTables;

/// <summary>
/// Represents the metadata and configuration for a single column in a DataTables grid.
/// </summary>
public class GropColumnProperty
{
    #region Ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="GropColumnProperty"/> class.
    /// </summary>
    /// <param name="data">The data property name from the row model.</param>
    public GropColumnProperty(string data)
    {
        Data = data;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the data property name from the row model.
    /// This is the JavaScript property path for reading data from the row object.
    /// </summary>
    public string Data { get; set; }

    /// <summary>
    /// Gets or sets the human-readable title for the column header.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the column width (CSS value, e.g., "100px", "20%").
    /// </summary>
    public string Width { get; set; }

    /// <summary>
    /// Gets or sets the CSS class name(s) applied to all cells in this column.
    /// </summary>
    public string ClassName { get; set; }

    /// <summary>
    /// Gets or sets the inline CSS style applied to all cells in this column.
    /// </summary>
    public string Style { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this column is sortable.
    /// Defaults to true.
    /// </summary>
    public bool Sortable { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether this column is searchable.
    /// Defaults to true.
    /// </summary>
    public bool Searchable { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether this column is visible.
    /// Defaults to true.
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Gets or sets the render strategy for this column.
    /// If not specified, the raw data value is displayed.
    /// </summary>
    public IGropRender Render { get; set; }

    #endregion
}
