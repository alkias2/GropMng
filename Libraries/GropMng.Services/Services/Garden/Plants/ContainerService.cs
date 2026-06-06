using GropMng.Core;
using GropMng.Core.Caching;
using GropMng.Core.Common.Exceptions;
using GropMng.Core.Domain.Garden.Plants;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.Garden.Plants;
using GropMng.Services.Caching.Garden;
using GropMng.Services.Helpers;
using Microsoft.EntityFrameworkCore;

namespace GropMng.Services.Services.Garden.Plants;

/// <summary>
/// Provides owner-scoped CRUD operations for container entities.
/// </summary>
public class ContainerService : IContainerService
{
    #region Fields

    private readonly IRepository<Container> _containerRepository;
    private readonly IGropStaticCacheManager _staticCacheManager;

    #endregion

    #region Ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="ContainerService" /> class.
    /// </summary>
    public ContainerService(
        IRepository<Container> containerRepository,
        IGropStaticCacheManager staticCacheManager)
    {
        _containerRepository = containerRepository ?? throw new ArgumentNullException(nameof(containerRepository));
        _staticCacheManager = staticCacheManager ?? throw new ArgumentNullException(nameof(staticCacheManager));
    }

    #endregion

    #region Public

    /// <inheritdoc />
    public async Task<IPagedList<Container>> GetContainersAsync(Guid ownerId, int pageIndex = 0, int pageSize = int.MaxValue, CancellationToken cancellationToken = default)
    {
        ValidateOwnerId(ownerId);

        var cacheKey = _staticCacheManager.PrepareKey(PlantCacheDefaults.ContainersByOwnerKey, ownerId.ToString("N"));

        return await _staticCacheManager.GetAsync(cacheKey, () =>
            _containerRepository.GetPagedAsync(
                query => query
                    .Where(c => c.OwnerId == ownerId)
                    .OrderBy(c => c.ContainerType)
                    .ThenBy(c => c.Id),
                pageIndex,
                pageSize,
                cancellationToken: cancellationToken));
    }

    /// <inheritdoc />
    public async Task<Container?> GetContainerByIdAsync(int containerId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        ValidateOwnerId(ownerId);

        var cacheKey = _staticCacheManager.PrepareKey(PlantCacheDefaults.ContainerByIdKey, ownerId.ToString("N"), containerId);

        return await _staticCacheManager.GetAsync(cacheKey, async () =>
        {
            var results = await _containerRepository.GetAllAsync(
                query => query
                    .Include(c => c.PlantInstance)
                    .Where(c => c.Id == containerId && c.OwnerId == ownerId),
                cancellationToken: cancellationToken);

            return results.FirstOrDefault();
        });
    }

    /// <inheritdoc />
    public async Task<Container> CreateContainerAsync(Container container, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(container);
        ValidateOwnerId(container.OwnerId);

        AuditableEntityHelper.StampForCreate(container);

        await _containerRepository.CreateAsync(container, cancellationToken: cancellationToken);
        await _staticCacheManager.RemoveByPrefixAsync(PlantCacheDefaults.ContainerPrefix);

        return container;
    }

    /// <inheritdoc />
    public async Task<Container> UpdateContainerAsync(Container container, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(container);
        ValidateOwnerId(container.OwnerId);

        var existingContainer = await EnsureContainerOwnedAsync(container.Id, container.OwnerId, cancellationToken);

        existingContainer.ContainerType = container.ContainerType;
        existingContainer.Material = container.Material?.Trim();
        existingContainer.LengthCm = container.LengthCm;
        existingContainer.WidthCm = container.WidthCm;
        existingContainer.BaseCircumferenceCm = container.BaseCircumferenceCm;
        existingContainer.RimCircumferenceCm = container.RimCircumferenceCm;
        existingContainer.HeightCm = container.HeightCm;
        existingContainer.VolumeL = container.VolumeL;
        existingContainer.Color = container.Color?.Trim();
        existingContainer.HasDrainageHole = container.HasDrainageHole;
        existingContainer.Notes = container.Notes?.Trim();
        AuditableEntityHelper.StampForUpdate(existingContainer);

        await _containerRepository.UpdateAsync(existingContainer, cancellationToken: cancellationToken);
        await _staticCacheManager.RemoveByPrefixAsync(PlantCacheDefaults.ContainerPrefix);

        return existingContainer;
    }

    /// <inheritdoc />
    public async Task DeleteContainerAsync(int containerId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        ValidateOwnerId(ownerId);

        var container = await EnsureContainerOwnedAsync(containerId, ownerId, cancellationToken);

        if (container.PlantInstanceId.HasValue)
            throw new DomainException("Cannot delete container: it is still linked to a plant instance.");

        await _containerRepository.DeleteAsync(container, cancellationToken: cancellationToken);
        await _staticCacheManager.RemoveByPrefixAsync(PlantCacheDefaults.ContainerPrefix);
    }

    #endregion

    #region Private

    private async Task<Container> EnsureContainerOwnedAsync(int containerId, Guid ownerId, CancellationToken cancellationToken)
    {
        var container = await _containerRepository.FirstOrDefaultAsync(
            c => c.Id == containerId && c.OwnerId == ownerId,
            cancellationToken: cancellationToken);

        return container ?? throw new DomainException($"Container {containerId} was not found for the current owner.");
    }

    private static void ValidateOwnerId(Guid ownerId)
    {
        if (ownerId == Guid.Empty)
            throw new DomainException("Owner ID must not be empty.");
    }

    #endregion
}