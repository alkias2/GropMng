using GropMng.Core;
using GropMng.Core.Caching;
using GropMng.Core.Common.Exceptions;
using GropMng.Core.Domain.Garden.Care;
using GropMng.Core.Domain.Garden.Plants;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.Garden.Plants;
using GropMng.Services.Caching.Garden;
using GropMng.Services.Helpers;

namespace GropMng.Services.Services.Garden.Plants;

/// <summary>
/// Provides owner-scoped CRUD for repotting logs tied to a plant instance.
/// The RepotPlant aggregate-root operation remains in PlantInstanceService.
/// </summary>
public class RepottingLogService : IRepottingLogService
{
    #region Fields

    private readonly IRepository<RepottingLog> _repottingLogRepository;
    private readonly IRepository<PlantInstance> _plantInstanceRepository;
    private readonly IGropStaticCacheManager _staticCacheManager;

    #endregion

    #region Ctor

    public RepottingLogService(
        IRepository<RepottingLog> repottingLogRepository,
        IRepository<PlantInstance> plantInstanceRepository,
        IGropStaticCacheManager staticCacheManager)
    {
        _repottingLogRepository = repottingLogRepository ?? throw new ArgumentNullException(nameof(repottingLogRepository));
        _plantInstanceRepository = plantInstanceRepository ?? throw new ArgumentNullException(nameof(plantInstanceRepository));
        _staticCacheManager = staticCacheManager ?? throw new ArgumentNullException(nameof(staticCacheManager));
    }

    #endregion

    #region Public

    /// <inheritdoc />
    public async Task<IPagedList<RepottingLog>> GetLogsAsync(int plantInstanceId, Guid ownerId, int pageIndex = 0, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        await EnsurePlantInstanceOwnedAsync(plantInstanceId, ownerId, cancellationToken);

        var cacheKey = _staticCacheManager.PrepareKey(
            RepottingCacheDefaults.LogsByInstanceKey, ownerId.ToString("N"), plantInstanceId);

        return await _staticCacheManager.GetAsync(cacheKey, () =>
            _repottingLogRepository.GetPagedAsync(
                query => query
                    .Where(log => log.PlantInstanceId == plantInstanceId && log.OwnerId == ownerId)
                    .OrderByDescending(log => log.RepottedAtUtc)
                    .ThenByDescending(log => log.Id),
                pageIndex,
                pageSize,
                cancellationToken: cancellationToken));
    }

    /// <inheritdoc />
    public async Task<RepottingLog> UpdateLogAsync(int plantInstanceId, RepottingLog repottingLog, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repottingLog);
        ValidateOwnerId(repottingLog.OwnerId);

        await EnsurePlantInstanceOwnedAsync(plantInstanceId, repottingLog.OwnerId, cancellationToken);
        var existingLog = await EnsureLogOwnedAsync(plantInstanceId, repottingLog.Id, repottingLog.OwnerId, cancellationToken);

        existingLog.NewContainerId = repottingLog.NewContainerId;
        existingLog.NewSoilMixId = repottingLog.NewSoilMixId;
        existingLog.ContainerChanged = existingLog.NewContainerId != existingLog.PreviousContainerId;
        existingLog.SoilMixChanged = existingLog.NewSoilMixId != existingLog.PreviousSoilMixId;
        existingLog.RepottedAtUtc = repottingLog.RepottedAtUtc;
        existingLog.Notes = repottingLog.Notes?.Trim();
        AuditableEntityHelper.StampForUpdate(existingLog);

        var updated = await _repottingLogRepository.UpdateAsync(existingLog, cancellationToken: cancellationToken);
        await _staticCacheManager.RemoveByPrefixAsync(RepottingCacheDefaults.Prefix);

        return updated;
    }

    /// <inheritdoc />
    public async Task DeleteLogAsync(int plantInstanceId, int logId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        await EnsurePlantInstanceOwnedAsync(plantInstanceId, ownerId, cancellationToken);
        var log = await EnsureLogOwnedAsync(plantInstanceId, logId, ownerId, cancellationToken);
        await _repottingLogRepository.DeleteAsync(log, cancellationToken: cancellationToken);
        await _staticCacheManager.RemoveByPrefixAsync(RepottingCacheDefaults.Prefix);
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

    private async Task<RepottingLog> EnsureLogOwnedAsync(int plantInstanceId, int logId, Guid ownerId, CancellationToken cancellationToken)
    {
        var log = await _repottingLogRepository.FirstOrDefaultAsync(
            entity => entity.Id == logId && entity.PlantInstanceId == plantInstanceId && entity.OwnerId == ownerId,
            cancellationToken: cancellationToken);

        return log ?? throw new DomainException($"RepottingLog with id '{logId}' was not found for plant instance '{plantInstanceId}'.");
    }

    private static void ValidateOwnerId(Guid ownerId)
    {
        if (ownerId == Guid.Empty)
            throw new DomainException("OwnerId is required.");
    }

    #endregion
}