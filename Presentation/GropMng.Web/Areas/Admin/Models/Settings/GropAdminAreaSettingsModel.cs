using System.ComponentModel.DataAnnotations;

namespace GropMng.Web.Areas.Admin.Models.Settings;

/// <summary>
/// Editable Admin settings model persisted to the Setting table through ISettingService.
/// </summary>
public class GropAdminAreaSettingsModel
{
    public int DefaultGridPageSize { get; set; } = 10;

    public string GridPageSizes { get; set; } = "10,20,50";

    public string RichEditorProvider { get; set; } = "Quill";

    public bool RichEditorAllowHtml { get; set; } = true;

    public string AdminDashboardWelcomeHtml { get; set; } = string.Empty;

    [Display(Name = "Require email confirmation")]
    public bool RequireEmailConfirmation { get; set; } = false;

    [Display(Name = "Password reset expiration in hours")]
    public int PasswordResetTokenExpirationHours { get; set; } = 24;

    public bool UseIsoDateFormatInJsonResult { get; set; } = true;
}
