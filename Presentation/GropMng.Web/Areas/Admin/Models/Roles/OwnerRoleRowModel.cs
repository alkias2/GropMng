namespace GropMng.Web.Areas.Admin.Models.Roles;

/// <summary>
/// Represents a single owner role row in the admin grid.
/// </summary>
public class OwnerRoleRowModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string SystemName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string PermissionsSummary { get; set; } = string.Empty;

    public int PermissionCount { get; set; }

    public bool IsActive { get; set; }

    public bool IsSystemRole { get; set; }
}
