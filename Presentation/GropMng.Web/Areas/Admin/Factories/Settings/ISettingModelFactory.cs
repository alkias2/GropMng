using GropMng.Web.Areas.Admin.Models.Settings;

namespace GropMng.Web.Areas.Admin.Factories.Settings;

/// <summary>
/// Prepares and persists Admin settings view models.
/// </summary>
public interface ISettingModelFactory
{
    Task<GropAdminAreaSettingsModel> PrepareAdminAreaSettingsModelAsync();

    Task SaveAdminAreaSettingsAsync(GropAdminAreaSettingsModel model);
}
