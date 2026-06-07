using GropMng.Core.Domain.Garden.Care;

namespace GropMng.Core.Interfaces.Services.Garden.Care;

/// <summary>
/// Defines application services for watering schedules and logs scoped to a plant instance.
/// </summary>
public interface IWateringService
{
    /// <summary>
    /// Retrieves all watering schedules for a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="ownerId">The owner identifier used to validate access to the plant instance.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of watering schedules.</returns>
    Task<IReadOnlyList<WateringSchedule>> GetSchedulesAsync(int plantInstanceId, Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new watering schedule for a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="schedule">The watering schedule to create.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The created watering schedule.</returns>
    Task<WateringSchedule> CreateScheduleAsync(int plantInstanceId, WateringSchedule schedule, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing watering schedule that belongs to a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="schedule">The watering schedule containing the updated values.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The updated watering schedule.</returns>
    Task<WateringSchedule> UpdateScheduleAsync(int plantInstanceId, WateringSchedule schedule, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a watering schedule from a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="scheduleId">The identifier of the watering schedule to delete.</param>
    /// <param name="ownerId">The owner identifier used to validate access to the plant instance.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    Task DeleteScheduleAsync(int plantInstanceId, int scheduleId, Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paged list of watering log entries for a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="ownerId">The owner identifier used to validate access.</param>
    /// <param name="pageIndex">The zero-based page index.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A paged list of watering log entries ordered by date descending.</returns>
    Task<IPagedList<WateringLog>> GetLogsAsync(int plantInstanceId, Guid ownerId, int pageIndex = 0, int pageSize = 5, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a watering log entry from a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="logId">The identifier of the watering log entry to delete.</param>
    /// <param name="ownerId">The owner identifier used to validate access.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    Task DeleteLogAsync(int plantInstanceId, int logId, Guid ownerId, CancellationToken cancellationToken = default);
}