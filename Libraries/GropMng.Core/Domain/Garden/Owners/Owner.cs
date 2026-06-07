using GropMng.Core.Domain.Garden;
using GropMng.Core.Domain.Garden.Enums;

namespace GropMng.Core.Domain.Garden.Owners;

/// <summary>
/// Represents an application owner account used to scope garden data.
/// </summary>
public class Owner : AuditableEntity
{
    /// <summary>
    /// Gets or sets the external business identifier for the owner.
    /// </summary>
    public Guid OwnerId { get; set; }

    /// <summary>
    /// Gets or sets the owner first name.
    /// </summary>
    public required string FirstName { get; set; }

    /// <summary>
    /// Gets or sets the owner last name.
    /// </summary>
    public required string LastName { get; set; }

    /// <summary>
    /// Gets or sets the display name shown in the UI.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the owner email.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Gets or sets the password hash used for the current baseline login flow.
    /// </summary>
    public required string PasswordHash { get; set; }

    /// <summary>
    /// Gets or sets the lifecycle status of the owner account.
    /// </summary>
    public OwnerAccountStatus Status { get; set; } = OwnerAccountStatus.Active;

    /// <summary>
    /// Gets or sets a value indicating whether the owner email has been confirmed.
    /// </summary>
    public bool IsEmailConfirmed { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the owner is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the roles assigned to the owner.
    /// </summary>
    public ICollection<OwnerRole> OwnerRoles { get; set; } = new List<OwnerRole>();

    /// <summary>
    /// Gets or sets the password history records for the owner.
    /// </summary>
    public ICollection<OwnerPassword> Passwords { get; set; } = new List<OwnerPassword>();
}
