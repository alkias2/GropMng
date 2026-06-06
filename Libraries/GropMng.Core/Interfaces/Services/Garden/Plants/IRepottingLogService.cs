using GropMng.Core.Domain.Garden.Care;

namespace GropMng.Core.Interfaces.Services.Garden.Plants;

/// <summary>
/// Defines application services for repotting log entries scoped to a plant instance.
/// RepotPlant (the aggregate-root operation) remains in IPlantInstanceService.
/// </summary>
public interface IRepottingLogService
{
    /// <summary>
    /// Retrieves repotting logs for a plant instance ordered by repot date descending.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="ownerId">The owner identifier used to validate access.</param>
    /// <param name="pageIndex">The zero-based page index.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A paged list of repotting logs.</returns>
    Task<IPagedList<RepottingLog>> GetLogsAsync(int plantInstanceId, Guid ownerId, int pageIndex = 0, int pageSize = 20, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a repotting log entry that belongs to a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="repottingLog">The repotting log with updated values.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The updated repotting log entry.</returns>
    Task<RepottingLog> UpdateLogAsync(int plantInstanceId, RepottingLog repottingLog, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a repotting log entry from a plant instance.
    /// </summary>
    /// <param name="plantInstanceId">The identifier of the parent plant instance.</param>
    /// <param name="logId">The identifier of the repotting log entry to delete.</param>
    /// <param name="ownerId">The owner identifier used to validate access.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    Task DeleteLogAsync(int plantInstanceId, int logId, Guid ownerId, CancellationToken cancellationToken = default);
}