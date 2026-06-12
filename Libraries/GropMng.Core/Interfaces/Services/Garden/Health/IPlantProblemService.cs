using GropMng.Core.Domain.Garden.Health;

namespace GropMng.Core.Interfaces.Services.Garden.Health;

/// <summary>
/// Provides operations for managing plant problem records and their treatment schedules.
/// </summary>
public interface IPlantProblemService
{
    /// <summary>
    /// Retrieves a single problem record by its identifier, scoped to the specified owner.
    /// </summary>
    Task<PlantProblemRecord> GetByIdAsync(int id, Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all non-deleted problem records for a plant instance, scoped to the specified owner.
    /// </summary>
    Task<List<PlantProblemRecord>> GetByPlantInstanceAsync(int plantInstanceId, Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new problem record. If <see cref="PlantProblemRecord.NotifyAdmin"/> is <c>true</c>
    /// and no <see cref="PlantProblemRecord.DiseaseKnowledgeId"/> is set, an <see cref="AdminNotification"/> is created.
    /// </summary>
    Task<PlantProblemRecord> CreateAsync(PlantProblemRecord record, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing problem record. Auto-sets <see cref="PlantProblemRecord.ResolvedDate"/>
    /// when <see cref="PlantProblemRecord.ProblemStatus"/> transitions to or from <c>Resolved</c>.
    /// </summary>
    Task<PlantProblemRecord> UpdateAsync(PlantProblemRecord record, Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft-deletes a problem record and all its child schedules.
    /// </summary>
    Task DeleteAsync(int id, Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all active upcoming treatment schedules for the specified owner.
    /// Schedules with <see cref="PlantProblemSchedule.ScheduleStatus"/> other than <c>Active</c> are excluded.
    /// </summary>
    Task<List<PlantProblemSchedule>> GetUpcomingSchedulesAsync(Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a treatment schedule to an existing problem record.
    /// </summary>
    Task<PlantProblemSchedule> AddScheduleAsync(PlantProblemSchedule schedule, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing treatment schedule.
    /// </summary>
    Task<PlantProblemSchedule> UpdateScheduleAsync(PlantProblemSchedule schedule, Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft-deletes a treatment schedule.
    /// </summary>
    Task DeleteScheduleAsync(int id, Guid ownerId, CancellationToken cancellationToken = default);
}