using GropMng.Core.Domain.Garden.Owners;

namespace GropMng.Core.Domain.Security;

/// <summary>
/// Represents a code-defined permission that can be assigned to owner roles.
/// </summary>
public class PermissionRecord : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string SystemName { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public ICollection<OwnerRole> OwnerRoles { get; set; } = new List<OwnerRole>();
}
