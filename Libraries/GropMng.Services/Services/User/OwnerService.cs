using GropMng.Core.Caching;
using GropMng.Core.Common.Exceptions;
using GropMng.Core.Domain.Garden.Owners;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.User;

namespace GropMng.Services.Services.User;

/// <summary>
/// Provides owner-account retrieval and maintenance operations.
/// </summary>
public class OwnerService : IOwnerService
{
    private readonly IRepository<Owner> _ownerRepository;
    private readonly IRepository<OwnerRole> _ownerRoleRepository;
    private readonly IGropStaticCacheManager _staticCacheManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="OwnerService"/> class.
    /// </summary>
    public OwnerService(
        IRepository<Owner> ownerRepository,
        IRepository<OwnerRole> ownerRoleRepository,
        IGropStaticCacheManager staticCacheManager)
    {
        _ownerRepository = ownerRepository ?? throw new ArgumentNullException(nameof(ownerRepository));
        _ownerRoleRepository = ownerRoleRepository ?? throw new ArgumentNullException(nameof(ownerRoleRepository));
        _staticCacheManager = staticCacheManager ?? throw new ArgumentNullException(nameof(staticCacheManager));
    }

    /// <inheritdoc />
    public async Task<Owner?> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await _ownerRepository.FirstOrDefaultAsync(
            entity => entity.OwnerId == ownerId,
            asNoTracking: true,
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Owner?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var normalizedEmail = email.Trim();
        return await _ownerRepository.FirstOrDefaultAsync(
            entity => entity.Email == normalizedEmail,
            asNoTracking: true,
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Owner> UpdateAsync(Owner owner, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(owner);

        owner.UpdatedAtUtc = DateTime.UtcNow;
        return await _ownerRepository.UpdateAsync(owner, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task AssignRolesAsync(Guid ownerId, IReadOnlyCollection<string> roleSystemNames, CancellationToken cancellationToken = default)
    {
        var owner = await _ownerRepository.FirstOrDefaultAsync(
            entity => entity.OwnerId == ownerId,
            asNoTracking: false,
            cancellationToken: cancellationToken);

        if (owner is null)
            throw new DomainException("Owner was not found.");

        var requestedNames = roleSystemNames
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Select(name => name.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var roles = requestedNames.Length == 0
            ? Array.Empty<OwnerRole>()
            : (await _ownerRoleRepository.FindAsync(
                entity => requestedNames.Contains(entity.SystemName) && entity.IsActive,
                cancellationToken: cancellationToken)).ToArray();

        owner.OwnerRoles = roles;
        owner.UpdatedAtUtc = DateTime.UtcNow;

        await _ownerRepository.UpdateAsync(owner, cancellationToken: cancellationToken);

        // Invalidate security-context and permission caches so the next login/check reflects the new roles.
        await _staticCacheManager.RemoveByPrefixAsync(OwnerAuthenticationService.OwnerSecurityContextByIdCachePrefix);
        await _staticCacheManager.RemoveByPrefixAsync(OwnerAuthenticationService.OwnerSecurityContextByEmailCachePrefix);
        await _staticCacheManager.RemoveByPrefixAsync(PermissionService.PermissionsByOwnerCachePrefix);
    }
}
