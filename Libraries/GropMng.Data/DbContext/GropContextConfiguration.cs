using Microsoft.Extensions.Configuration;
using System.Xml.Linq;

namespace GropMng.Data.DbContext;

public sealed record SqlServerConnectionSettings(bool IsEnabled, string? ConnectionString, string? FailureReason)
{
    public bool CanConnect => IsEnabled && !string.IsNullOrWhiteSpace(ConnectionString);
}

public sealed record DesignTimeConfigurationContext(IConfigurationRoot Configuration, string StartupProjectPath, string EnvironmentName, string? UserSecretsFilePath);

public static class GropContextConfiguration
{
    private const string StartupProjectRelativePath = "Presentation/GropMng.Web";
    private const string StartupProjectName = "GropMng.Web";
    private const string StartupProjectFileName = "GropMng.Web.csproj";
    private const string StartupProjectPathEnvironmentVariable = "GROPMNG_STARTUP_PROJECT_PATH";

    public static SqlServerConnectionSettings ResolveSqlServerSettings(IConfiguration configuration)
    {
        var enableSqlServer = string.Equals(configuration["Database:EnableSqlServer"], bool.TrueString, StringComparison.OrdinalIgnoreCase);
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (!enableSqlServer)
        {
            return new SqlServerConnectionSettings(false, connectionString, "Database:EnableSqlServer is not enabled.");
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return new SqlServerConnectionSettings(true, null, "ConnectionStrings:DefaultConnection is missing or empty.");
        }

        return new SqlServerConnectionSettings(true, connectionString, null);
    }

    public static DesignTimeConfigurationContext BuildDesignTimeConfiguration()
    {
        var startupProjectPath = ResolveStartupProjectPath();
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var startupProjectFilePath = Path.Combine(startupProjectPath, StartupProjectFileName);
        var userSecretsFilePath = ResolveUserSecretsFilePath(startupProjectFilePath);

        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(startupProjectPath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
            .AddEnvironmentVariables();

        if (userSecretsFilePath is not null)
        {
            configurationBuilder.AddJsonFile(userSecretsFilePath, optional: true, reloadOnChange: false);
        }

        return new DesignTimeConfigurationContext(configurationBuilder.Build(), startupProjectPath, environmentName, userSecretsFilePath);
    }

    private static string ResolveStartupProjectPath()
    {
        var explicitPath = Environment.GetEnvironmentVariable(StartupProjectPathEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(explicitPath))
        {
            var normalizedExplicitPath = Path.GetFullPath(explicitPath);
            if (Directory.Exists(normalizedExplicitPath) && File.Exists(Path.Combine(normalizedExplicitPath, StartupProjectFileName)))
            {
                return normalizedExplicitPath;
            }

            throw new DirectoryNotFoundException(
                $"The path from environment variable '{StartupProjectPathEnvironmentVariable}' does not point to the startup project '{StartupProjectFileName}': '{normalizedExplicitPath}'.");
        }

        var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (currentDirectory is not null)
        {
            var candidateRelativePath = Path.Combine(currentDirectory.FullName, StartupProjectRelativePath);
            if (Directory.Exists(candidateRelativePath) && File.Exists(Path.Combine(candidateRelativePath, StartupProjectFileName)))
            {
                return candidateRelativePath;
            }

            var candidateDirectPath = Path.Combine(currentDirectory.FullName, StartupProjectName);
            if (Directory.Exists(candidateDirectPath) && File.Exists(Path.Combine(candidateDirectPath, StartupProjectFileName)))
            {
                return candidateDirectPath;
            }

            currentDirectory = currentDirectory.Parent;
        }

        throw new DirectoryNotFoundException(
            $"Could not locate the startup project path for design-time operations. Checked '{StartupProjectRelativePath}' and direct '{StartupProjectName}' folders from the current working directory upwards.");
    }

    private static string? ResolveUserSecretsFilePath(string startupProjectFilePath)
    {
        if (!File.Exists(startupProjectFilePath))
        {
            return null;
        }

        var projectDocument = XDocument.Load(startupProjectFilePath);
        var userSecretsId = projectDocument
            .Descendants()
            .FirstOrDefault(element => string.Equals(element.Name.LocalName, "UserSecretsId", StringComparison.Ordinal))
            ?.Value
            .Trim();

        if (string.IsNullOrWhiteSpace(userSecretsId))
        {
            return null;
        }

        var secretsRoot = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var secretsFilePath = Path.Combine(secretsRoot, "Microsoft", "UserSecrets", userSecretsId, "secrets.json");

        return File.Exists(secretsFilePath) ? secretsFilePath : null;
    }
}