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
/// Provides owner-scoped operations for watering schedules and logs tied to a plant instance.
/// </summary>
public class WateringService : IWateringService
{
    #region Fields

    private readonly IRepository<WateringSchedule> _wateringScheduleRepository;
    private readonly IRepository<WateringLog> _wateringLogRepository;
    private readonly IRepository<PlantInstance> _plantInstanceRepository;
    private readonly IGropStaticCacheManager _staticCacheManager;

    #endregion

    #region Ctor

    public WateringService(
        IRepository<WateringSchedule> wateringScheduleRepository,
        IRepository<WateringLog> wateringLogRepository,
        IRepository<PlantInstance> plantInstanceRepository,
        IGropStaticCacheManager staticCacheManager)
    {
        _wateringScheduleRepository = wateringScheduleRepository ?? throw new ArgumentNullException(nameof(wateringScheduleRepository));
        _wateringLogRepository = wateringLogRepository ?? throw new ArgumentNullException(nameof(wateringLogRepository));
        _plantInstanceRepository = plantInstanceRepository ?? throw new ArgumentNullException(nameof(plantInstanceRepository));
        _staticCacheManager = staticCacheManager ?? throw new ArgumentNullException(nameof(staticCacheManager));
    }

    #endregion

    #region Public

    /// <inheritdoc />
    public async Task<IReadOnlyList<WateringSchedule>> GetSchedulesAsync(int plantInstanceId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        await EnsurePlantInstanceOwnedAsync(plantInstanceId, ownerId, cancellationToken);

        var cacheKey = _staticCacheManager.PrepareKey(
            WateringCacheDefaults.SchedulesByInstanceKey, ownerId.ToString("N"), plantInstanceId);

        return await _staticCacheManager.GetAsync(cacheKey, () =>
            _wateringScheduleRepository.GetAllAsync(
                query => query
                    .Where(schedule => schedule.PlantInstanceId == plantInstanceId && schedule.OwnerId == ownerId)
                    .OrderBy(schedule => schedule.Season)
                    .ThenBy(schedule => schedule.Id),
                cancellationToken: cancellationToken));
    }

    /// <inheritdoc />
    public async Task<WateringSchedule> CreateScheduleAsync(int plantInstanceId, WateringSchedule schedule, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(schedule);
        ValidateScheduleFrequency(schedule.FrequencyDays);

        var plantInstance = await EnsurePlantInstanceOwnedAsync(plantInstanceId, schedule.OwnerId, cancellationToken);
        schedule.PlantInstanceId = plantInstanceId;
        schedule.OwnerId = plantInstance.OwnerId;
        schedule.Notes = schedule.Notes?.Trim();
        AuditableEntityHelper.StampForCreate(schedule);

        var created = await _wateringScheduleRepository.CreateAsync(schedule, cancellationToken: cancellationToken);
        await _staticCacheManager.RemoveByPrefixAsync(WateringCacheDefaults.SchedulePrefix);

        return created;
    }

    /// <inheritdoc />
    public async Task<WateringSchedule> UpdateScheduleAsync(int plantInstanceId, WateringSchedule schedule, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(schedule);
        ValidateScheduleFrequency(schedule.FrequencyDays);

        await EnsurePlantInstanceOwnedAsync(plantInstanceId, schedule.OwnerId, cancellationToken);
        var existingSchedule = await EnsureScheduleOwnedAsync(plantInstanceId, schedule.Id, schedule.OwnerId, cancellationToken);
        existingSchedule.Season = schedule.Season;
        existingSchedule.FrequencyDays = schedule.FrequencyDays;
        existingSchedule.WaterAmountL = schedule.WaterAmountL;
        existingSchedule.TimeOfDay = schedule.TimeOfDay;
        existingSchedule.Notes = schedule.Notes?.Trim();
        AuditableEntityHelper.StampForUpdate(existingSchedule);

        var updated = await _wateringScheduleRepository.UpdateAsync(existingSchedule, cancellationToken: cancellationToken);
        await _staticCacheManager.RemoveByPrefixAsync(WateringCacheDefaults.SchedulePrefix);

        return updated;
    }

    /// <inheritdoc />
    public async Task DeleteScheduleAsync(int plantInstanceId, int scheduleId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        await EnsurePlantInstanceOwnedAsync(plantInstanceId, ownerId, cancellationToken);
        var schedule = await EnsureScheduleOwnedAsync(plantInstanceId, scheduleId, ownerId, cancellationToken);
        await _wateringScheduleRepository.DeleteAsync(schedule, cancellationToken: cancellationToken);
        await _staticCacheManager.RemoveByPrefixAsync(WateringCacheDefaults.SchedulePrefix);
    }

    /// <inheritdoc />
    public async Task<IPagedList<WateringLog>> GetLogsAsync(int plantInstanceId, Guid ownerId, int pageIndex = 0, int pageSize = 5, CancellationToken cancellationToken = default)
    {
        await EnsurePlantInstanceOwnedAsync(plantInstanceId, ownerId, cancellationToken);

        var cacheKey = _staticCacheManager.PrepareKey(
            WateringCacheDefaults.LogsByInstanceKey, ownerId.ToString("N"), plantInstanceId);

        return await _staticCacheManager.GetAsync(cacheKey, () =>
            _wateringLogRepository.GetPagedAsync(
                query => query
                    .Where(log => log.PlantInstanceId == plantInstanceId && log.OwnerId == ownerId)
                    .OrderByDescending(log => log.WateredAtUtc),
                pageIndex,
                pageSize,
                cancellationToken: cancellationToken));
    }

    /// <inheritdoc />
    public async Task DeleteLogAsync(int plantInstanceId, int logId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        await EnsurePlantInstanceOwnedAsync(plantInstanceId, ownerId, cancellationToken);
        var log = await EnsureLogOwnedAsync(plantInstanceId, logId, ownerId, cancellationToken);
        await _wateringLogRepository.DeleteAsync(log, cancellationToken: cancellationToken);
        await _staticCacheManager.RemoveByPrefixAsync(WateringCacheDefaults.LogPrefix);
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

    private async Task<WateringSchedule> EnsureScheduleOwnedAsync(int plantInstanceId, int scheduleId, Guid ownerId, CancellationToken cancellationToken)
    {
        var schedule = await _wateringScheduleRepository.FirstOrDefaultAsync(
            entity => entity.Id == scheduleId && entity.PlantInstanceId == plantInstanceId && entity.OwnerId == ownerId,
            cancellationToken: cancellationToken);

        return schedule ?? throw new DomainException($"WateringSchedule with id '{scheduleId}' was not found for plant instance '{plantInstanceId}'.");
    }

    private async Task<WateringLog> EnsureLogOwnedAsync(int plantInstanceId, int logId, Guid ownerId, CancellationToken cancellationToken)
    {
        var log = await _wateringLogRepository.FirstOrDefaultAsync(
            entity => entity.Id == logId && entity.PlantInstanceId == plantInstanceId && entity.OwnerId == ownerId,
            cancellationToken: cancellationToken);

        return log ?? throw new DomainException($"WateringLog with id '{logId}' was not found for plant instance '{plantInstanceId}'.");
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