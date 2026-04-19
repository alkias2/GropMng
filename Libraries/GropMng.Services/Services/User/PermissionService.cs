using GropMng.Core.Domain.Garden.Owners;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace GropMng.Services.Services.User;

/// <summary>
/// Resolves permission access for owner accounts based on their assigned roles.
/// </summary>
public class PermissionService : IPermissionService
{
    private readonly IRepository<Owner> _ownerRepository;
    private readonly ICurrentOwnerProvider _currentOwnerProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionService"/> class.
    /// </summary>
    public PermissionService(IRepository<Owner> ownerRepository, ICurrentOwnerProvider currentOwnerProvider)
    {
        _ownerRepository = ownerRepository ?? throw new ArgumentNullException(nameof(ownerRepository));
        _currentOwnerProvider = currentOwnerProvider ?? throw new ArgumentNullException(nameof(currentOwnerProvider));
    }

    /// <inheritdoc />
    public async Task<bool> AuthorizeAsync(string permissionSystemName, CancellationToken cancellationToken = default)
    {
        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);
        var owner = await GetOwnerWithPermissionsAsync(ownerId, cancellationToken);

        return owner is not null && await AuthorizeAsync(permissionSystemName, owner, cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> AuthorizeAsync(string permissionSystemName, Owner owner, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(permissionSystemName) || owner is null || !owner.IsActive)
            return Task.FromResult(false);

        var isAuthorized = owner.OwnerRoles
            .Where(role => role.IsActive)
            .SelectMany(role => role.PermissionRecords)
            .Any(permission => string.Equals(permission.SystemName, permissionSystemName, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(isAuthorized);
    }

    private async Task<Owner?> GetOwnerWithPermissionsAsync(Guid ownerId, CancellationToken cancellationToken)
    {
        var owner = await _ownerRepository.FirstOrDefaultAsync(
            entity => entity.OwnerId == ownerId && entity.IsActive,
            cancellationToken: cancellationToken);

        if (owner is null)
            return null;

        if (owner.OwnerRoles.Count != 0 && owner.OwnerRoles.Any(role => role.PermissionRecords.Count != 0))
            return owner;

        var query = _ownerRepository.TableNoTracking
            .Include(entity => entity.OwnerRoles)
                .ThenInclude(role => role.PermissionRecords);

        if (query.Provider is IAsyncQueryProvider)
            return await query.FirstOrDefaultAsync(entity => entity.OwnerId == ownerId && entity.IsActive, cancellationToken);

        return query.FirstOrDefault(entity => entity.OwnerId == ownerId && entity.IsActive);
    }
}
