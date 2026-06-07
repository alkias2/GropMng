namespace GropMng.Web.Areas.Admin.Models.Localization;

/// <summary>
/// Represents a single row in the Language DataTables grid.
/// </summary>
public class LanguageRowModel
{
    /// <summary>
    /// The unique language identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The display name of the language (e.g., "English", "Greek").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The language culture code (e.g., "en-US", "el-GR").
    /// </summary>
    public string LanguageCulture { get; set; } = string.Empty;

    /// <summary>
    /// The unique SEO-friendly code for the language (e.g., "en", "el").
    /// </summary>
    public string UniqueSeoCode { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the language is published/active.
    /// </summary>
    public bool Published { get; set; }

    /// <summary>
    /// The display order in lists (lower numbers appear first).
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// The flag image file name if set.
    /// </summary>
    public string? FlagImageFileName { get; set; }

    /// <summary>
    /// Indicates whether the language uses right-to-left text direction.
    /// </summary>
    public bool Rtl { get; set; }
}
