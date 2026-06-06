using GropMng.Core.Domain.Garden.Plants;

namespace GropMng.Core.Interfaces.Services.Garden.Plants;

/// <summary>
/// Defines application services for plant photos scoped to a plant instance.
/// </summary>
public interface IPlantPhotoService
{
    /// <summary>
    /// Retrieves all photos for a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="ownerId">The owner identifier used to validate access to the plant instance.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of plant photos.</returns>
    Task<IReadOnlyList<PlantPhoto>> GetPhotosAsync(int plantInstanceId, Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single photo by its identifier.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="photoId">The identifier of the photo to retrieve.</param>
    /// <param name="ownerId">The owner identifier used to validate access.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The matching photo when found; otherwise, <see langword="null" />.</returns>
    Task<PlantPhoto?> GetPhotoByIdAsync(int plantInstanceId, int photoId, Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the main (primary) photo for a plant instance — the one with the lowest DisplayOrder.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the plant instance.</param>
    /// <param name="ownerId">The owner identifier used to validate access.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The primary photo when one exists; otherwise, <see langword="null" />.</returns>
    Task<PlantPhoto?> GetMainPhotoAsync(int plantInstanceId, Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a photo to a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="photo">The photo to create.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The created plant photo.</returns>
    Task<PlantPhoto> CreatePhotoAsync(int plantInstanceId, PlantPhoto photo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a photo that belongs to a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="photo">The photo entity containing the updated values.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The updated plant photo.</returns>
    Task<PlantPhoto> UpdatePhotoAsync(int plantInstanceId, PlantPhoto photo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a photo from a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="photoId">The identifier of the photo to delete.</param>
    /// <param name="ownerId">The owner identifier used to validate access to the plant instance.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    Task DeletePhotoAsync(int plantInstanceId, int photoId, Guid ownerId, CancellationToken cancellationToken = default);
}