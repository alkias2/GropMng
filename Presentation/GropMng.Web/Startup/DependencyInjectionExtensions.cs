using System.IO;
using FluentValidation;
using GropMng.Core.Caching;
using GropMng.Core.Domain.Garden.Care;
using GropMng.Core.Domain.Garden.Health;
using GropMng.Core.Domain.Garden.Locations;
using GropMng.Core.Domain.Garden.Plants;
using GropMng.Core.Domain.Garden.Preferences;
using GropMng.Core.Domain.Localization;
using GropMng.Core.Domain.Media;
using GropMng.Core.Domain.Security;
using GropMng.Core.Events;
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
using GropMng.Core.Interfaces.Services.Media;
using GropMng.Core.Interfaces.Services.User;
using GropMng.Data.DbContext;
using GropMng.Data.Repositories;
using GropMng.Services.Services.Configuration;
using GropMng.Services.Caching;
using GropMng.Services.Caching.Garden;
using GropMng.Services.Caching.System;
using GropMng.Services.Events;
using GropMng.Services.Services.Garden.AI;
using GropMng.Services.Services.Garden.Care;
using GropMng.Services.Services.Garden.Health;
using GropMng.Services.Services.Garden.Locations;
using GropMng.Services.Services.Garden.Plants;
using GropMng.Services.Services.Garden.Preferences;
using GropMng.Services.Services.Localization;
using GropMng.Services.Services.Logging;
using GropMng.Services.Services.Media;
using GropMng.Services.Services.User;
using GropMng.Web.Initialization;
using GropMng.Web.Initialization.Options;
using GropMng.Web.Initialization.Seeders;
using GropMng.Web.Framework.UI;
using GropMng.Web.Areas.Admin.Validators.Logging;
using GropMng.Web.Areas.Admin.Factories.Disease;
using GropMng.Web.Areas.Admin.Factories.Fertilizer;
using GropMng.Web.Areas.Admin.Factories.Localization;
using GropMng.Web.Areas.Admin.Factories.Logging;
using GropMng.Web.Areas.Admin.Factories.Pesticide;
using GropMng.Web.Areas.Admin.Factories.Plant;
using GropMng.Web.Areas.Admin.Factories.SoilMix;
using GropMng.Web.Areas.Admin.Factories.Settings;
using GropMng.Web.Areas.Admin.Factories.User;
using GropMng.Web.Factories.Dashboard;
using GropMng.Web.Infrastructure.ModelBinding;
using GropMng.Web.Infrastructure.Navigation;
using Microsoft.AspNetCore.DataProtection;
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
        IConfiguration configuration,
        IHostEnvironment hostEnvironment)
    {
        services.AddCoreFrameworkServices(hostEnvironment);
        services.Configure<OwnerBootstrapOptions>(configuration.GetSection(OwnerBootstrapOptions.SectionName));
        services.Configure<DashboardOptions>(configuration.GetSection(DashboardOptions.SectionName));
        services.AddRequestLocalizationOptions();
        services.AddDataAccessAndDomainServices(configuration);

        return services;
    }

    /// <summary>
    /// Registers core ASP.NET Core and web-framework services used by the application.
    /// </summary>
    /// <param name="services">The service collection being configured.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddCoreFrameworkServices(
        this IServiceCollection services,
        IHostEnvironment hostEnvironment)
    {
        var dataProtectionKeyDirectory = Path.Combine(hostEnvironment.ContentRootPath, "App_Data", "DataProtectionKeys");
        Directory.CreateDirectory(dataProtectionKeyDirectory);

        services
            .AddControllersWithViews(options =>
            {
                options.ModelBinderProviders.Insert(0, new FlexibleDecimalModelBinderProvider());
            })
            .AddRazorRuntimeCompilation();
        services.AddMemoryCache();
        services.AddSingleton<IGropCacheKeyManager, GropCacheKeyManager>();
        services.AddSingleton<IGropStaticCacheManager, GropMemoryCacheManager>();
        services.AddScoped<IGropShortTermCacheManager, GropPerRequestCacheManager>();
        services.AddScoped<IGropCacheKeyService>(serviceProvider => serviceProvider.GetRequiredService<IGropStaticCacheManager>());
        services.AddHttpContextAccessor();
        services.AddDataProtection()
            .SetApplicationName("GropMng.Web")
            .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeyDirectory));
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = "GropMng.OwnerAuth";
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.LoginPath = "/owner/auth/login";
                options.AccessDeniedPath = "/Common/Error";
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = context =>
                    {
                        var requestPath = context.Request.Path.Value ?? string.Empty;
                        var redirectUri = context.RedirectUri ?? string.Empty;

                        if (requestPath.StartsWith("/owner/auth/login", StringComparison.OrdinalIgnoreCase)
                            || redirectUri.Contains("returnUrl=%2Fowner%2Fauth%2Flogin", StringComparison.OrdinalIgnoreCase)
                            || redirectUri.Contains("returnUrl=/owner/auth/login", StringComparison.OrdinalIgnoreCase))
                        {
                            context.Response.Redirect("/owner/auth/login");
                            return Task.CompletedTask;
                        }

                        context.Response.Redirect(redirectUri);
                        return Task.CompletedTask;
                    }
                };
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

        services.AddScoped<IEventPublisher, DefaultEventPublisher>();

        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<ISqlQueryExecutor, SqlQueryExecutor>();

        services.AddScoped<IConsumer<EntityInsertedEvent<WateringSchedule>>, WateringScheduleCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityUpdatedEvent<WateringSchedule>>, WateringScheduleCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityDeletedEvent<WateringSchedule>>, WateringScheduleCacheEventConsumer>();

        services.AddScoped<IConsumer<EntityInsertedEvent<WateringLog>>, WateringLogCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityUpdatedEvent<WateringLog>>, WateringLogCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityDeletedEvent<WateringLog>>, WateringLogCacheEventConsumer>();

        services.AddScoped<IConsumer<EntityInsertedEvent<FertilizingSchedule>>, FertilizingScheduleCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityUpdatedEvent<FertilizingSchedule>>, FertilizingScheduleCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityDeletedEvent<FertilizingSchedule>>, FertilizingScheduleCacheEventConsumer>();

        services.AddScoped<IConsumer<EntityInsertedEvent<FertilizingLog>>, FertilizingLogCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityUpdatedEvent<FertilizingLog>>, FertilizingLogCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityDeletedEvent<FertilizingLog>>, FertilizingLogCacheEventConsumer>();

        services.AddScoped<IConsumer<EntityInsertedEvent<ActionSkip>>, ActionSkipCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityUpdatedEvent<ActionSkip>>, ActionSkipCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityDeletedEvent<ActionSkip>>, ActionSkipCacheEventConsumer>();

        services.AddScoped<IConsumer<EntityInsertedEvent<PlantInstance>>, PlantInstanceCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityUpdatedEvent<PlantInstance>>, PlantInstanceCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityDeletedEvent<PlantInstance>>, PlantInstanceCacheEventConsumer>();

        services.AddScoped<IConsumer<EntityInsertedEvent<Plant>>, PlantCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityUpdatedEvent<Plant>>, PlantCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityDeletedEvent<Plant>>, PlantCacheEventConsumer>();

        services.AddScoped<IConsumer<EntityInsertedEvent<GardenSpot>>, GardenSpotCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityUpdatedEvent<GardenSpot>>, GardenSpotCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityDeletedEvent<GardenSpot>>, GardenSpotCacheEventConsumer>();

        services.AddScoped<IConsumer<EntityInsertedEvent<Location>>, LocationCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityUpdatedEvent<Location>>, LocationCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityDeletedEvent<Location>>, LocationCacheEventConsumer>();

        services.AddScoped<IConsumer<EntityInsertedEvent<PlantPhoto>>, PlantPhotoCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityUpdatedEvent<PlantPhoto>>, PlantPhotoCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityDeletedEvent<PlantPhoto>>, PlantPhotoCacheEventConsumer>();

        services.AddScoped<IConsumer<EntityInsertedEvent<Fertilizer>>, FertilizerCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityUpdatedEvent<Fertilizer>>, FertilizerCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityDeletedEvent<Fertilizer>>, FertilizerCacheEventConsumer>();

        services.AddScoped<IConsumer<EntityInsertedEvent<PlantDiseaseRecord>>, PlantDiseaseRecordCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityUpdatedEvent<PlantDiseaseRecord>>, PlantDiseaseRecordCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityDeletedEvent<PlantDiseaseRecord>>, PlantDiseaseRecordCacheEventConsumer>();

        services.AddScoped<IConsumer<EntityInsertedEvent<PlantNote>>, PlantNoteCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityUpdatedEvent<PlantNote>>, PlantNoteCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityDeletedEvent<PlantNote>>, PlantNoteCacheEventConsumer>();

        services.AddScoped<IConsumer<EntityInsertedEvent<Container>>, ContainerCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityUpdatedEvent<Container>>, ContainerCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityDeletedEvent<Container>>, ContainerCacheEventConsumer>();

        services.AddScoped<IConsumer<EntityInsertedEvent<SoilMix>>, SoilMixCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityUpdatedEvent<SoilMix>>, SoilMixCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityDeletedEvent<SoilMix>>, SoilMixCacheEventConsumer>();

        services.AddScoped<IConsumer<EntityInsertedEvent<SoilIngredient>>, SoilIngredientCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityUpdatedEvent<SoilIngredient>>, SoilIngredientCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityDeletedEvent<SoilIngredient>>, SoilIngredientCacheEventConsumer>();

        services.AddScoped<IConsumer<EntityInsertedEvent<Disease>>, DiseaseCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityUpdatedEvent<Disease>>, DiseaseCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityDeletedEvent<Disease>>, DiseaseCacheEventConsumer>();

        services.AddScoped<IConsumer<EntityInsertedEvent<Language>>, LanguageCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityUpdatedEvent<Language>>, LanguageCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityDeletedEvent<Language>>, LanguageCacheEventConsumer>();

        services.AddScoped<IConsumer<EntityInsertedEvent<LocaleStringResource>>, LocaleStringResourceCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityUpdatedEvent<LocaleStringResource>>, LocaleStringResourceCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityDeletedEvent<LocaleStringResource>>, LocaleStringResourceCacheEventConsumer>();

        services.AddScoped<IConsumer<EntityInsertedEvent<PermissionRecord>>, PermissionRecordCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityUpdatedEvent<PermissionRecord>>, PermissionRecordCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityDeletedEvent<PermissionRecord>>, PermissionRecordCacheEventConsumer>();

        services.AddScoped<IConsumer<EntityInsertedEvent<UserPreference>>, UserPreferenceCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityUpdatedEvent<UserPreference>>, UserPreferenceCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityDeletedEvent<UserPreference>>, UserPreferenceCacheEventConsumer>();

        services.AddScoped<IConsumer<EntityInsertedEvent<Picture>>, PictureCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityUpdatedEvent<Picture>>, PictureCacheEventConsumer>();
        services.AddScoped<IConsumer<EntityDeletedEvent<Picture>>, PictureCacheEventConsumer>();

        services.AddScoped<IAppLogService, AppLogService>();

        services.AddScoped<ILocationService, LocationService>();
        services.AddScoped<IContainerService, ContainerService>();
        services.AddScoped<IPlantService, PlantService>();
        services.AddScoped<IPlantInstanceService, PlantInstanceService>();
        services.AddScoped<IWateringService, WateringService>();
        services.AddScoped<IFertilizingService, FertilizingService>();
        services.AddScoped<IRepottingLogService, RepottingLogService>();
        services.AddScoped<IDiseaseService, DiseaseService>();
        services.AddScoped<IFertilizerService, FertilizerService>();
        services.AddScoped<IPesticideService, PesticideService>();
        services.AddScoped<ISoilMixService, SoilMixService>();
        services.AddScoped<IPlantPhotoService, PlantPhotoService>();
        services.AddScoped<IPlantNoteService, PlantNoteService>();
        services.AddScoped<IPlantDiseaseService, PlantDiseaseService>();

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
        services.AddScoped<IPictureService, PictureService>();

        services.AddScoped<IAppLogModelFactory, AppLogModelFactory>();
        services.AddScoped<ILocalizationModelFactory, LocalizationModelFactory>();
        services.AddScoped<IDashboardModelFactory, DashboardModelFactory>();
        services.AddScoped<IPlantModelFactory, PlantModelFactory>();
        services.AddScoped<IFertilizerModelFactory, FertilizerModelFactory>();
        services.AddScoped<IPesticideModelFactory, PesticideModelFactory>();
        services.AddScoped<IDiseaseModelFactory, DiseaseModelFactory>();
        services.AddScoped<ISoilMixModelFactory, SoilMixModelFactory>();
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
        services.AddScoped<PlantPhotoSeeder>();
        services.AddScoped<WateringScheduleSeeder>();
        services.AddScoped<FertilizingScheduleSeeder>();
        services.AddScoped<PlantDiseaseRecordSeeder>();
        services.AddScoped<RepottingLogSeeder>();

        return services;
    }
}