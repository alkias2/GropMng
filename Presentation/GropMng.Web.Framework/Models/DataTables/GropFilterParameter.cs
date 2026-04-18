namespace GropMng.Web.Framework.Models.DataTables;

/// <summary>
/// Represents a search/filter parameter configuration for a DataTables grid.
/// Mirrors the NopCommerce <c>FilterParameter</c> pattern with GropMng extensions.
/// </summary>
public class GropFilterParameter
{
    #region Ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="GropFilterParameter"/> class with a server-side parameter name.
    /// </summary>
    /// <param name="name">The server-side parameter name (matches the search model property name).</param>
    public GropFilterParameter(string name)
    {
        Name = name;
        Type = typeof(string);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GropFilterParameter"/> class
    /// with a name and an explicit model/element name.
    /// </summary>
    /// <param name="name">The server-side parameter name.</param>
    /// <param name="modelName">The HTML element ID to read the filter value from.</param>
    public GropFilterParameter(string name, string modelName)
    {
        Name = name;
        ModelName = modelName;
        Type = typeof(string);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GropFilterParameter"/> class
    /// with a name and an explicit .NET type.
    /// </summary>
    /// <param name="name">The server-side parameter name.</param>
    /// <param name="type">The .NET type of the filter value.</param>
    public GropFilterParameter(string name, Type type)
    {
        Name = name;
        Type = type;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GropFilterParameter"/> class
    /// with a static fixed value sent with every request.
    /// </summary>
    /// <param name="name">The server-side parameter name.</param>
    /// <param name="value">The static value to send with every AJAX request.</param>
    public GropFilterParameter(string name, object value)
    {
        Name = name;
        Type = value.GetType();
        Value = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GropFilterParameter"/> class
    /// for parent-child table linking.
    /// </summary>
    /// <param name="name">The server-side parameter name.</param>
    /// <param name="parentName">The parent table column name whose value is forwarded.</param>
    /// <param name="isParentChildParameter">Must be <c>true</c> to distinguish from the (name, modelName) overload.</param>
    public GropFilterParameter(string name, string parentName, bool isParentChildParameter)
    {
        Name = name;
        ParentName = parentName;
        Type = typeof(string);
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets the server-side parameter name.
    /// This name is used as the AJAX POST field name sent to the controller action.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets or sets the HTML element ID to read the filter value from.
    /// Acts as a fallback when <see cref="ElementId"/> is not explicitly set.
    /// </summary>
    public string ModelName { get; set; }

    /// <summary>
    /// Gets the .NET type of the filter value (used for JS value coercion).
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// Gets or sets a static value to send with every AJAX request.
    /// Used for fixed parameters and parent-child table linking.
    /// </summary>
    public object Value { get; set; }

    /// <summary>
    /// Gets or sets the parent table column name whose value is forwarded to child table queries.
    /// </summary>
    public string ParentName { get; set; }

    /// <summary>
    /// Gets or sets an explicit HTML element ID to read the filter value from.
    /// When set, takes priority over <see cref="ModelName"/> and <see cref="Name"/>.
    /// This is a GropMng extension for cases where the element ID differs from the property name.
    /// </summary>
    public string ElementId { get; set; }

    /// <summary>
    /// Gets or sets a human-readable label for the filter (used in UI generation).
    /// If not set, <see cref="Name"/> is used as the label.
    /// This is a GropMng extension.
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    /// Gets the resolved DOM element ID for filter value binding.
    /// Priority order: <see cref="ElementId"/> → <see cref="ModelName"/> → <see cref="Name"/>.
    /// </summary>
    public string ResolvedElementId => ElementId ?? ModelName ?? Name;

    #endregion
}
