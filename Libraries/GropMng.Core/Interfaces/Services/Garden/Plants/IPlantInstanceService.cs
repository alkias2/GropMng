using GropMng.Core.Domain.Garden.Care;
using GropMng.Core.Domain.Garden.Plants;

namespace GropMng.Core.Interfaces.Services.Garden.Plants;

/// <summary>
/// Defines application services for managing plant instances (aggregate root).
/// </summary>
public interface IPlantInstanceService
{
    /// <summary>
    /// Retrieves a paged list of plant instances for an owner using optional aggregate filters.
    /// </summary>
    Task<IPagedList<PlantInstance>> GetPlantInstancesAsync(Guid ownerId, int? plantId = null, int? gardenSpotId = null, int? locationId = null, bool activeOnly = false, int pageIndex = 0, int pageSize = int.MaxValue, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single plant instance for an owner.
    /// </summary>
    Task<PlantInstance?> GetPlantInstanceByIdAsync(int plantInstanceId, Guid ownerId, bool includeDetails = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new plant instance.
    /// </summary>
    Task<PlantInstance> CreatePlantInstanceAsync(PlantInstance plantInstance, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing plant instance.
    /// </summary>
    Task<PlantInstance> UpdatePlantInstanceAsync(PlantInstance plantInstance, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an existing plant instance.
    /// </summary>
    Task DeletePlantInstanceAsync(int plantInstanceId, Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a repotting event and updates the current container/soil mix of the plant instance atomically.
    /// This is an aggregate-root operation.
    /// </summary>
    Task<RepottingLog> RepotPlantAsync(int plantInstanceId, RepottingLog repottingLog, CancellationToken cancellationToken = default);
}