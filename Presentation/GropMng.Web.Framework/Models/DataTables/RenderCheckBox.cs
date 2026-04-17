namespace GropMng.Web.Framework.Models.DataTables;

/// <summary>
/// Render strategy for displaying a row-selection checkbox in a DataTables column.
/// Mirrors the NopCommerce <c>RenderCheckBox</c> class.
/// </summary>
public class RenderCheckBox : IGropRender
{
    #region Ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="RenderCheckBox"/> class.
    /// </summary>
    /// <param name="name">
    /// The CSS class applied to each generated checkbox input.
    /// Used to identify all checkboxes in the group for select-all logic.
    /// Default: <c>"dt-checkbox"</c>.
    /// </param>
    /// <param name="propertyKeyName">
    /// The row data field name used as the checkbox <c>value</c> attribute.
    /// Typically the entity primary key field.
    /// Default: <c>"id"</c>.
    /// </param>
    public RenderCheckBox(string name = "dt-checkbox", string propertyKeyName = "id")
    {
        Name = name;
        PropertyKeyName = propertyKeyName;
    }

    #endregion

    #region Properties

    /// <inheritdoc/>
    public string RenderType => "checkbox";

    /// <summary>
    /// Gets or sets the CSS class applied to each generated checkbox input.
    /// This class is used to identify all checkboxes in the group
    /// and to drive the select-all header checkbox logic.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the row data field name used as the checkbox <c>value</c> attribute.
    /// Typically the entity primary key (e.g., <c>"id"</c>).
    /// </summary>
    public string PropertyKeyName { get; set; }

    #endregion
}
