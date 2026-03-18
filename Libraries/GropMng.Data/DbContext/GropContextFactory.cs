using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GropMng.Data.DbContext;

/// <summary>
/// Defines the Entity Framework Core context represented by GropContextFactory.
/// Exposes mapped sets and configuration required for database interaction.
/// </summary>
public class GropContextFactory : IDesignTimeDbContextFactory<GropContext>
{
    public GropContext CreateDbContext(string[] args)
    {
        var designTimeConfiguration = GropContextConfiguration.BuildDesignTimeConfiguration();
        var sqlServerSettings = GropContextConfiguration.ResolveSqlServerSettings(designTimeConfiguration.Configuration);

        if (!sqlServerSettings.CanConnect)
        {
            throw new InvalidOperationException(
                $"Unable to resolve a SQL Server connection string for design-time operations. " +
                $"Startup project path: '{designTimeConfiguration.StartupProjectPath}'. " +
                $"Environment: '{designTimeConfiguration.EnvironmentName}'. " +
                $"User secrets file: '{designTimeConfiguration.UserSecretsFilePath ?? "not found"}'. " +
                $"Reason: {sqlServerSettings.FailureReason}");
        }

        var optionsBuilder = new DbContextOptionsBuilder<GropContext>();

        optionsBuilder.UseSqlServer(sqlServerSettings.ConnectionString!);

        return new GropContext(optionsBuilder.Options);
    }
}
