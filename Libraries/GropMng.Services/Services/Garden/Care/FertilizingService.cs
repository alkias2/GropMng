using GropMng.Core;
using GropMng.Core.Caching;
using GropMng.Core.Common.Exceptions;
using GropMng.Core.Domain.Garden.Care;
using GropMng.Core.Domain.Garden.Plants;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.Garden.Care;
using GropMng.Services.Caching.Garden;
using GropMng.Services.Helpers;

namespace GropMng.Services.Services.Garden.Care;

/// <summary>
/// Provides owner-scoped operations for fertilizing schedules and logs tied to a plant instance.
/// </summary>
public class FertilizingService : IFertilizingService
{
    #region Fields

    private readonly IRepository<FertilizingSchedule> _fertilizingScheduleRepository;
    private readonly IRepository<FertilizingLog> _fertilizingLogRepository;
    private readonly IRepository<PlantInstance> _plantInstanceRepository;
    private readonly IRepository<Fertilizer> _fertilizerRepository;
    private readonly IGropStaticCacheManager _staticCacheManager;

    #endregion

    #region Ctor

    public FertilizingService(
        IRepository<FertilizingSchedule> fertilizingScheduleRepository,
        IRepository<FertilizingLog> fertilizingLogRepository,
        IRepository<PlantInstance> plantInstanceRepository,
        IRepository<Fertilizer> fertilizerRepository,
        IGropStaticCacheManager staticCacheManager)
    {
        _fertilizingScheduleRepository = fertilizingScheduleRepository ?? throw new ArgumentNullException(nameof(fertilizingScheduleRepository));
        _fertilizingLogRepository = fertilizingLogRepository ?? throw new ArgumentNullException(nameof(fertilizingLogRepository));
        _plantInstanceRepository = plantInstanceRepository ?? throw new ArgumentNullException(nameof(plantInstanceRepository));
        _fertilizerRepository = fertilizerRepository ?? throw new ArgumentNullException(nameof(fertilizerRepository));
        _staticCacheManager = staticCacheManager ?? throw new ArgumentNullException(nameof(staticCacheManager));
    }

    #endregion

    #region Public

    /// <inheritdoc />
    public async Task<IReadOnlyList<FertilizingSchedule>> GetSchedulesAsync(int plantInstanceId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        await EnsurePlantInstanceOwnedAsync(plantInstanceId, ownerId, cancellationToken);

        var cacheKey = _staticCacheManager.PrepareKey(
            FertilizingCacheDefaults.SchedulesByInstanceKey, ownerId.ToString("N"), plantInstanceId);

        return await _staticCacheManager.GetAsync(cacheKey, () =>
            _fertilizingScheduleRepository.GetAllAsync(
                query => query
                    .Where(schedule => schedule.PlantInstanceId == plantInstanceId && schedule.OwnerId == ownerId)
                    .OrderBy(schedule => schedule.Season)
                    .ThenBy(schedule => schedule.Id),
                cancellationToken: cancellationToken));
    }

    /// <inheritdoc />
    public async Task<FertilizingSchedule> CreateScheduleAsync(int plantInstanceId, FertilizingSchedule schedule, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(schedule);
        ValidateScheduleFrequency(schedule.FrequencyDays);
        await EnsureFertilizerExistsAsync(schedule.FertilizerId, cancellationToken);

        var plantInstance = await EnsurePlantInstanceOwnedAsync(plantInstanceId, schedule.OwnerId, cancellationToken);
        schedule.PlantInstanceId = plantInstanceId;
        schedule.OwnerId = plantInstance.OwnerId;
        schedule.Notes = schedule.Notes?.Trim();
        AuditableEntityHelper.StampForCreate(schedule);

        var created = await _fertilizingScheduleRepository.CreateAsync(schedule, cancellationToken: cancellationToken);
        await _staticCacheManager.RemoveByPrefixAsync(FertilizingCacheDefaults.SchedulePrefix);

        return created;
    }

    /// <inheritdoc />
    public async Task<FertilizingSchedule> UpdateScheduleAsync(int plantInstanceId, FertilizingSchedule schedule, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(schedule);
        ValidateScheduleFrequency(schedule.FrequencyDays);
        await EnsureFertilizerExistsAsync(schedule.FertilizerId, cancellationToken);

        await EnsurePlantInstanceOwnedAsync(plantInstanceId, schedule.OwnerId, cancellationToken);
        var existingSchedule = await EnsureScheduleOwnedAsync(plantInstanceId, schedule.Id, schedule.OwnerId, cancellationToken);
        existingSchedule.FertilizerId = schedule.FertilizerId;
        existingSchedule.Season = schedule.Season;
        existingSchedule.FrequencyDays = schedule.FrequencyDays;
        existingSchedule.Quantity = schedule.Quantity;
        existingSchedule.Unit = schedule.Unit;
        existingSchedule.Notes = schedule.Notes?.Trim();
        AuditableEntityHelper.StampForUpdate(existingSchedule);

        var updated = await _fertilizingScheduleRepository.UpdateAsync(existingSchedule, cancellationToken: cancellationToken);
        await _staticCacheManager.RemoveByPrefixAsync(FertilizingCacheDefaults.SchedulePrefix);

        return updated;
    }

    /// <inheritdoc />
    public async Task DeleteScheduleAsync(int plantInstanceId, int scheduleId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        await EnsurePlantInstanceOwnedAsync(plantInstanceId, ownerId, cancellationToken);
        var schedule = await EnsureScheduleOwnedAsync(plantInstanceId, scheduleId, ownerId, cancellationToken);
        await _fertilizingScheduleRepository.DeleteAsync(schedule, cancellationToken: cancellationToken);
        await _staticCacheManager.RemoveByPrefixAsync(FertilizingCacheDefaults.SchedulePrefix);
    }

    /// <inheritdoc />
    public async Task<IPagedList<FertilizingLog>> GetLogsAsync(int plantInstanceId, Guid ownerId, int pageIndex = 0, int pageSize = 5, CancellationToken cancellationToken = default)
    {
        await EnsurePlantInstanceOwnedAsync(plantInstanceId, ownerId, cancellationToken);

        var cacheKey = _staticCacheManager.PrepareKey(
            FertilizingCacheDefaults.LogsByInstanceKey, ownerId.ToString("N"), plantInstanceId);

        return await _staticCacheManager.GetAsync(cacheKey, () =>
            _fertilizingLogRepository.GetPagedAsync(
                query => query
                    .Where(log => log.PlantInstanceId == plantInstanceId && log.OwnerId == ownerId)
                    .OrderByDescending(log => log.AppliedAtUtc),
                pageIndex,
                pageSize,
                cancellationToken: cancellationToken));
    }

    /// <inheritdoc />
    public async Task DeleteLogAsync(int plantInstanceId, int logId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        await EnsurePlantInstanceOwnedAsync(plantInstanceId, ownerId, cancellationToken);
        var log = await EnsureLogOwnedAsync(plantInstanceId, logId, ownerId, cancellationToken);
        await _fertilizingLogRepository.DeleteAsync(log, cancellationToken: cancellationToken);
        await _staticCacheManager.RemoveByPrefixAsync(FertilizingCacheDefaults.LogPrefix);
    }

    #endregion

    #region Private

    private async Task<PlantInstance> EnsurePlantInstanceOwnedAsync(int plantInstanceId, Guid ownerId, CancellationToken cancellationToken)
    {
        ValidateOwnerId(ownerId);

        var plantInstance = await _plantInstanceRepository.FirstOrDefaultAsync(
            entity => entity.Id == plantInstanceId && entity.OwnerId == ownerId,
            cancellationToken: cancellationToken);

        return plantInstance ?? throw new DomainException($"PlantInstance with id '{plantInstanceId}' was not found for owner '{ownerId}'.");
    }

    private async Task<FertilizingSchedule> EnsureScheduleOwnedAsync(int plantInstanceId, int scheduleId, Guid ownerId, CancellationToken cancellationToken)
    {
        var schedule = await _fertilizingScheduleRepository.FirstOrDefaultAsync(
            entity => entity.Id == scheduleId && entity.PlantInstanceId == plantInstanceId && entity.OwnerId == ownerId,
            cancellationToken: cancellationToken);

        return schedule ?? throw new DomainException($"FertilizingSchedule with id '{scheduleId}' was not found for plant instance '{plantInstanceId}'.");
    }

    private async Task<FertilizingLog> EnsureLogOwnedAsync(int plantInstanceId, int logId, Guid ownerId, CancellationToken cancellationToken)
    {
        var log = await _fertilizingLogRepository.FirstOrDefaultAsync(
            entity => entity.Id == logId && entity.PlantInstanceId == plantInstanceId && entity.OwnerId == ownerId,
            cancellationToken: cancellationToken);

        return log ?? throw new DomainException($"FertilizingLog with id '{logId}' was not found for plant instance '{plantInstanceId}'.");
    }

    private async Task EnsureFertilizerExistsAsync(int fertilizerId, CancellationToken cancellationToken)
    {
        var fertilizer = await _fertilizerRepository.GetByIdAsync(fertilizerId, cancellationToken: cancellationToken);
        if (fertilizer is null)
            throw new DomainException($"Fertilizer with id '{fertilizerId}' was not found.");
    }

    private static void ValidateOwnerId(Guid ownerId)
    {
        if (ownerId == Guid.Empty)
            throw new DomainException("OwnerId is required.");
    }

    private static void ValidateScheduleFrequency(byte frequencyDays)
    {
        if (frequencyDays == 0)
            throw new DomainException("FrequencyDays must be greater than zero.");
    }

    #endregion
}