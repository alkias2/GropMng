using AutoMapper;
using GropMng.Core.Domain.Configuration;
using GropMng.Core.Domain.Logging;
using GropMng.Web.Areas.Admin.Models.Logging;
using GropMng.Web.Areas.Admin.Models.Settings;

namespace GropMng.Web.Areas.Admin.Infrastructure.Mapper;

/// <summary>
/// AutoMapper profile for GropMng Admin area models.
/// </summary>
public class GropAdminMapperProfile : Profile
{
    public GropAdminMapperProfile()
    {
        CreateMap<AppLog, AppLogRowModel>();

        CreateMap<GropAdminAreaSettings, GropAdminAreaSettingsModel>();
        CreateMap<GropAdminAreaSettingsModel, GropAdminAreaSettings>();
    }
}
