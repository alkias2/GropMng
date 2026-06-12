using GropMng.Core.Caching;
using GropMng.Core.Common.Exceptions;
using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Health;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.Garden.Health;
using GropMng.Services.Caching;
using Microsoft.EntityFrameworkCore;

namespace GropMng.Services.Services.Garden.Health;

/// <summary>
/// Manages plant problem records and their treatment schedules.
/// </summary>
public class PlantProblemService : IPlantProblemService
{
    #region Fields

    private readonly IRepository<PlantProblemRecord> _problemRecordRepository;
    private readonly IRepository<PlantProblemSchedule> _scheduleRepository;
    private readonly IRepository<AdminNotification> _notificationRepository;
    private readonly IRepository<GropMng.Core.Domain.Garden.Plants.PlantInstance> _plantInstanceRepository;
    private readonly IGropStaticCacheManager _staticCacheManager;

    #endregion

    #region Ctor

    public PlantProblemService(
        IRepository<PlantProblemRecord> problemRecordRepository,
        IRepository<PlantProblemSchedule> scheduleRepository,
        IRepository<AdminNotification> notificationRepository,
        IRepository<GropMng.Core.Domain.Garden.Plants.PlantInstance> plantInstanceRepository,
        IGropStaticCacheManager staticCacheManager)
    {
        _problemRecordRepository = problemRecordRepository ?? throw new ArgumentNullException(nameof(problemRecordRepository));
        _scheduleRepository = scheduleRepository ?? throw new ArgumentNullException(nameof(scheduleRepository));
        _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
        _plantInstanceRepository = plantInstanceRepository ?? throw new ArgumentNullException(nameof(plantInstanceRepository));
        _staticCacheManager = staticCacheManager ?? throw new ArgumentNullException(nameof(staticCacheManager));
    }

    #endregion

    #region Public — Problem Records

    /// <inheritdoc />
    public async Task<PlantProblemRecord> GetByIdAsync(int id, Guid ownerId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ownerId);
        return await EnsureRecordOwnedAsync(id, ownerId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<PlantProblemRecord>> GetByPlantInstanceAsync(int plantInstanceId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ownerId);

        var records = await _problemRecordRepository.GetAllAsync(
            query => query
                .Where(r => r.PlantInstanceId == plantInstanceId && r.OwnerId == ownerId && !r.IsDeleted)
                .OrderByDescending(r => r.DetectedDate),
            cancellationToken: cancellationToken);

        return records.ToList();
    }

    /// <inheritdoc />
    public async Task<PlantProblemRecord> CreateAsync(PlantProblemRecord record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);
        ValidateProblemRecord(record);

        await EnsurePlantInstanceExistsAsync(record.PlantInstanceId, record.OwnerId, cancellationToken);

        if (record.DiseaseKnowledgeId.HasValue)
        {
            record.NotifyAdmin = false;
        }

        var now = DateTime.UtcNow;
        record.CreatedAtUtc = now;
        record.UpdatedAtUtc = now;
        record.IsDeleted = false;
        record.DeletedAtUtc = null;
        record.ResolvedDate = record.ProblemStatus == ProblemStatus.Resolved
            ? DateOnly.FromDateTime(DateTime.UtcNow)
            : null;

        var created = await _problemRecordRepository.CreateAsync(record, cancellationToken: cancellationToken);

        if (record.NotifyAdmin && !record.DiseaseKnowledgeId.HasValue)
        {
            var notification = new AdminNotification
            {
                OwnerId = record.OwnerId,
                PlantInstanceId = record.PlantInstanceId,
                ProblemName = record.ProblemName.Trim(),
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
                IsDeleted = false,
                DeletedAtUtc = null
            };
            await _notificationRepository.CreateAsync(notification, cancellationToken: cancellationToken);
        }

        await ClearProblemCacheAsync();
        return created;
    }

    /// <inheritdoc />
    public async Task<PlantProblemRecord> UpdateAsync(PlantProblemRecord record, Guid ownerId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);
        ValidateProblemRecord(record);

        var existing = await EnsureRecordOwnedAsync(record.Id, ownerId, cancellationToken);

        existing.ProblemName = record.ProblemName.Trim();
        existing.DiseaseKnowledgeId = record.DiseaseKnowledgeId;
        existing.DetectedDate = record.DetectedDate;
        existing.Severity = record.Severity;
        existing.InfoSource = record.InfoSource;
        existing.Notes = record.Notes?.Trim();

        var previousStatus = existing.ProblemStatus;
        existing.ProblemStatus = record.ProblemStatus;

        if (existing.ProblemStatus == ProblemStatus.Resolved && previousStatus != ProblemStatus.Resolved)
        {
            existing.ResolvedDate = DateOnly.FromDateTime(DateTime.UtcNow);
        }
        else if (existing.ProblemStatus != ProblemStatus.Resolved && previousStatus == ProblemStatus.Resolved)
        {
            existing.ResolvedDate = null;
        }

        if (existing.DiseaseKnowledgeId.HasValue)
        {
            existing.NotifyAdmin = false;
        }
        else
        {
            existing.NotifyAdmin = record.NotifyAdmin;
        }

        existing.UpdatedAtUtc = DateTime.UtcNow;

        var updated = await _problemRecordRepository.UpdateAsync(existing, cancellationToken: cancellationToken);
        await ClearProblemCacheAsync();
        return updated;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(int id, Guid ownerId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ownerId);

        var record = await EnsureRecordOwnedAsync(id, ownerId, cancellationToken);

        var schedules = await _scheduleRepository.GetAllAsync(
            query => query.Where(s => s.PlantProblemRecordId == id && !s.IsDeleted),
            cancellationToken: cancellationToken);

        var now = DateTime.UtcNow;
        foreach (var schedule in schedules)
        {
            schedule.IsDeleted = true;
            schedule.DeletedAtUtc = now;
            schedule.UpdatedAtUtc = now;
            await _scheduleRepository.UpdateAsync(schedule, cancellationToken: cancellationToken);
        }

        record.IsDeleted = true;
        record.DeletedAtUtc = now;
        record.UpdatedAtUtc = now;
        await _problemRecordRepository.UpdateAsync(record, cancellationToken: cancellationToken);

        await ClearProblemCacheAsync();
    }

    #endregion

    #region Public — Schedules

    /// <inheritdoc />
    public async Task<List<PlantProblemSchedule>> GetUpcomingSchedulesAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ownerId);

        var schedules = await _scheduleRepository.GetAllAsync(
            query => query
                .Include(s => s.PlantProblemRecord)
                .Where(s => s.OwnerId == ownerId
                    && !s.IsDeleted
                    && s.ScheduleStatus == ScheduleStatus.Active)
                .OrderBy(s => s.NextDueDate),
            cancellationToken: cancellationToken);

        return schedules.ToList();
    }

    /// <inheritdoc />
    public async Task<PlantProblemSchedule> AddScheduleAsync(PlantProblemSchedule schedule, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(schedule);
        ValidateSchedule(schedule);

        await EnsureRecordOwnedAsync(schedule.PlantProblemRecordId, schedule.OwnerId, cancellationToken);

        schedule.NextDueDate = CalculateNextDueDate(schedule.StartDate, schedule.FrequencyValue, schedule.FrequencyUnit);

        var now = DateTime.UtcNow;
        schedule.CreatedAtUtc = now;
        schedule.UpdatedAtUtc = now;
        schedule.IsDeleted = false;
        schedule.DeletedAtUtc = null;

        var created = await _scheduleRepository.CreateAsync(schedule, cancellationToken: cancellationToken);
        await ClearProblemCacheAsync();
        return created;
    }

    /// <inheritdoc />
    public async Task<PlantProblemSchedule> UpdateScheduleAsync(PlantProblemSchedule schedule, Guid ownerId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(schedule);
        ValidateSchedule(schedule);

        var existing = await EnsureScheduleOwnedAsync(schedule.Id, ownerId, cancellationToken);

        existing.ActionName = schedule.ActionName.Trim();
        existing.FrequencyValue = schedule.FrequencyValue;
        existing.FrequencyUnit = schedule.FrequencyUnit;
        existing.DosageNotes = schedule.DosageNotes?.Trim();
        existing.StartDate = schedule.StartDate;
        existing.NextDueDate = CalculateNextDueDate(schedule.StartDate, schedule.FrequencyValue, schedule.FrequencyUnit);
        existing.ScheduleStatus = schedule.ScheduleStatus;
        existing.UpdatedAtUtc = DateTime.UtcNow;

        var updated = await _scheduleRepository.UpdateAsync(existing, cancellationToken: cancellationToken);
        await ClearProblemCacheAsync();
        return updated;
    }

    /// <inheritdoc />
    public async Task DeleteScheduleAsync(int id, Guid ownerId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ownerId);

        var schedule = await EnsureScheduleOwnedAsync(id, ownerId, cancellationToken);
        schedule.IsDeleted = true;
        schedule.DeletedAtUtc = DateTime.UtcNow;
        schedule.UpdatedAtUtc = DateTime.UtcNow;

        await _scheduleRepository.UpdateAsync(schedule, cancellationToken: cancellationToken);
        await ClearProblemCacheAsync();
    }

    #endregion

    #region Privates — Guards

    private async Task<PlantProblemRecord> EnsureRecordOwnedAsync(int id, Guid ownerId, CancellationToken cancellationToken)
    {
        var record = await _problemRecordRepository.FirstOrDefaultAsync(
            entity => entity.Id == id && entity.OwnerId == ownerId && !entity.IsDeleted,
            cancellationToken: cancellationToken);

        return record ?? throw new DomainException($"PlantProblemRecord with id '{id}' was not found for the specified owner.");
    }

    private async Task<PlantProblemSchedule> EnsureScheduleOwnedAsync(int id, Guid ownerId, CancellationToken cancellationToken)
    {
        var schedule = await _scheduleRepository.FirstOrDefaultAsync(
            entity => entity.Id == id && entity.OwnerId == ownerId && !entity.IsDeleted,
            cancellationToken: cancellationToken);

        return schedule ?? throw new DomainException($"PlantProblemSchedule with id '{id}' was not found for the specified owner.");
    }

    private async Task EnsurePlantInstanceExistsAsync(int plantInstanceId, Guid ownerId, CancellationToken cancellationToken)
    {
        var instance = await _plantInstanceRepository.FirstOrDefaultAsync(
            entity => entity.Id == plantInstanceId && entity.OwnerId == ownerId,
            cancellationToken: cancellationToken);

        if (instance is null)
            throw new DomainException($"PlantInstance with id '{plantInstanceId}' was not found for the specified owner.");
    }

    #endregion

    #region Privates — Validation

    private static void ValidateProblemRecord(PlantProblemRecord record)
    {
        if (string.IsNullOrWhiteSpace(record.ProblemName))
            throw new DomainException("Problem name is required.");

        if (record.ProblemName.Length > 300)
            throw new DomainException($"Problem name must not exceed 300 characters (was {record.ProblemName.Length}).");

        if (record.DetectedDate > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new DomainException("Detected date cannot be in the future.");
    }

    private static void ValidateSchedule(PlantProblemSchedule schedule)
    {
        if (string.IsNullOrWhiteSpace(schedule.ActionName))
            throw new DomainException("Action name is required.");

        if (schedule.ActionName.Length > 300)
            throw new DomainException($"Action name must not exceed 300 characters (was {schedule.ActionName.Length}).");

        if (schedule.FrequencyValue <= 0)
            throw new DomainException("Frequency value must be greater than zero.");
    }

    #endregion

    #region Privates — Helpers

    private static DateOnly CalculateNextDueDate(DateOnly startDate, int frequencyValue, ScheduleFrequencyUnit frequencyUnit)
    {
        return frequencyUnit switch
        {
            ScheduleFrequencyUnit.Days => startDate.AddDays(frequencyValue),
            ScheduleFrequencyUnit.Weeks => startDate.AddDays(frequencyValue * 7),
            ScheduleFrequencyUnit.Months => startDate.AddMonths(frequencyValue),
            _ => throw new DomainException($"Unsupported frequency unit: {frequencyUnit}")
        };
    }

    private async Task ClearProblemCacheAsync()
    {
        await _staticCacheManager.RemoveByPrefixAsync(ProblemCacheDefaults.ProblemRecordPrefix);
    }

    #endregion
}