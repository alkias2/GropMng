using GropMng.Core.Domain.Garden.Care;
using GropMng.Core.Domain.Garden.Health;
using GropMng.Core.Domain.Garden.Plants;

namespace GropMng.Core.Interfaces.Services.Garden.Plants;

/// <summary>
/// Defines application services for managing plant instances and their supporting entities.
/// </summary>
public interface IPlantInstanceService
{
    /// <summary>
    /// Retrieves a paged list of plant instances for an owner using optional aggregate filters.
    /// </summary>
    /// <param name="ownerId">The owner identifier used to scope the result.</param>
    /// <param name="plantId">An optional plant catalog identifier filter.</param>
    /// <param name="gardenSpotId">An optional garden spot identifier filter.</param>
    /// <param name="locationId">An optional location identifier filter.</param>
    /// <param name="activeOnly">A value indicating whether only active plant instances should be returned.</param>
    /// <param name="pageIndex">The zero-based page index.</param>
    /// <param name="pageSize">The number of items to include in the page.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A paged list of plant instances.</returns>
    Task<IPagedList<PlantInstance>> GetPlantInstancesAsync(Guid ownerId, int? plantId = null, int? gardenSpotId = null, int? locationId = null, bool activeOnly = false, int pageIndex = 0, int pageSize = int.MaxValue, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single plant instance for an owner.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the plant instance to retrieve.</param>
    /// <param name="ownerId">The owner identifier used to validate access to the instance.</param>
    /// <param name="includeDetails">A value indicating whether supporting entities should be loaded together with the plant instance.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The matching plant instance when found; otherwise, <see langword="null" />.</returns>
    Task<PlantInstance?> GetPlantInstanceByIdAsync(int plantInstanceId, Guid ownerId, bool includeDetails = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new plant instance.
    /// </summary>
    /// <param name="plantInstance">The plant instance to create.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The created plant instance.</returns>
    Task<PlantInstance> CreatePlantInstanceAsync(PlantInstance plantInstance, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing plant instance.
    /// </summary>
    /// <param name="plantInstance">The plant instance entity containing the updated values.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The updated plant instance.</returns>
    Task<PlantInstance> UpdatePlantInstanceAsync(PlantInstance plantInstance, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an existing plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the plant instance to delete.</param>
    /// <param name="ownerId">The owner identifier used to validate access to the instance.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    Task DeletePlantInstanceAsync(int plantInstanceId, Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all containers available to a specific owner.
    /// </summary>
    /// <param name="ownerId">The owner identifier used to scope the result.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of containers.</returns>
    Task<IReadOnlyList<Container>> GetContainersAsync(Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new container that can be assigned through the plant instance administration workflow.
    /// </summary>
    /// <param name="container">The container to create.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The created container.</returns>
    Task<Container> CreateContainerAsync(Container container, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing container.
    /// </summary>
    /// <param name="container">The container entity containing the updated values.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The updated container.</returns>
    Task<Container> UpdateContainerAsync(Container container, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an existing container.
    /// </summary>
    /// <param name="containerId">The identifier of the container to delete.</param>
    /// <param name="ownerId">The owner identifier used to validate access to the container.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    Task DeleteContainerAsync(int containerId, Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all soil mixes available for administration workflows.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of soil mixes.</returns>
    Task<IReadOnlyList<SoilMix>> GetSoilMixesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new soil mix.
    /// </summary>
    /// <param name="soilMix">The soil mix to create.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The created soil mix.</returns>
    Task<SoilMix> CreateSoilMixAsync(SoilMix soilMix, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing soil mix.
    /// </summary>
    /// <param name="soilMix">The soil mix entity containing the updated values.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The updated soil mix.</returns>
    Task<SoilMix> UpdateSoilMixAsync(SoilMix soilMix, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an existing soil mix.
    /// </summary>
    /// <param name="soilMixId">The identifier of the soil mix to delete.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    Task DeleteSoilMixAsync(int soilMixId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all watering schedules for a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="ownerId">The owner identifier used to validate access to the plant instance.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of watering schedules.</returns>
    Task<IReadOnlyList<WateringSchedule>> GetWateringSchedulesAsync(int plantInstanceId, Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a watering schedule to a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="schedule">The watering schedule to create.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The created watering schedule.</returns>
    Task<WateringSchedule> AddWateringScheduleAsync(int plantInstanceId, WateringSchedule schedule, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a watering schedule that belongs to a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="schedule">The watering schedule containing the updated values.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The updated watering schedule.</returns>
    Task<WateringSchedule> UpdateWateringScheduleAsync(int plantInstanceId, WateringSchedule schedule, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a watering schedule from a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="scheduleId">The identifier of the watering schedule to delete.</param>
    /// <param name="ownerId">The owner identifier used to validate access to the plant instance.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    Task DeleteWateringScheduleAsync(int plantInstanceId, int scheduleId, Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paged list of watering log entries for a plant instance. Logs are read-only from this service.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="ownerId">The owner identifier used to validate access.</param>
    /// <param name="pageIndex">The zero-based page index.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A paged list of watering log entries ordered by date descending.</returns>
    Task<IPagedList<WateringLog>> GetWateringLogsAsync(int plantInstanceId, Guid ownerId, int pageIndex = 0, int pageSize = 5, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a watering log entry from a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="logId">The identifier of the watering log entry to delete.</param>
    /// <param name="ownerId">The owner identifier used to validate access.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    Task DeleteWateringLogAsync(int plantInstanceId, int logId, Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all fertilizing schedules for a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="ownerId">The owner identifier used to validate access to the plant instance.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of fertilizing schedules.</returns>
    Task<IReadOnlyList<FertilizingSchedule>> GetFertilizingSchedulesAsync(int plantInstanceId, Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a fertilizing schedule to a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="schedule">The fertilizing schedule to create.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The created fertilizing schedule.</returns>
    Task<FertilizingSchedule> AddFertilizingScheduleAsync(int plantInstanceId, FertilizingSchedule schedule, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a fertilizing schedule that belongs to a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="schedule">The fertilizing schedule containing the updated values.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The updated fertilizing schedule.</returns>
    Task<FertilizingSchedule> UpdateFertilizingScheduleAsync(int plantInstanceId, FertilizingSchedule schedule, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a fertilizing schedule from a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="scheduleId">The identifier of the fertilizing schedule to delete.</param>
    /// <param name="ownerId">The owner identifier used to validate access to the plant instance.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    Task DeleteFertilizingScheduleAsync(int plantInstanceId, int scheduleId, Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paged list of fertilizing log entries for a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="ownerId">The owner identifier used to validate access.</param>
    /// <param name="pageIndex">The zero-based page index.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A paged list of fertilizing log entries ordered by date descending.</returns>
    Task<IPagedList<FertilizingLog>> GetFertilizingLogsAsync(int plantInstanceId, Guid ownerId, int pageIndex = 0, int pageSize = 5, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a fertilizing log entry from a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="logId">The identifier of the fertilizing log entry to delete.</param>
    /// <param name="ownerId">The owner identifier used to validate access.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    Task DeleteFertilizingLogAsync(int plantInstanceId, int logId, Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a repotting event and updates the current container/soil mix of the plant instance atomically.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="repottingLog">The repotting payload to persist.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The created repotting log entry.</returns>
    Task<RepottingLog> RepotPlantAsync(int plantInstanceId, RepottingLog repottingLog, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves repotting logs for a plant instance ordered by repot date descending.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="ownerId">The owner identifier used to validate access.</param>
    /// <param name="pageIndex">The zero-based page index.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A paged list of repotting logs.</returns>
    Task<IPagedList<RepottingLog>> GetRepottingLogsAsync(int plantInstanceId, Guid ownerId, int pageIndex = 0, int pageSize = 5, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a repotting log entry that belongs to a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="repottingLog">The repotting log with updated values.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The updated repotting log entry.</returns>
    Task<RepottingLog> UpdateRepottingLogAsync(int plantInstanceId, RepottingLog repottingLog, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a repotting log entry from a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="logId">The identifier of the repotting log entry to delete.</param>
    /// <param name="ownerId">The owner identifier used to validate access.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    Task DeleteRepottingLogAsync(int plantInstanceId, int logId, Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all photos for a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="ownerId">The owner identifier used to validate access to the plant instance.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of plant photos.</returns>
    Task<IReadOnlyList<PlantPhoto>> GetPlantPhotosAsync(int plantInstanceId, Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a photo to a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="photo">The photo to create.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The created plant photo.</returns>
    Task<PlantPhoto> AddPlantPhotoAsync(int plantInstanceId, PlantPhoto photo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a photo that belongs to a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="photo">The photo entity containing the updated values.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The updated plant photo.</returns>
    Task<PlantPhoto> UpdatePlantPhotoAsync(int plantInstanceId, PlantPhoto photo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a photo from a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="photoId">The identifier of the photo to delete.</param>
    /// <param name="ownerId">The owner identifier used to validate access to the plant instance.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    Task DeletePlantPhotoAsync(int plantInstanceId, int photoId, Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single photo by its identifier.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="photoId">The identifier of the photo to retrieve.</param>
    /// <param name="ownerId">The owner identifier used to validate access.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The matching photo when found; otherwise, <see langword="null" />.</returns>
    Task<PlantPhoto?> GetPlantPhotoByIdAsync(int plantInstanceId, int photoId, Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the main (primary) photo for a plant instance — the one with the lowest DisplayOrder.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the plant instance.</param>
    /// <param name="ownerId">The owner identifier used to validate access.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The primary photo when one exists; otherwise, <see langword="null" />.</returns>
    Task<PlantPhoto?> GetMainPlantPhotoAsync(int plantInstanceId, Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all notes for a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="ownerId">The owner identifier used to validate access to the plant instance.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of plant notes.</returns>
    Task<IReadOnlyList<PlantNote>> GetPlantNotesAsync(int plantInstanceId, Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a note to a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="note">The note to create.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The created plant note.</returns>
    Task<PlantNote> AddPlantNoteAsync(int plantInstanceId, PlantNote note, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a note that belongs to a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="note">The note entity containing the updated values.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The updated plant note.</returns>
    Task<PlantNote> UpdatePlantNoteAsync(int plantInstanceId, PlantNote note, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a note from a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="noteId">The identifier of the note to delete.</param>
    /// <param name="ownerId">The owner identifier used to validate access to the plant instance.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    Task DeletePlantNoteAsync(int plantInstanceId, int noteId, Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all disease records for a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="ownerId">The owner identifier used to validate access to the plant instance.</param>
    /// <param name="includePhotos">A value indicating whether disease photos should be loaded for each record.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of plant disease records.</returns>
    Task<IReadOnlyList<PlantDiseaseRecord>> GetDiseaseRecordsAsync(int plantInstanceId, Guid ownerId, bool includePhotos = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a disease record to a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="record">The disease record to create.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The created disease record.</returns>
    Task<PlantDiseaseRecord> AddDiseaseRecordAsync(int plantInstanceId, PlantDiseaseRecord record, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a disease record that belongs to a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="record">The disease record containing the updated values.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The updated disease record.</returns>
    Task<PlantDiseaseRecord> UpdateDiseaseRecordAsync(int plantInstanceId, PlantDiseaseRecord record, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a disease record from a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="recordId">The identifier of the disease record to delete.</param>
    /// <param name="ownerId">The owner identifier used to validate access to the plant instance.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    Task DeleteDiseaseRecordAsync(int plantInstanceId, int recordId, Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all disease photos for a disease record that belongs to a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="recordId">The identifier of the parent disease record.</param>
    /// <param name="ownerId">The owner identifier used to validate access to the aggregate.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of disease photos.</returns>
    Task<IReadOnlyList<DiseasePhoto>> GetDiseasePhotosAsync(int plantInstanceId, int recordId, Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a disease photo to a disease record that belongs to a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="recordId">The identifier of the parent disease record.</param>
    /// <param name="photo">The disease photo to create.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The created disease photo.</returns>
    Task<DiseasePhoto> AddDiseasePhotoAsync(int plantInstanceId, int recordId, DiseasePhoto photo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a disease photo that belongs to a disease record under a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="recordId">The identifier of the parent disease record.</param>
    /// <param name="photo">The disease photo entity containing the updated values.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The updated disease photo.</returns>
    Task<DiseasePhoto> UpdateDiseasePhotoAsync(int plantInstanceId, int recordId, DiseasePhoto photo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a disease photo from a disease record under a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="recordId">The identifier of the parent disease record.</param>
    /// <param name="photoId">The identifier of the disease photo to delete.</param>
    /// <param name="ownerId">The owner identifier used to validate access to the aggregate.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    Task DeleteDiseasePhotoAsync(int plantInstanceId, int recordId, int photoId, Guid ownerId, CancellationToken cancellationToken = default);
}
