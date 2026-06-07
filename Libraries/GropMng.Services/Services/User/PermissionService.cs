using GropMng.Core.Caching;
using GropMng.Core.Domain.Garden.Owners;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.User;
using Microsoft.EntityFrameworkCore;

namespace GropMng.Services.Services.User;

/// <summary>
/// Resolves permission access for owner accounts based on their assigned roles.
/// </summary>
public class PermissionService : IPermissionService
{
    internal const string PermissionsByOwnerCachePrefix = "Grop.permissions.byowner.";

    private static readonly GropCacheKey PermissionsByOwnerCacheKey =
        new("Grop.permissions.byowner.v1.{0}", PermissionsByOwnerCachePrefix) { CacheTime = 10 };

    private readonly IRepository<Owner> _ownerRepository;
    private readonly ICurrentOwnerProvider _currentOwnerProvider;
    private readonly IGropStaticCacheManager _staticCacheManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionService"/> class.
    /// </summary>
    public PermissionService(
        IRepository<Owner> ownerRepository,
        ICurrentOwnerProvider currentOwnerProvider,
        IGropStaticCacheManager staticCacheManager)
    {
        _ownerRepository = ownerRepository ?? throw new ArgumentNullException(nameof(ownerRepository));
        _currentOwnerProvider = currentOwnerProvider ?? throw new ArgumentNullException(nameof(currentOwnerProvider));
        _staticCacheManager = staticCacheManager ?? throw new ArgumentNullException(nameof(staticCacheManager));
    }

    /// <inheritdoc />
    public async Task<bool> AuthorizeAsync(string permissionSystemName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(permissionSystemName))
            return false;

        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);
        var permissionSystemNames = await GetActivePermissionSystemNamesAsync(ownerId, cancellationToken);

        return permissionSystemNames.Any(permission => string.Equals(permission, permissionSystemName, StringComparison.OrdinalIgnoreCase));
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

    private Task<string[]> GetActivePermissionSystemNamesAsync(Guid ownerId, CancellationToken cancellationToken)
    {
        var cacheKey = _staticCacheManager.PrepareKey(PermissionsByOwnerCacheKey, ownerId.ToString("N"));
        return _staticCacheManager.GetAsync(cacheKey, () => GetActivePermissionSystemNamesCoreAsync(ownerId, cancellationToken));
    }

    private async Task<string[]> GetActivePermissionSystemNamesCoreAsync(Guid ownerId, CancellationToken cancellationToken)
    {
        var query = _ownerRepository.TableNoTracking
            .Where(entity => entity.OwnerId == ownerId && entity.IsActive)
            .SelectMany(entity => entity.OwnerRoles
                .Where(role => role.IsActive)
                .SelectMany(role => role.PermissionRecords.Select(permission => permission.SystemName)))
            .Distinct();

        if (query is IAsyncEnumerable<string>)
        {
            try
            {
                return await query.ToArrayAsync(cancellationToken);
            }
            catch (InvalidOperationException)
            {
                // Fallback for unit tests that provide non-EF IQueryable providers.
            }
        }

        return query.ToArray();
    }
}
