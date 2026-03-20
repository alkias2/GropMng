using AutoMapper;
using GropMng.Core.Domain.Configuration;
using GropMng.Core.Interfaces.Services.Configuration;
using GropMng.Web.Areas.Admin.Models.Settings;

namespace GropMng.Web.Factories.Settings;

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
        var settings = await _settingService.LoadAsync<GropAdminAreaSettings>();
        return _mapper.Map<GropAdminAreaSettingsModel>(settings);
    }

    public async Task SaveAdminAreaSettingsAsync(GropAdminAreaSettingsModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        var settings = _mapper.Map<GropAdminAreaSettings>(model);
        await _settingService.SaveAsync(settings);
    }
}
