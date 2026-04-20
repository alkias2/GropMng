using GropMng.Core.Domain.Security;

namespace GropMng.Web.Infrastructure.Security;

/// <summary>
/// Defines the permission system names used by the application.
/// </summary>
public static class GropMngPermissions
{
    public static class Security
    {
        public const string AccessAdminArea = "Security.AccessAdminArea";
    }

    public static class Owners
    {
        public const string ManageOwners = "Owners.Manage";
        public const string ManageRoles = "Owners.ManageRoles";
    }

    public static class Garden
    {
        public const string AccessOwnerWorkspace = "Garden.AccessOwnerWorkspace";
        public const string ManagePlants = "Garden.ManagePlants";
    }

    public static class Configuration
    {
        public const string ManageSettings = "Configuration.ManageSettings";
    }

    public static class Logging
    {
        public const string ManageAppLogs = "Logging.ManageAppLogs";
    }

    public static class Localization
    {
        public const string ManageLocalization = "Localization.ManageLocalization";
    }
}

/// <summary>
/// Represents a code-defined permission definition and its default role assignment behavior.
/// </summary>
public sealed record PermissionDefinition(
    string Name,
    string SystemName,
    string Category,
    bool AssignToRegisteredOwnerByDefault = false)
{
    /// <summary>
    /// Converts the definition into a persistent permission entity.
    /// </summary>
    public PermissionRecord ToEntity()
        => new()
        {
            Name = Name,
            SystemName = SystemName,
            Category = Category
        };
}

/// <summary>
/// Provides the application's code-driven permission catalog.
/// </summary>
public static class GropMngPermissionProvider
{
    private static readonly IReadOnlyList<PermissionDefinition> AllPermissions =
    [
        new("Access Admin Area", GropMngPermissions.Security.AccessAdminArea, "Security"),
        new("Manage Owners", GropMngPermissions.Owners.ManageOwners, "Owners"),
        new("Manage Owner Roles", GropMngPermissions.Owners.ManageRoles, "Owners"),
        new("Access Owner Workspace", GropMngPermissions.Garden.AccessOwnerWorkspace, "Garden", AssignToRegisteredOwnerByDefault: true),
        new("Manage Plants", GropMngPermissions.Garden.ManagePlants, "Garden"),
        new("Manage Settings", GropMngPermissions.Configuration.ManageSettings, "Configuration"),
        new("Manage App Logs", GropMngPermissions.Logging.ManageAppLogs, "Logging"),
        new("Manage Localization", GropMngPermissions.Localization.ManageLocalization, "Localization")
    ];

    /// <summary>
    /// Gets all code-defined permissions.
    /// </summary>
    public static IReadOnlyList<PermissionDefinition> GetAllPermissions() => AllPermissions;
}
