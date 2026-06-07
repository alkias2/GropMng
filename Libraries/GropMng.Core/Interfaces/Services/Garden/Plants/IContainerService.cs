using GropMng.Core;
using GropMng.Core.Domain.Garden.Plants;

namespace GropMng.Core.Interfaces.Services.Garden.Plants;

/// <summary>
/// Provides CRUD operations for owner-scoped container entities.
/// </summary>
public interface IContainerService
{
    /// <summary>Gets a paged list of containers owned by the specified owner.</summary>
    Task<IPagedList<Container>> GetContainersAsync(Guid ownerId, int pageIndex = 0, int pageSize = int.MaxValue, CancellationToken cancellationToken = default);

    /// <summary>Returns a single container by id, verified against the owner.</summary>
    Task<Container?> GetContainerByIdAsync(int containerId, Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>Creates a new container.</summary>
    Task<Container> CreateContainerAsync(Container container, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing container.</summary>
    Task<Container> UpdateContainerAsync(Container container, CancellationToken cancellationToken = default);

    /// <summary>Deletes a container, provided it is not referenced by any plant instance.</summary>
    Task DeleteContainerAsync(int containerId, Guid ownerId, CancellationToken cancellationToken = default);
}
