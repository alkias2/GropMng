using GropMng.Core.Domain.Garden.Locations;

namespace GropMng.Core.Interfaces.Services.Garden.Locations;

/// <summary>
/// Defines application services for managing locations and their supporting garden spots.
/// </summary>
public interface ILocationService
{
    /// <summary>
    /// Retrieves a paged list of locations that belong to a specific owner.
    /// </summary>
    /// <param name="ownerId">The owner identifier used to scope the result.</param>
    /// <param name="pageIndex">The zero-based page index.</param>
    /// <param name="pageSize">The number of items to include in the page.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A paged list of locations for the specified owner.</returns>
    Task<IPagedList<Location>> GetLocationsAsync(Guid ownerId, int pageIndex = 0, int pageSize = int.MaxValue, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single location by identifier for a specific owner.
    /// </summary>
    /// <param name="locationId">The identifier of the location to retrieve.</param>
    /// <param name="ownerId">The owner identifier used to validate access to the location.</param>
    /// <param name="includeGardenSpots">A value indicating whether the related garden spots should be loaded.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The matching location when found; otherwise, <see langword="null" />.</returns>
    Task<Location?> GetLocationByIdAsync(int locationId, Guid ownerId, bool includeGardenSpots = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new location.
    /// </summary>
    /// <param name="location">The location to create.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The created location entity.</returns>
    Task<Location> CreateLocationAsync(Location location, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing location.
    /// </summary>
    /// <param name="location">The location entity containing the updated values.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The updated location entity.</returns>
    Task<Location> UpdateLocationAsync(Location location, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an existing location.
    /// </summary>
    /// <param name="locationId">The identifier of the location to delete.</param>
    /// <param name="ownerId">The owner identifier used to validate access to the location.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    Task DeleteLocationAsync(int locationId, Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all garden spots associated with a location.
    /// </summary>
    /// <param name="locationId">The identifier of the parent location.</param>
    /// <param name="ownerId">The owner identifier used to validate access to the location.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of garden spots belonging to the location.</returns>
    Task<IReadOnlyList<GardenSpot>> GetGardenSpotsAsync(int locationId, Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new garden spot under a location.
    /// </summary>
    /// <param name="locationId">The identifier of the parent location.</param>
    /// <param name="gardenSpot">The garden spot to create.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The created garden spot entity.</returns>
    Task<GardenSpot> AddGardenSpotAsync(int locationId, GardenSpot gardenSpot, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing garden spot that belongs to a location.
    /// </summary>
    /// <param name="locationId">The identifier of the parent location.</param>
    /// <param name="gardenSpot">The garden spot entity containing the updated values.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The updated garden spot entity.</returns>
    Task<GardenSpot> UpdateGardenSpotAsync(int locationId, GardenSpot gardenSpot, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a garden spot that belongs to a location.
    /// </summary>
    /// <param name="locationId">The identifier of the parent location.</param>
    /// <param name="gardenSpotId">The identifier of the garden spot to delete.</param>
    /// <param name="ownerId">The owner identifier used to validate access to the location and garden spot.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    Task DeleteGardenSpotAsync(int locationId, int gardenSpotId, Guid ownerId, CancellationToken cancellationToken = default);
}
