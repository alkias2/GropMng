using GropMng.Core.Domain.Security;

namespace GropMng.Core.Domain.Garden.Owners;

/// <summary>
/// Represents an assignable role for an owner account.
/// </summary>
public class OwnerRole : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string SystemName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public bool IsSystemRole { get; set; } = true;

    public ICollection<Owner> Owners { get; set; } = new List<Owner>();

    public ICollection<PermissionRecord> PermissionRecords { get; set; } = new List<PermissionRecord>();
}
