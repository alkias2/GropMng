using GropMng.Core.Domain.Garden.Care;

namespace GropMng.Core.Interfaces.Services.Garden.Care;

/// <summary>
/// Defines application services for fertilizing schedules and logs scoped to a plant instance.
/// </summary>
public interface IFertilizingService
{
    /// <summary>
    /// Retrieves all fertilizing schedules for a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="ownerId">The owner identifier used to validate access to the plant instance.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of fertilizing schedules.</returns>
    Task<IReadOnlyList<FertilizingSchedule>> GetSchedulesAsync(int plantInstanceId, Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new fertilizing schedule for a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="schedule">The fertilizing schedule to create.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The created fertilizing schedule.</returns>
    Task<FertilizingSchedule> CreateScheduleAsync(int plantInstanceId, FertilizingSchedule schedule, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing fertilizing schedule that belongs to a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="schedule">The fertilizing schedule containing the updated values.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The updated fertilizing schedule.</returns>
    Task<FertilizingSchedule> UpdateScheduleAsync(int plantInstanceId, FertilizingSchedule schedule, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a fertilizing schedule from a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="scheduleId">The identifier of the fertilizing schedule to delete.</param>
    /// <param name="ownerId">The owner identifier used to validate access to the plant instance.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    Task DeleteScheduleAsync(int plantInstanceId, int scheduleId, Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paged list of fertilizing log entries for a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="ownerId">The owner identifier used to validate access.</param>
    /// <param name="pageIndex">The zero-based page index.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A paged list of fertilizing log entries ordered by date descending.</returns>
    Task<IPagedList<FertilizingLog>> GetLogsAsync(int plantInstanceId, Guid ownerId, int pageIndex = 0, int pageSize = 5, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a fertilizing log entry from a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="logId">The identifier of the fertilizing log entry to delete.</param>
    /// <param name="ownerId">The owner identifier used to validate access.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    Task DeleteLogAsync(int plantInstanceId, int logId, Guid ownerId, CancellationToken cancellationToken = default);
}