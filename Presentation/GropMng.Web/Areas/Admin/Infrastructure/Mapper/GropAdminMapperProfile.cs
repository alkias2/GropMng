using AutoMapper;
using GropMng.Core.Domain.Configuration;
using GropMng.Core.Domain.Garden.Plants;
using GropMng.Core.Domain.Logging;
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
        CreateMap<AppLog, AppLogRowModel>();

        CreateMap<Plant, PlantRowModel>()
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.ToString()));

        CreateMap<Plant, PlantModel>();
        CreateMap<PlantModel, Plant>()
            .ForMember(dest => dest.PlantInstances, opt => opt.Ignore());

        CreateMap<GropAdminAreaSettings, GropAdminAreaSettingsModel>();
        CreateMap<GropAdminAreaSettingsModel, GropAdminAreaSettings>();
    }
}
