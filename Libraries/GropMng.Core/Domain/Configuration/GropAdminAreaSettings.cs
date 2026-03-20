using GropMng.Core.Configuration;

namespace GropMng.Core.Domain.Configuration;

/// <summary>
/// Global Admin UI settings for GropMng.
/// No store-specific overrides are supported in this application.
/// </summary>
public class GropAdminAreaSettings : ISettings
{
    public int DefaultGridPageSize { get; set; } = 10;

    /// <summary>
    /// Comma-separated list of available page sizes for DataTables-like grids.
    /// Example: "10,20,50,100"
    /// </summary>
    public string GridPageSizes { get; set; } = "10,20,50";

    /// <summary>
    /// Rich editor provider selected for Admin forms.
    /// Aligned with Frest template support (Quill).
    /// </summary>
    public string RichEditorProvider { get; set; } = "Quill";

    public bool RichEditorAllowHtml { get; set; } = true;

    /// <summary>
    /// HTML block shown on top of Admin dashboard pages.
    /// Edited via Quill-based RichEditor template.
    /// </summary>
    public string AdminDashboardWelcomeHtml { get; set; } = string.Empty;

    public bool UseIsoDateFormatInJsonResult { get; set; } = true;
}
