using Microsoft.EntityFrameworkCore;
using GropMng.Core.Domain.Logging;
using GropMng.Data.DbContext;
using GropMng.Core.Interfaces.Services;
using GropMng.Services.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();


var enableSqlServer = builder.Configuration.GetValue<bool>("Database:EnableSqlServer");
var sqlServerConnection = builder.Configuration.GetConnectionString("DefaultConnection");


if (enableSqlServer && !string.IsNullOrWhiteSpace(sqlServerConnection))
{
    builder.Services.AddDbContext<GropContext>(options =>
        options.UseSqlServer(sqlServerConnection));

    builder.Services.AddScoped<IAppLogService, AppLogService>();
}


var app = builder.Build();

if (enableSqlServer && !string.IsNullOrWhiteSpace(sqlServerConnection))
{
    try
    {
        using var scope = app.Services.CreateScope();
        var sqlServerContext = scope.ServiceProvider.GetRequiredService<GropContext>();
        sqlServerContext.Database.Migrate();

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
