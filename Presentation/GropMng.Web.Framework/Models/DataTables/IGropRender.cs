namespace GropMng.Web.Framework.Models.DataTables;

/// <summary>
/// Defines the contract for DataTables column render strategies.
/// </summary>
public interface IGropRender
{
    /// <summary>
    /// Gets the render type identifier (e.g., "checkbox", "link", "edit", "custom").
    /// </summary>
    string RenderType { get; }
}
