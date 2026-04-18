using GropMng.Core.Common.Exceptions;
using GropMng.Core.Domain.Garden.Owners;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.User;
using Microsoft.Extensions.Caching.Memory;

namespace GropMng.Services.Services.User;

/// <summary>
/// Resolves the current owner context using the first active seeded owner.
/// </summary>
public class CurrentOwnerProvider : ICurrentOwnerProvider
{
    private const string CurrentOwnerCacheKey = "grop.current-owner-id";

    private readonly IRepository<Owner> _ownerRepository;
    private readonly IMemoryCache _memoryCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrentOwnerProvider"/> class.
    /// </summary>
    /// <param name="ownerRepository">The owner repository.</param>
    /// <param name="memoryCache">The memory cache.</param>
    public CurrentOwnerProvider(IRepository<Owner> ownerRepository, IMemoryCache memoryCache)
    {
        _ownerRepository = ownerRepository ?? throw new ArgumentNullException(nameof(ownerRepository));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    }

    /// <inheritdoc />
    public async Task<Guid> GetCurrentOwnerIdAsync(CancellationToken cancellationToken = default)
    {
        if (_memoryCache.TryGetValue(CurrentOwnerCacheKey, out Guid ownerId) && ownerId != Guid.Empty)
            return ownerId;

        var owner = await _ownerRepository.FirstOrDefaultAsync(
            entity => entity.IsActive,
            cancellationToken: cancellationToken);

        if (owner is null)
            throw new DomainException("No active owner was found.");

        _memoryCache.Set(CurrentOwnerCacheKey, owner.OwnerId, TimeSpan.FromMinutes(15));
        return owner.OwnerId;
    }
}
