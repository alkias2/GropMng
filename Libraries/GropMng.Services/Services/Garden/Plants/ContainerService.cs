using GropMng.Core;
using GropMng.Core.Common.Exceptions;
using GropMng.Core.Domain.Garden.Plants;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.Garden.Plants;

namespace GropMng.Services.Services.Garden.Plants;

/// <summary>
/// Provides owner-scoped CRUD operations for container entities.
/// </summary>
public class ContainerService : IContainerService
{
    #region Fields

    private readonly IRepository<Container> _containerRepository;
    private readonly IRepository<PlantInstance> _plantInstanceRepository;

    #endregion

    #region Ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="ContainerService" /> class.
    /// </summary>
    public ContainerService(
        IRepository<Container> containerRepository,
        IRepository<PlantInstance> plantInstanceRepository)
    {
        _containerRepository = containerRepository ?? throw new ArgumentNullException(nameof(containerRepository));
        _plantInstanceRepository = plantInstanceRepository ?? throw new ArgumentNullException(nameof(plantInstanceRepository));
    }

    #endregion

    #region Public

    /// <inheritdoc />
    public Task<IPagedList<Container>> GetContainersAsync(Guid ownerId, int pageIndex = 0, int pageSize = int.MaxValue, CancellationToken cancellationToken = default)
    {
        ValidateOwnerId(ownerId);

        return _containerRepository.GetPagedAsync(
            query => query
                .Where(c => c.OwnerId == ownerId)
                .OrderBy(c => c.ContainerType)
                .ThenBy(c => c.Id),
            pageIndex,
            pageSize,
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public Task<Container?> GetContainerByIdAsync(int containerId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        ValidateOwnerId(ownerId);

        return _containerRepository.FirstOrDefaultAsync(
            c => c.Id == containerId && c.OwnerId == ownerId,
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Container> CreateContainerAsync(Container container, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(container);
        ValidateOwnerId(container.OwnerId);

        await _containerRepository.CreateAsync(container, cancellationToken: cancellationToken);
        return container;
    }

    /// <inheritdoc />
    public async Task<Container> UpdateContainerAsync(Container container, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(container);
        ValidateOwnerId(container.OwnerId);

        await EnsureContainerOwnedAsync(container.Id, container.OwnerId, cancellationToken);
        await _containerRepository.UpdateAsync(container, cancellationToken: cancellationToken);
        return container;
    }

    /// <inheritdoc />
    public async Task DeleteContainerAsync(int containerId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        ValidateOwnerId(ownerId);

        var container = await EnsureContainerOwnedAsync(containerId, ownerId, cancellationToken);

        var references = await _plantInstanceRepository.CountAsync(
            p => p.ContainerId == containerId,
            cancellationToken: cancellationToken);

        if (references > 0)
            throw new DomainException($"Cannot delete container: it is referenced by {references} plant instance(s).");

        await _containerRepository.DeleteAsync(container, cancellationToken: cancellationToken);
    }

    #endregion

    #region Private

    private async Task<Container> EnsureContainerOwnedAsync(int containerId, Guid ownerId, CancellationToken cancellationToken)
    {
        var container = await _containerRepository.FirstOrDefaultAsync(
            c => c.Id == containerId && c.OwnerId == ownerId,
            cancellationToken: cancellationToken);

        if (container is null)
            throw new DomainException($"Container {containerId} was not found for the current owner.");

        return container;
    }

    private static void ValidateOwnerId(Guid ownerId)
    {
        if (ownerId == Guid.Empty)
            throw new ArgumentException("Owner ID must not be empty.", nameof(ownerId));
    }

    #endregion
}
