using GropMng.Core.Domain.Logging;
using GropMng.Core.Interfaces.Services.Configuration;
using GropMng.Core.Interfaces.Services.Logging;
using GropMng.Data.DbContext;
using GropMng.Web.Initialization;
using Microsoft.EntityFrameworkCore;

namespace GropMng.Web.Startup;

/// <summary>
/// Provides startup extension methods that execute database initialization and seeding.
/// </summary>
public static class ApplicationInitializationExtensions
{
    /// <summary>
    /// Runs database migration, baseline seeding, and startup logging while preserving the current startup policy.
    /// </summary>
    /// <param name="app">The web application being initialized.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous initialization operation.</returns>
    public static async Task RunStartupInitializationAsync(this WebApplication app, CancellationToken cancellationToken = default)
    {
        var sqlServerSettings = GropContextConfiguration.ResolveSqlServerSettings(app.Configuration);

        if (sqlServerSettings.IsEnabled && !sqlServerSettings.CanConnect)
        {
            var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
            startupLogger.LogWarning("SQL Server is enabled but no usable connection string was resolved. Reason: {Reason}", sqlServerSettings.FailureReason);
        }

        if (!sqlServerSettings.CanConnect)
            return;

        try
        {
            using var scope = app.Services.CreateScope();
            var sqlServerContext = scope.ServiceProvider.GetRequiredService<GropContext>();
            await sqlServerContext.Database.MigrateAsync(cancellationToken);

            var startupSeeder = scope.ServiceProvider.GetRequiredService<IStartupSeeder>();
            await startupSeeder.SeedAsync(cancellationToken);

            var appLogService = scope.ServiceProvider.GetService<IAppLogService>();
            if (appLogService is not null)
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
}