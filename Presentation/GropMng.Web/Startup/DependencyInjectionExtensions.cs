using FluentValidation;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.Configuration;
using GropMng.Core.Interfaces.Services.Garden.AI;
using GropMng.Core.Interfaces.Services.Garden.Care;
using GropMng.Core.Interfaces.Services.Garden.Health;
using GropMng.Core.Interfaces.Services.Garden.Locations;
using GropMng.Core.Interfaces.Services.Garden.Plants;
using GropMng.Core.Interfaces.Services.Garden.Preferences;
using GropMng.Core.Interfaces.Services.Localization;
using GropMng.Core.Interfaces.Services.Logging;
using GropMng.Core.Interfaces.Services.User;
using GropMng.Data.DbContext;
using GropMng.Data.Repositories;
using GropMng.Services.Services.Configuration;
using GropMng.Services.Services.Garden.AI;
using GropMng.Services.Services.Garden.Care;
using GropMng.Services.Services.Garden.Health;
using GropMng.Services.Services.Garden.Locations;
using GropMng.Services.Services.Garden.Plants;
using GropMng.Services.Services.Garden.Preferences;
using GropMng.Services.Services.Localization;
using GropMng.Services.Services.Logging;
using GropMng.Services.Services.User;
using GropMng.Web.Initialization;
using GropMng.Web.Initialization.Options;
using GropMng.Web.Initialization.Seeders;
using GropMng.Web.Framework.UI;
using GropMng.Web.Areas.Admin.Validators.Logging;
using GropMng.Web.Areas.Admin.Factories.Localization;
using GropMng.Web.Areas.Admin.Factories.Logging;
using GropMng.Web.Areas.Admin.Factories.Plant;
using GropMng.Web.Areas.Admin.Factories.Settings;
using GropMng.Web.Areas.Admin.Factories.User;
using GropMng.Web.Infrastructure.Navigation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace GropMng.Web.Startup;

/// <summary>
/// Provides startup extension methods that register application services for the web layer.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registers the web application's framework, persistence, domain, localization, and factory services.
    /// </summary>
    /// <param name="services">The service collection being configured.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddCoreFrameworkServices();
        services.Configure<OwnerBootstrapOptions>(configuration.GetSection(OwnerBootstrapOptions.SectionName));
        services.AddRequestLocalizationOptions();
        services.AddDataAccessAndDomainServices(configuration);

        return services;
    }

    /// <summary>
    /// Registers core ASP.NET Core and web-framework services used by the application.
    /// </summary>
    /// <param name="services">The service collection being configured.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddCoreFrameworkServices(this IServiceCollection services)
    {
        services.AddControllersWithViews();
        services.AddMemoryCache();
        services.AddHttpContextAccessor();
        services.AddDataProtection();
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = "GropMng.OwnerAuth";
                options.LoginPath = "/owner/auth/login";
                options.AccessDeniedPath = "/Common/Error";
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
            });
        services.AddAuthorization();
        services.AddAutoMapper(cfg => cfg.AddMaps(typeof(Program).Assembly));
        services.AddValidatorsFromAssemblyContaining<AppLogSearchModelValidator>();
        services.AddScoped<IGropHtmlHelper, GropHtmlHelper>();
        services.AddScoped<IAppMenuSiteMap, XmlAppMenuSiteMap>();
        services.AddScoped<IAppMenuProvider, DefaultAppMenuProvider>();

        return services;
    }

    /// <summary>
    /// Configures request localization using the currently supported application cultures.
    /// </summary>
    /// <param name="services">The service collection being configured.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddRequestLocalizationOptions(this IServiceCollection services)
    {
        var supportedCultures = new[]
        {
            new CultureInfo("el-GR"),
            new CultureInfo("en-US")
        };

        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture = new RequestCulture("el-GR");
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
            options.RequestCultureProviders = new IRequestCultureProvider[]
            {
                new CookieRequestCultureProvider { CookieName = CookieRequestCultureProvider.DefaultCookieName },
                new AcceptLanguageHeaderRequestCultureProvider()
            };
        });

        return services;
    }

    /// <summary>
    /// Registers persistence, business services, localization services, and web factories when SQL Server connectivity is available.
    /// </summary>
    /// <param name="services">The service collection being configured.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddDataAccessAndDomainServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var sqlServerSettings = GropContextConfiguration.ResolveSqlServerSettings(configuration);
        if (!sqlServerSettings.CanConnect)
            return services;

        services.AddDbContext<GropContext>(options =>
            options.UseSqlServer(sqlServerSettings.ConnectionString));

        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<ISqlQueryExecutor, SqlQueryExecutor>();

        services.AddScoped<IAppLogService, AppLogService>();

        services.AddScoped<ILocationService, LocationService>();
        services.AddScoped<IPlantService, PlantService>();
        services.AddScoped<IPlantInstanceService, PlantInstanceService>();
        services.AddScoped<IDiseaseService, DiseaseService>();
        services.AddScoped<IFertilizerService, FertilizerService>();
        services.AddScoped<IPesticideService, PesticideService>();

        services.AddScoped<IAIQueryTemplateService, AIQueryTemplateService>();

        services.AddScoped<GropMng.Core.Interfaces.Services.Garden.Preferences.IUserPreferenceService, UserPreferenceService>();
        services.AddScoped<ICurrentOwnerProvider, CurrentOwnerProvider>();
        services.AddScoped<IOwnerService, OwnerService>();
        services.AddScoped<IOwnerPasswordService, OwnerPasswordService>();
        services.AddScoped<IOwnerAuthenticationService, OwnerAuthenticationService>();
        services.AddScoped<IOwnerAccountFlowService, OwnerAccountFlowService>();
        services.AddScoped<IPermissionService, PermissionService>();

        services.AddScoped<ILanguageService, LanguageService>();
        services.AddScoped<ICurrentLanguageContext, CurrentLanguageContext>();
        services.AddScoped<ILocalizationService, LocalizationService>();
        services.AddScoped<IEnumLocalizationHelper, EnumLocalizationHelper>();

        services.AddScoped<ISettingService, SettingService>();

        services.AddScoped<IAppLogModelFactory, AppLogModelFactory>();
        services.AddScoped<ILocalizationModelFactory, LocalizationModelFactory>();
        services.AddScoped<IPlantModelFactory, PlantModelFactory>();
        services.AddScoped<ISettingModelFactory, SettingModelFactory>();
        services.AddScoped<IOwnerModelFactory, OwnerModelFactory>();
        services.AddScoped<IOwnerRoleModelFactory, OwnerRoleModelFactory>();

        services.AddScoped<IStartupSeeder, StartupSeeder>();
        services.AddScoped<OwnerSeeder>();
        services.AddScoped<LanguageSeeder>();
        services.AddScoped<LocaleResourceSeeder>();
        services.AddScoped<EnumLocalizationSeeder>();

        // Garden data seeders
        services.AddScoped<SoilIngredientSeeder>();
        services.AddScoped<PlantCatalogSeeder>();
        services.AddScoped<FertilizerCatalogSeeder>();
        services.AddScoped<DiseaseCatalogSeeder>();
        services.AddScoped<LocationAndGardenSpotSeeder>();
        services.AddScoped<SoilMixSeeder>();
        services.AddScoped<ContainerSeeder>();
        services.AddScoped<PlantInstanceSeeder>();
        services.AddScoped<WateringScheduleSeeder>();
        services.AddScoped<FertilizingScheduleSeeder>();
        services.AddScoped<PlantDiseaseRecordSeeder>();
        services.AddScoped<RepottingLogSeeder>();

        return services;
    }
}