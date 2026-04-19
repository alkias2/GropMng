using System.Security.Claims;
using GropMng.Core.Common.Exceptions;
using GropMng.Core.Domain.Garden.Owners;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.User;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace GropMng.Services.Services.User;

/// <summary>
/// Resolves the current owner context from the authenticated HTTP user, with a seeded fallback for bootstrap scenarios.
/// </summary>
public class CurrentOwnerProvider : ICurrentOwnerProvider
{
    private const string FallbackCurrentOwnerCacheKey = "grop.current-owner-id:fallback";

    /// <summary>
    /// The custom claim type that stores the authenticated owner business identifier.
    /// </summary>
    public const string OwnerIdClaimType = "grop.owner-id";

    private readonly IRepository<Owner> _ownerRepository;
    private readonly IMemoryCache _memoryCache;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrentOwnerProvider"/> class.
    /// </summary>
    /// <param name="ownerRepository">The owner repository.</param>
    /// <param name="memoryCache">The memory cache.</param>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    public CurrentOwnerProvider(
        IRepository<Owner> ownerRepository,
        IMemoryCache memoryCache,
        IHttpContextAccessor httpContextAccessor)
    {
        _ownerRepository = ownerRepository ?? throw new ArgumentNullException(nameof(ownerRepository));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <inheritdoc />
    public async Task<Guid> GetCurrentOwnerIdAsync(CancellationToken cancellationToken = default)
    {
        var authenticatedOwnerId = ResolveAuthenticatedOwnerId();
        if (authenticatedOwnerId != Guid.Empty)
            return authenticatedOwnerId;

        if (_memoryCache.TryGetValue(FallbackCurrentOwnerCacheKey, out Guid ownerId) && ownerId != Guid.Empty)
            return ownerId;

        var owner = await _ownerRepository.FirstOrDefaultAsync(
            entity => entity.IsActive,
            cancellationToken: cancellationToken);

        if (owner is null)
            throw new DomainException("No active owner was found.");

        _memoryCache.Set(FallbackCurrentOwnerCacheKey, owner.OwnerId, TimeSpan.FromMinutes(15));
        return owner.OwnerId;
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
