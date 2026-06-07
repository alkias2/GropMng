using GropMng.Core.Domain.Garden.Owners;

namespace GropMng.Core.Interfaces.Services.User;

/// <summary>
/// Provides owner-account retrieval and maintenance operations for the auth subsystem.
/// </summary>
public interface IOwnerService
{
    /// <summary>
    /// Gets an owner by the public business identifier.
    /// </summary>
    Task<Owner?> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an owner by email address.
    /// </summary>
    Task<Owner?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists changes to an owner account.
    /// </summary>
    Task<Owner> UpdateAsync(Owner owner, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces the role membership for the supplied owner.
    /// </summary>
    Task AssignRolesAsync(Guid ownerId, IReadOnlyCollection<string> roleSystemNames, CancellationToken cancellationToken = default);
}
