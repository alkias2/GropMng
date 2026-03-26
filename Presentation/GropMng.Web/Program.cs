using FluentValidation;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using GropMng.Core.Domain.Garden.Owners;
using GropMng.Core.Domain.Localization;
using GropMng.Core.Domain.Logging;
using GropMng.Data.DbContext;
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
using GropMng.Data.Repositories;
using GropMng.Services.Services.Garden.AI;
using GropMng.Services.Services.Garden.Care;
using GropMng.Services.Services.Garden.Health;
using GropMng.Services.Services.Garden.Locations;
using GropMng.Services.Services.Garden.Plants;
using GropMng.Services.Services.Garden.Preferences;
using GropMng.Services.Services.Configuration;
using GropMng.Services.Services.Localization;
using GropMng.Services.Services.Logging;
using GropMng.Services.Services.User;
using GropMng.Web.Areas.Admin.Validators.Logging;
using GropMng.Web.Factories.Logging;
using GropMng.Web.Factories.Plant;
using GropMng.Web.Factories.Settings;
using GropMng.Web.Infrastructure.Navigation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();

// Configure request localization with supported cultures
var supportedCultures = new[] 
{
    new CultureInfo("el-GR"),
    new CultureInfo("en-US")
};
builder.Services.Configure<RequestLocalizationOptions>(options =>
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

// Add IHttpContextAccessor to allow services to access HttpContext
builder.Services.AddHttpContextAccessor();

builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(Program).Assembly));
builder.Services.AddValidatorsFromAssemblyContaining<AppLogSearchModelValidator>();
builder.Services.AddScoped<IAppMenuProvider, DefaultAppMenuProvider>();

var sqlServerSettings = GropContextConfiguration.ResolveSqlServerSettings(builder.Configuration);

if (sqlServerSettings.CanConnect)
{
    builder.Services.AddDbContext<GropContext>(options =>
        options.UseSqlServer(sqlServerSettings.ConnectionString));

    builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
    builder.Services.AddScoped<ISqlQueryExecutor, SqlQueryExecutor>();
    
    // Logging services
    builder.Services.AddScoped<IAppLogService, AppLogService>();
    
    // Garden domain services
    builder.Services.AddScoped<ILocationService, LocationService>();
    builder.Services.AddScoped<IPlantService, PlantService>();
    builder.Services.AddScoped<IPlantInstanceService, PlantInstanceService>();
    builder.Services.AddScoped<IDiseaseService, DiseaseService>();
    builder.Services.AddScoped<IFertilizerService, FertilizerService>();
    builder.Services.AddScoped<IPesticideService, PesticideService>();
    
    // AI domain services
    builder.Services.AddScoped<IAIQueryTemplateService, AIQueryTemplateService>();
    
    // User domain services
    builder.Services.AddScoped<GropMng.Core.Interfaces.Services.Garden.Preferences.IUserPreferenceService, UserPreferenceService>();
    builder.Services.AddScoped<ICurrentOwnerProvider, CurrentOwnerProvider>();

    // Localization services
    builder.Services.AddScoped<ILanguageService, LanguageService>();
    builder.Services.AddScoped<ILocalizationService, LocalizationService>();
    builder.Services.AddScoped<IEnumLocalizationHelper, EnumLocalizationHelper>();

    // Configuration settings service (global scope, no store overrides)
    builder.Services.AddScoped<ISettingService, SettingService>();

    // Web factories
    builder.Services.AddScoped<IAppLogModelFactory, AppLogModelFactory>();
    builder.Services.AddScoped<IPlantModelFactory, PlantModelFactory>();
    builder.Services.AddScoped<ISettingModelFactory, SettingModelFactory>();
}


var app = builder.Build();

if (sqlServerSettings.IsEnabled && !sqlServerSettings.CanConnect)
{
    var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    startupLogger.LogWarning("SQL Server is enabled but no usable connection string was resolved. Reason: {Reason}", sqlServerSettings.FailureReason);
}

if (sqlServerSettings.CanConnect)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var sqlServerContext = scope.ServiceProvider.GetRequiredService<GropContext>();
        sqlServerContext.Database.Migrate();
        await SeedInitialOwnerAndLocalizationAsync(sqlServerContext);

        var appLogService = scope.ServiceProvider.GetService<IAppLogService>();
        if (appLogService != null)
        {
            await appLogService.InsertLogAsync(new AppLog
            {
                Level = "Information",
                Category = "Startup",
                Message = "SQL Server logging context initialized successfully.",
                Timestamp = DateTime.UtcNow
            });
        }
    }
    catch (Exception ex)
    {
        var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
        startupLogger.LogWarning(ex, "SQL Server initialization failed. Application will continue with SQLite data context.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Common/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseRequestLocalization();

app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

static async Task SeedInitialOwnerAndLocalizationAsync(GropContext dbContext)
{
    var defaultOwnerBusinessId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    var defaultOwnerEmail = "owner@gropmng.local";

    var owner = await dbContext.Owners.FirstOrDefaultAsync(entity => entity.Email == defaultOwnerEmail);
    if (owner is null)
    {
        var now = DateTime.UtcNow;
        owner = new Owner
        {
            OwnerId = defaultOwnerBusinessId,
            FirstName = "Default",
            LastName = "Owner",
            Email = defaultOwnerEmail,
            PasswordHash = ComputeSha256("ChangeMe123!"),
            IsActive = true,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            IsDeleted = false
        };

        dbContext.Owners.Add(owner);
        await dbContext.SaveChangesAsync();
    }

    var greekLanguage = await dbContext.Languages.FirstOrDefaultAsync(entity => entity.UniqueSeoCode == "el");
    if (greekLanguage is null)
    {
        greekLanguage = new Language
        {
            Name = "Greek",
            LanguageCulture = "el-GR",
            UniqueSeoCode = "el",
            Published = true,
            DisplayOrder = 0,
            Rtl = false,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow
        };

        dbContext.Languages.Add(greekLanguage);
    }

    var englishLanguage = await dbContext.Languages.FirstOrDefaultAsync(entity => entity.UniqueSeoCode == "en");
    if (englishLanguage is null)
    {
        englishLanguage = new Language
        {
            Name = "English",
            LanguageCulture = "en-US",
            UniqueSeoCode = "en",
            Published = true,
            DisplayOrder = 1,
            Rtl = false,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow
        };

        dbContext.Languages.Add(englishLanguage);
    }

    await dbContext.SaveChangesAsync();

    await SeedLocaleResourcesAsync(dbContext, greekLanguage.Id, new Dictionary<string, string>
    {
        ["common.save"] = "Αποθήκευση",
        ["common.create"] = "Δημιουργία",
        ["common.edit"] = "Επεξεργασία",
        ["common.delete"] = "Διαγραφή",
        ["common.apply"] = "Εφαρμογή",
        ["common.clear"] = "Καθαρισμός",
        ["common.cancel"] = "Ακύρωση",
        ["common.search"] = "Αναζήτηση",
        ["common.filters"] = "Φίλτρα",
        ["common.actions"] = "Ενέργειες",
        ["common.details"] = "Λεπτομέρειες",
        ["common.dashboard"] = "Πίνακας Ελέγχου",
        ["common.general"] = "Γενικά",
        ["common.related"] = "Σχετικά",
        ["common.none"] = "Κανένα",
        ["common.yes"] = "Ναι",
        ["common.no"] = "Όχι",
        ["common.loading"] = "Φόρτωση...",
        ["common.fromdate"] = "Από Ημερομηνία",
        ["common.todate"] = "Έως Ημερομηνία",
        ["common.dangerzone"] = "Επικίνδυνη Ζώνη",
        ["common.delete.cannotundo"] = "Αυτή η ενέργεια δεν αναιρείται.",
        ["admin.datatable.lengthmenu"] = "Εμφάνιση _MENU_ εγγραφών",
        ["admin.datatable.info"] = "Εμφάνιση _START_ έως _END_ από _TOTAL_ εγγραφές",
        ["admin.datatable.infoempty"] = "Εμφάνιση 0 έως 0 από 0 εγγραφές",
        ["admin.datatable.infofiltered"] = "(φιλτραρισμένες από _MAX_ συνολικές εγγραφές)",
        ["admin.datatable.emptytable"] = "Δεν υπάρχουν διαθέσιμα δεδομένα στον πίνακα",
        ["admin.datatable.zerorecords"] = "Δεν βρέθηκαν εγγραφές",
        ["admin.datatable.search"] = "Αναζήτηση:",
        ["admin.datatable.processing"] = "Φόρτωση...",
        ["admin.datatable.paginate.first"] = "Πρώτη",
        ["admin.datatable.paginate.last"] = "Τελευταία",
        ["admin.datatable.paginate.next"] = "Επόμενη",
        ["admin.datatable.paginate.previous"] = "Προηγούμενη",
        ["admin.plant.title"] = "Φυτά",
        ["admin.plant.create"] = "Νέο Φυτό",
        ["admin.plant.edit"] = "Επεξεργασία Φυτού",
        ["admin.plant.delete"] = "Διαγραφή Φυτού",
        ["admin.plant.delete.title"] = "Διαγραφή φυτού",
        ["admin.plant.delete.confirm"] = "Το φυτό θα διαγραφεί οριστικά.",
        ["admin.plant.notifications.create.success"] = "Το φυτό δημιουργήθηκε με επιτυχία.",
        ["admin.plant.notifications.edit.success"] = "Το φυτό ενημερώθηκε με επιτυχία.",
        ["admin.plant.notifications.delete.success"] = "Το φυτό διαγράφηκε με επιτυχία.",
        ["admin.plant.search.placeholder"] = "Κοινή ονομασία, επιστημονική ονομασία, οικογένεια",
        ["admin.plant.fields.commonname"] = "Κοινή Ονομασία",
        ["admin.plant.fields.scientificname"] = "Επιστημονική Ονομασία",
        ["admin.plant.fields.family"] = "Οικογένεια",
        ["admin.plant.fields.category"] = "Κατηγορία",
        ["admin.plant.fields.growthtype"] = "Τύπος Ανάπτυξης",
        ["admin.plant.fields.sunrequirement"] = "Ανάγκη Ήλιου",
        ["admin.plant.fields.waterrequirement"] = "Ανάγκη Νερού",
        ["admin.plant.fields.mintempcelsius"] = "Ελάχιστη Θερμοκρασία (°C)",
        ["admin.plant.fields.maxtempcelsius"] = "Μέγιστη Θερμοκρασία (°C)",
        ["admin.plant.fields.isedible"] = "Βρώσιμο",
        ["admin.plant.fields.ismedicinal"] = "Φαρμακευτικό",
        ["admin.plant.fields.istoxic"] = "Τοξικό",
        ["admin.plant.fields.generalnotes"] = "Γενικές Σημειώσεις",
        ["admin.plant.category.all"] = "Όλες οι κατηγορίες",
        ["admin.plant.grid.id"] = "ID",
        ["admin.plant.grid.flags"] = "Χαρακτηριστικά",
        ["admin.plant.category.shrub"] = "Θάμνος",
        ["admin.plant.category.tree"] = "Δέντρο",
        ["admin.plant.category.climber"] = "Αναρριχώμενο",
        ["admin.plant.category.ornamental"] = "Καλλωπιστικό",
        ["admin.plant.category.edible"] = "Βρώσιμο",
        ["admin.plant.category.aromatic"] = "Αρωματικό",
        ["admin.plant.category.succulent"] = "Παχύφυτο",
        ["admin.plant.category.grass"] = "Χόρτο",
        ["admin.plant.category.fern"] = "Φτέρη",
        ["admin.plant.category.other"] = "Άλλο",
        ["admin.plant.growthtype.annual"] = "Ετήσιο",
        ["admin.plant.growthtype.biennial"] = "Διετές",
        ["admin.plant.growthtype.perennial"] = "Πολυετές",
        ["admin.plant.growthtype.bulb"] = "Βολβός",
        ["admin.plant.sunrequirement.fullsun"] = "Πλήρης Ήλιος",
        ["admin.plant.sunrequirement.partialshade"] = "Ημισκιά",
        ["admin.plant.sunrequirement.fullshade"] = "Πλήρης Σκιά",
        ["admin.plant.waterrequirement.low"] = "Χαμηλή",
        ["admin.plant.waterrequirement.moderate"] = "Μέτρια",
        ["admin.plant.waterrequirement.high"] = "Υψηλή",
        ["admin.plant.flags.edible"] = "Βρώσιμο",
        ["admin.plant.flags.medicinal"] = "Φαρμακευτικό",
        ["admin.plant.flags.toxic"] = "Τοξικό",
        ["admin.plant.growth.environment"] = "Ανάπτυξη και Περιβάλλον",
        ["admin.applog.title"] = "Καταγραφές Εφαρμογής",
        ["admin.applog.breadcrumb"] = "Καταγραφές",
        ["admin.applog.delete.selected"] = "Διαγραφή Επιλεγμένων",
        ["admin.applog.delete.all"] = "Διαγραφή Όλων",
        ["admin.applog.delete.all.title"] = "Διαγραφή όλων των καταγραφών",
        ["admin.applog.delete.all.confirm"] = "Αυτή η ενέργεια θα διαγράψει οριστικά όλες τις καταγραφές.",
        ["admin.applog.delete.all.confirmbutton"] = "Διαγραφή όλων",
        ["admin.applog.filters.alllevels"] = "Όλα τα Επίπεδα",
        ["admin.applog.grid.selectallvisible"] = "Επιλογή όλων των ορατών",
        ["admin.applog.fields.id"] = "ID",
        ["admin.applog.fields.level"] = "Επίπεδο",
        ["admin.applog.fields.category"] = "Κατηγορία",
        ["admin.applog.fields.message"] = "Μήνυμα",
        ["admin.applog.fields.timestamp"] = "Χρονική Σήμανση",
        ["admin.applog.delete.selected.title"] = "Διαγραφή επιλεγμένων καταγραφών",
        ["admin.applog.delete.selected.confirm"] = "Αυτή η ενέργεια θα διαγράψει οριστικά {0} επιλεγμένες καταγραφές.",
        ["admin.applog.delete.selected.confirm.single"] = "Αυτή η ενέργεια θα διαγράψει οριστικά 1 επιλεγμένη καταγραφή.",
        ["admin.applog.delete.selected.confirmbutton"] = "Διαγραφή επιλεγμένων",
        ["admin.applog.delete.selected.success"] = "Οι επιλεγμένες καταγραφές διαγράφηκαν.",
        ["admin.applog.delete.selected.error"] = "Αποτυχία διαγραφής επιλεγμένων καταγραφών.",
        ["admin.applog.delete.single.title"] = "Διαγραφή καταγραφής",
        ["admin.applog.delete.single.confirm"] = "Η καταγραφή θα διαγραφεί οριστικά.",
        ["admin.applog.delete.single.success"] = "Η καταγραφή διαγράφηκε.",
        ["admin.applog.delete.single.error"] = "Αποτυχία διαγραφής καταγραφής.",
        ["admin.applog.level.trace"] = "Ανίχνευση",
        ["admin.applog.level.debug"] = "Αποσφαλμάτωση",
        ["admin.applog.level.information"] = "Πληροφορία",
        ["admin.applog.level.warning"] = "Προειδοποίηση",
        ["admin.applog.level.error"] = "Σφάλμα",
        ["admin.applog.level.critical"] = "Κρίσιμο",
        ["admin.localization.languages.title"] = "Γλώσσες",
        ["admin.localization.language.create"] = "Νέα Γλώσσα",
        ["admin.localization.language.edit"] = "Επεξεργασία Γλώσσας",
        ["admin.localization.language.delete"] = "Διαγραφή Γλώσσας",
        ["admin.localization.language.delete.confirm"] = "Η γλώσσα θα διαγραφεί οριστικά.",
        ["admin.localization.language.create.success"] = "Η γλώσσα δημιουργήθηκε με επιτυχία.",
        ["admin.localization.language.edit.success"] = "Η γλώσσα ενημερώθηκε με επιτυχία.",
        ["admin.localization.language.delete.success"] = "Η γλώσσα διαγράφηκε με επιτυχία.",
        ["admin.localization.language.delete.hasresources"] = "Η γλώσσα δεν μπορεί να διαγραφεί όσο υπάρχουν πόροι.",
        ["admin.localization.language.fields.name"] = "Όνομα",
        ["admin.localization.language.fields.culture"] = "Culture",
        ["admin.localization.language.fields.seocode"] = "SEO Code",
        ["admin.localization.language.fields.flag"] = "Αρχείο Σημαίας",
        ["admin.localization.language.fields.rtl"] = "RTL",
        ["admin.localization.language.fields.published"] = "Δημοσιευμένη",
        ["admin.localization.language.fields.displayorder"] = "Σειρά Εμφάνισης",
        ["admin.localization.resources.title"] = "Πόροι Μετάφρασης",
        ["admin.localization.resource.create"] = "Νέος Πόρος",
        ["admin.localization.resource.edit"] = "Επεξεργασία Πόρου",
        ["admin.localization.resource.delete"] = "Διαγραφή Πόρου",
        ["admin.localization.resource.delete.confirm"] = "Ο πόρος θα διαγραφεί οριστικά.",
        ["admin.localization.resource.create.success"] = "Ο πόρος δημιουργήθηκε με επιτυχία.",
        ["admin.localization.resource.edit.success"] = "Ο πόρος ενημερώθηκε με επιτυχία.",
        ["admin.localization.resource.delete.success"] = "Ο πόρος διαγράφηκε με επιτυχία.",
        ["admin.localization.resource.export"] = "Εξαγωγή XML",
        ["admin.localization.resource.import"] = "Εισαγωγή XML",
        ["admin.localization.resource.import.success"] = "Οι πόροι εισήχθησαν με επιτυχία.",
        ["admin.localization.resource.import.empty"] = "Παρακαλώ επίλεξε έγκυρο XML αρχείο.",
        ["admin.localization.resource.import.updateexisting"] = "Ενημέρωση υπαρχόντων πόρων",
        ["admin.localization.resource.fields.name"] = "Κλειδί",
        ["admin.localization.resource.fields.value"] = "Τιμή"
    });

    await SeedLocaleResourcesAsync(dbContext, englishLanguage.Id, new Dictionary<string, string>
    {
        ["common.save"] = "Save",
        ["common.create"] = "Create",
        ["common.edit"] = "Edit",
        ["common.delete"] = "Delete",
        ["common.apply"] = "Apply",
        ["common.clear"] = "Clear",
        ["common.cancel"] = "Cancel",
        ["common.search"] = "Search",
        ["common.filters"] = "Filters",
        ["common.actions"] = "Actions",
        ["common.details"] = "Details",
        ["common.dashboard"] = "Dashboard",
        ["common.general"] = "General",
        ["common.related"] = "Related",
        ["common.none"] = "None",
        ["common.yes"] = "Yes",
        ["common.no"] = "No",
        ["common.loading"] = "Loading...",
        ["common.fromdate"] = "From Date",
        ["common.todate"] = "To Date",
        ["common.dangerzone"] = "Danger Zone",
        ["common.delete.cannotundo"] = "This action cannot be undone.",
        ["admin.datatable.lengthmenu"] = "Show _MENU_ entries",
        ["admin.datatable.info"] = "Showing _START_ to _END_ of _TOTAL_ entries",
        ["admin.datatable.infoempty"] = "Showing 0 to 0 of 0 entries",
        ["admin.datatable.infofiltered"] = "(filtered from _MAX_ total entries)",
        ["admin.datatable.emptytable"] = "No data available in table",
        ["admin.datatable.zerorecords"] = "No matching records found",
        ["admin.datatable.search"] = "Search:",
        ["admin.datatable.processing"] = "Loading...",
        ["admin.datatable.paginate.first"] = "First",
        ["admin.datatable.paginate.last"] = "Last",
        ["admin.datatable.paginate.next"] = "Next",
        ["admin.datatable.paginate.previous"] = "Previous",
        ["admin.plant.title"] = "Plants",
        ["admin.plant.create"] = "Create Plant",
        ["admin.plant.edit"] = "Edit Plant",
        ["admin.plant.delete"] = "Delete Plant",
        ["admin.plant.delete.title"] = "Delete plant",
        ["admin.plant.delete.confirm"] = "This plant will be permanently deleted.",
        ["admin.plant.notifications.create.success"] = "Plant was created successfully.",
        ["admin.plant.notifications.edit.success"] = "Plant was updated successfully.",
        ["admin.plant.notifications.delete.success"] = "Plant was deleted successfully.",
        ["admin.plant.search.placeholder"] = "Common name, scientific name, family",
        ["admin.plant.fields.commonname"] = "Common Name",
        ["admin.plant.fields.scientificname"] = "Scientific Name",
        ["admin.plant.fields.family"] = "Family",
        ["admin.plant.fields.category"] = "Category",
        ["admin.plant.fields.growthtype"] = "Growth Type",
        ["admin.plant.fields.sunrequirement"] = "Sun Requirement",
        ["admin.plant.fields.waterrequirement"] = "Water Requirement",
        ["admin.plant.fields.mintempcelsius"] = "Min Temperature (°C)",
        ["admin.plant.fields.maxtempcelsius"] = "Max Temperature (°C)",
        ["admin.plant.fields.isedible"] = "Edible",
        ["admin.plant.fields.ismedicinal"] = "Medicinal",
        ["admin.plant.fields.istoxic"] = "Toxic",
        ["admin.plant.fields.generalnotes"] = "General Notes",
        ["admin.plant.category.all"] = "All Categories",
        ["admin.plant.grid.id"] = "ID",
        ["admin.plant.grid.flags"] = "Flags",
        ["admin.plant.category.shrub"] = "Shrub",
        ["admin.plant.category.tree"] = "Tree",
        ["admin.plant.category.climber"] = "Climber",
        ["admin.plant.category.ornamental"] = "Ornamental",
        ["admin.plant.category.edible"] = "Edible",
        ["admin.plant.category.aromatic"] = "Aromatic",
        ["admin.plant.category.succulent"] = "Succulent",
        ["admin.plant.category.grass"] = "Grass",
        ["admin.plant.category.fern"] = "Fern",
        ["admin.plant.category.other"] = "Other",
        ["admin.plant.growthtype.annual"] = "Annual",
        ["admin.plant.growthtype.biennial"] = "Biennial",
        ["admin.plant.growthtype.perennial"] = "Perennial",
        ["admin.plant.growthtype.bulb"] = "Bulb",
        ["admin.plant.sunrequirement.fullsun"] = "Full Sun",
        ["admin.plant.sunrequirement.partialshade"] = "Partial Shade",
        ["admin.plant.sunrequirement.fullshade"] = "Full Shade",
        ["admin.plant.waterrequirement.low"] = "Low",
        ["admin.plant.waterrequirement.moderate"] = "Moderate",
        ["admin.plant.waterrequirement.high"] = "High",
        ["admin.plant.flags.edible"] = "Edible",
        ["admin.plant.flags.medicinal"] = "Medicinal",
        ["admin.plant.flags.toxic"] = "Toxic",
        ["admin.plant.growth.environment"] = "Growth and Environment",
        ["admin.applog.title"] = "Application Logs",
        ["admin.applog.breadcrumb"] = "App Logs",
        ["admin.applog.delete.selected"] = "Delete Selected",
        ["admin.applog.delete.all"] = "Delete All",
        ["admin.applog.delete.all.title"] = "Delete all log entries",
        ["admin.applog.delete.all.confirm"] = "This action will permanently delete all log entries.",
        ["admin.applog.delete.all.confirmbutton"] = "Delete all",
        ["admin.applog.filters.alllevels"] = "All Levels",
        ["admin.applog.grid.selectallvisible"] = "Select all visible",
        ["admin.applog.fields.id"] = "ID",
        ["admin.applog.fields.level"] = "Level",
        ["admin.applog.fields.category"] = "Category",
        ["admin.applog.fields.message"] = "Message",
        ["admin.applog.fields.timestamp"] = "Timestamp",
        ["admin.applog.delete.selected.title"] = "Delete selected logs",
        ["admin.applog.delete.selected.confirm"] = "This action will permanently delete {0} selected log entries.",
        ["admin.applog.delete.selected.confirm.single"] = "This action will permanently delete 1 selected log entry.",
        ["admin.applog.delete.selected.confirmbutton"] = "Delete selected",
        ["admin.applog.delete.selected.success"] = "Selected log entries were deleted.",
        ["admin.applog.delete.selected.error"] = "Failed to delete selected log entries.",
        ["admin.applog.delete.single.title"] = "Delete log entry",
        ["admin.applog.delete.single.confirm"] = "This log entry will be permanently deleted.",
        ["admin.applog.delete.single.success"] = "Log entry deleted.",
        ["admin.applog.delete.single.error"] = "Failed to delete the log entry.",
        ["admin.applog.level.trace"] = "Trace",
        ["admin.applog.level.debug"] = "Debug",
        ["admin.applog.level.information"] = "Information",
        ["admin.applog.level.warning"] = "Warning",
        ["admin.applog.level.error"] = "Error",
        ["admin.applog.level.critical"] = "Critical",
        ["admin.localization.languages.title"] = "Languages",
        ["admin.localization.language.create"] = "Create Language",
        ["admin.localization.language.edit"] = "Edit Language",
        ["admin.localization.language.delete"] = "Delete Language",
        ["admin.localization.language.delete.confirm"] = "This language will be permanently deleted.",
        ["admin.localization.language.create.success"] = "Language created successfully.",
        ["admin.localization.language.edit.success"] = "Language updated successfully.",
        ["admin.localization.language.delete.success"] = "Language deleted successfully.",
        ["admin.localization.language.delete.hasresources"] = "Language cannot be deleted while it has resources.",
        ["admin.localization.language.fields.name"] = "Name",
        ["admin.localization.language.fields.culture"] = "Culture",
        ["admin.localization.language.fields.seocode"] = "SEO Code",
        ["admin.localization.language.fields.flag"] = "Flag File Name",
        ["admin.localization.language.fields.rtl"] = "RTL",
        ["admin.localization.language.fields.published"] = "Published",
        ["admin.localization.language.fields.displayorder"] = "Display Order",
        ["admin.localization.resources.title"] = "Locale Resources",
        ["admin.localization.resource.create"] = "Create Resource",
        ["admin.localization.resource.edit"] = "Edit Resource",
        ["admin.localization.resource.delete"] = "Delete Resource",
        ["admin.localization.resource.delete.confirm"] = "This resource will be permanently deleted.",
        ["admin.localization.resource.create.success"] = "Resource created successfully.",
        ["admin.localization.resource.edit.success"] = "Resource updated successfully.",
        ["admin.localization.resource.delete.success"] = "Resource deleted successfully.",
        ["admin.localization.resource.export"] = "Export XML",
        ["admin.localization.resource.import"] = "Import XML",
        ["admin.localization.resource.import.success"] = "Resources imported successfully.",
        ["admin.localization.resource.import.empty"] = "Please select a valid XML file.",
        ["admin.localization.resource.import.updateexisting"] = "Update existing resources",
        ["admin.localization.resource.fields.name"] = "Key",
        ["admin.localization.resource.fields.value"] = "Value"
    });
}

static async Task SeedLocaleResourcesAsync(GropContext dbContext, int languageId, IReadOnlyDictionary<string, string> resources)
{
    var existingResourceNames = await dbContext.LocaleStringResources
        .Where(entity => entity.LanguageId == languageId)
        .Select(entity => entity.ResourceName)
        .ToListAsync();

    var existingSet = existingResourceNames
        .Select(name => name.Trim().ToLowerInvariant())
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    var now = DateTime.UtcNow;
    foreach (var resource in resources)
    {
        var key = resource.Key.Trim().ToLowerInvariant();
        if (existingSet.Contains(key))
            continue;

        dbContext.LocaleStringResources.Add(new LocaleStringResource
        {
            LanguageId = languageId,
            ResourceName = key,
            ResourceValue = resource.Value,
            CreatedOnUtc = now,
            UpdatedOnUtc = now
        });
    }

    await dbContext.SaveChangesAsync();
}

static string ComputeSha256(string raw)
{
    var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
    return Convert.ToHexString(bytes);
}

app.Run();
