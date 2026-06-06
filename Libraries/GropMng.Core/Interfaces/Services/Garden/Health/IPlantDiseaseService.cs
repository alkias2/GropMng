using GropMng.Core.Domain.Garden.Health;

namespace GropMng.Core.Interfaces.Services.Garden.Health;

/// <summary>
/// Defines application services for plant disease records and their photos,
/// scoped to a plant instance.
/// </summary>
public interface IPlantDiseaseService
{
    /// <summary>
    /// Retrieves all disease records for a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="ownerId">The owner identifier used to validate access to the plant instance.</param>
    /// <param name="includePhotos">A value indicating whether disease photos should be loaded for each record.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of plant disease records.</returns>
    Task<IReadOnlyList<PlantDiseaseRecord>> GetRecordsAsync(int plantInstanceId, Guid ownerId, bool includePhotos = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a disease record to a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="record">The disease record to create.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The created disease record.</returns>
    Task<PlantDiseaseRecord> CreateRecordAsync(int plantInstanceId, PlantDiseaseRecord record, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a disease record that belongs to a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="record">The disease record containing the updated values.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The updated disease record.</returns>
    Task<PlantDiseaseRecord> UpdateRecordAsync(int plantInstanceId, PlantDiseaseRecord record, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a disease record from a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="recordId">The identifier of the disease record to delete.</param>
    /// <param name="ownerId">The owner identifier used to validate access to the plant instance.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    Task DeleteRecordAsync(int plantInstanceId, int recordId, Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all disease photos for a disease record that belongs to a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="recordId">The identifier of the parent disease record.</param>
    /// <param name="ownerId">The owner identifier used to validate access to the aggregate.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of disease photos.</returns>
    Task<IReadOnlyList<DiseasePhoto>> GetPhotosAsync(int plantInstanceId, int recordId, Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a disease photo to a disease record that belongs to a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="recordId">The identifier of the parent disease record.</param>
    /// <param name="photo">The disease photo to create.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The created disease photo.</returns>
    Task<DiseasePhoto> CreatePhotoAsync(int plantInstanceId, int recordId, DiseasePhoto photo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a disease photo that belongs to a disease record under a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="recordId">The identifier of the parent disease record.</param>
    /// <param name="photo">The disease photo entity containing the updated values.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The updated disease photo.</returns>
    Task<DiseasePhoto> UpdatePhotoAsync(int plantInstanceId, int recordId, DiseasePhoto photo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a disease photo from a disease record under a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="recordId">The identifier of the parent disease record.</param>
    /// <param name="photoId">The identifier of the disease photo to delete.</param>
    /// <param name="ownerId">The owner identifier used to validate access to the aggregate.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    Task DeletePhotoAsync(int plantInstanceId, int recordId, int photoId, Guid ownerId, CancellationToken cancellationToken = default);
}