using GropMng.Core.Domain.Garden;

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
    /// Gets or sets the owner email.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Gets or sets the password hash used for future authentication flows.
    /// </summary>
    public required string PasswordHash { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the owner is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
