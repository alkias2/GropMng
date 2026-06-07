using AutoMapper;
using GropMng.Core.Domain.Configuration;
using GropMng.Core.Interfaces.Services.Configuration;
using GropMng.Web.Areas.Admin.Models.Settings;

namespace GropMng.Web.Areas.Admin.Factories.Settings;

/// <summary>
/// Default factory for Admin settings pages.
/// Keeps controller actions thin by centralizing settings model preparation and persistence.
/// </summary>
public class SettingModelFactory : ISettingModelFactory
{
    private readonly ISettingService _settingService;
    private readonly IMapper _mapper;

    public SettingModelFactory(ISettingService settingService, IMapper mapper)
    {
        _settingService = settingService;
        _mapper = mapper;
    }

    public async Task<GropAdminAreaSettingsModel> PrepareAdminAreaSettingsModelAsync()
    {
        var adminSettings = await _settingService.LoadAsync<GropAdminAreaSettings>();
        var registrationSettings = await _settingService.LoadAsync<GropOwnerRegistrationSettings>();

        var model = _mapper.Map<GropAdminAreaSettingsModel>(adminSettings);
        model.RequireEmailConfirmation = registrationSettings.RequireEmailConfirmation;
        model.PasswordResetTokenExpirationHours = registrationSettings.PasswordResetTokenExpirationHours;

        return model;
    }

    public async Task SaveAdminAreaSettingsAsync(GropAdminAreaSettingsModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var adminSettings = _mapper.Map<GropAdminAreaSettings>(model);
        await _settingService.SaveAsync(adminSettings);

        var registrationSettings = await _settingService.LoadAsync<GropOwnerRegistrationSettings>();
        registrationSettings.RequireEmailConfirmation = model.RequireEmailConfirmation;
        registrationSettings.PasswordResetTokenExpirationHours = model.PasswordResetTokenExpirationHours;

        await _settingService.SaveAsync(registrationSettings);
    }
}
