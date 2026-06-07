namespace GropMng.Web.Areas.Admin.Models.Localization;

/// <summary>
/// Represents a single row in the LocaleResource DataTables grid.
/// Includes all fields needed for inline editing.
/// </summary>
public class LocaleResourceRowModel
{
    /// <summary>
    /// The unique resource identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The language ID this resource belongs to.
    /// </summary>
    public int LanguageId { get; set; }

    /// <summary>
    /// The unique resource key/name (e.g., "admin.language.create").
    /// </summary>
    public string ResourceName { get; set; } = string.Empty;

    /// <summary>
    /// The localized resource value (editable inline).
    /// </summary>
    public string ResourceValue { get; set; } = string.Empty;
}
