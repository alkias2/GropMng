using System.Security.Claims;
using GropMng.Core.Common.Exceptions;
using GropMng.Core.Caching;
using GropMng.Core.Domain.Garden.Owners;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.User;
using Microsoft.AspNetCore.Http;

namespace GropMng.Services.Services.User;

/// <summary>
/// Resolves the current owner context from the authenticated HTTP user, with a seeded fallback for bootstrap scenarios.
/// </summary>
public class CurrentOwnerProvider : ICurrentOwnerProvider
{
    private const string CurrentOwnerCachePrefix = "Grop.current-owner.";
    private static readonly GropCacheKey FallbackCurrentOwnerCacheKey = new("Grop.current-owner.id.fallback.v1", CurrentOwnerCachePrefix)
    {
        CacheTime = 15
    };

    /// <summary>
    /// The custom claim type that stores the authenticated owner business identifier.
    /// </summary>
    public const string OwnerIdClaimType = "grop.owner-id";

    private readonly IRepository<Owner> _ownerRepository;
    private readonly IGropStaticCacheManager _staticCacheManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrentOwnerProvider"/> class.
    /// </summary>
    /// <param name="ownerRepository">The owner repository.</param>
    /// <param name="memoryCache">The memory cache.</param>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    public CurrentOwnerProvider(
        IRepository<Owner> ownerRepository,
        IGropStaticCacheManager staticCacheManager,
        IHttpContextAccessor httpContextAccessor)
    {
        _ownerRepository = ownerRepository ?? throw new ArgumentNullException(nameof(ownerRepository));
        _staticCacheManager = staticCacheManager ?? throw new ArgumentNullException(nameof(staticCacheManager));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <inheritdoc />
    public async Task<Guid> GetCurrentOwnerIdAsync(CancellationToken cancellationToken = default)
    {
        var authenticatedOwnerId = ResolveAuthenticatedOwnerId();
        if (authenticatedOwnerId != Guid.Empty)
            return authenticatedOwnerId;

        return await _staticCacheManager.GetAsync(
            FallbackCurrentOwnerCacheKey,
            async () =>
            {
                var owner = await _ownerRepository.FirstOrDefaultAsync(
                    entity => entity.IsActive,
                    cancellationToken: cancellationToken);

                if (owner is null)
                    throw new DomainException("No active owner was found.");

                return owner.OwnerId;
            });
    }

    private Guid ResolveAuthenticatedOwnerId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
            return Guid.Empty;

        var ownerIdValue = user.FindFirstValue(OwnerIdClaimType)
            ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(ownerIdValue, out var ownerId)
            ? ownerId
            : Guid.Empty;
    }
}
