using AutoMapper;
using GropMng.Core.Domain.Configuration;
using GropMng.Core.Domain.Garden.Plants;
using GropMng.Core.Domain.Localization;
using GropMng.Core.Domain.Logging;
using GropMng.Web.Areas.Admin.Models.Localization;
using GropMng.Web.Areas.Admin.Models.Logging;
using GropMng.Web.Areas.Admin.Models.Plant;
using GropMng.Web.Areas.Admin.Models.Settings;

namespace GropMng.Web.Areas.Admin.Infrastructure.Mapper;

/// <summary>
/// AutoMapper profile for GropMng Admin area models.
/// </summary>
public class GropAdminMapperProfile : Profile
{
    public GropAdminMapperProfile()
    {
        CreateMap<AppLog, AppLogRowModel>()
            .ForMember(dest => dest.LevelLocalized, opt => opt.Ignore())
            .ForMember(dest => dest.TimestampLocalized, opt => opt.Ignore());

        CreateMap<Language, LanguageRowModel>();

        CreateMap<LocaleStringResource, LocaleResourceRowModel>();

        CreateMap<Plant, PlantRowModel>()
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.ToString()))
            .ForMember(dest => dest.PictureThumbnailUrl, opt => opt.Ignore());

        CreateMap<Plant, PlantModel>();
        CreateMap<PlantModel, Plant>()
            .ForMember(dest => dest.PlantInstances, opt => opt.Ignore());

        CreateMap<GropAdminAreaSettings, GropAdminAreaSettingsModel>();
        CreateMap<GropAdminAreaSettingsModel, GropAdminAreaSettings>();
    }
}
