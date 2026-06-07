using System.ComponentModel.DataAnnotations;

namespace GropMng.Web.Areas.Admin.Models.Roles;

/// <summary>
/// Editable owner-role model including grouped permission selection.
/// </summary>
public class OwnerRoleEditModel
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Role name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Display(Name = "System name")]
    public string SystemName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public bool IsSystemRole { get; set; }

    public IList<string> SelectedPermissionSystemNames { get; set; } = new List<string>();

    public IList<PermissionGroupModel> PermissionGroups { get; set; } = new List<PermissionGroupModel>();
}

public class PermissionGroupModel
{
    public string Category { get; set; } = string.Empty;

    public IList<PermissionCheckboxModel> Permissions { get; set; } = new List<PermissionCheckboxModel>();
}

public class PermissionCheckboxModel
{
    public string SystemName { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public bool Selected { get; set; }
}
