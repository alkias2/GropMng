using GropMng.Core.Caching;
using GropMng.Core.Common.Exceptions;
using GropMng.Core.Domain.Garden.Health;
using GropMng.Core.Domain.Garden.Plants;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.Garden.Health;
using GropMng.Services.Caching.Garden;
using GropMng.Services.Helpers;

namespace GropMng.Services.Services.Garden.Health;

/// <summary>
/// Provides owner-scoped CRUD for plant disease records and their photos,
/// scoped to a plant instance.
/// </summary>
public class PlantDiseaseService : IPlantDiseaseService
{
    #region Fields

    private readonly IRepository<PlantDiseaseRecord> _plantDiseaseRecordRepository;
    private readonly IRepository<DiseasePhoto> _diseasePhotoRepository;
    private readonly IRepository<PlantInstance> _plantInstanceRepository;
    private readonly IRepository<Disease> _diseaseRepository;
    private readonly IGropStaticCacheManager _staticCacheManager;

    #endregion

    #region Ctor

    public PlantDiseaseService(
        IRepository<PlantDiseaseRecord> plantDiseaseRecordRepository,
        IRepository<DiseasePhoto> diseasePhotoRepository,
        IRepository<PlantInstance> plantInstanceRepository,
        IRepository<Disease> diseaseRepository,
        IGropStaticCacheManager staticCacheManager)
    {
        _plantDiseaseRecordRepository = plantDiseaseRecordRepository ?? throw new ArgumentNullException(nameof(plantDiseaseRecordRepository));
        _diseasePhotoRepository = diseasePhotoRepository ?? throw new ArgumentNullException(nameof(diseasePhotoRepository));
        _plantInstanceRepository = plantInstanceRepository ?? throw new ArgumentNullException(nameof(plantInstanceRepository));
        _diseaseRepository = diseaseRepository ?? throw new ArgumentNullException(nameof(diseaseRepository));
        _staticCacheManager = staticCacheManager ?? throw new ArgumentNullException(nameof(staticCacheManager));
    }

    #endregion

    #region Public

    /// <inheritdoc />
    public async Task<IReadOnlyList<PlantDiseaseRecord>> GetRecordsAsync(int plantInstanceId, Guid ownerId, bool includePhotos = false, CancellationToken cancellationToken = default)
    {
        await EnsurePlantInstanceOwnedAsync(plantInstanceId, ownerId, cancellationToken);

        var cacheKey = _staticCacheManager.PrepareKey(
            DiseaseCacheDefaults.PlantDiseaseRecordsByInstanceKey, ownerId.ToString("N"), plantInstanceId);

        var records = await _staticCacheManager.GetAsync(cacheKey, () =>
            _plantDiseaseRecordRepository.GetAllAsync(
                query => query
                    .Where(record => record.PlantInstanceId == plantInstanceId && record.OwnerId == ownerId)
                    .OrderByDescending(record => record.DetectedDate)
                    .ThenBy(record => record.Id),
                cancellationToken: cancellationToken));

        if (!includePhotos)
            return records;

        foreach (var record in records)
        {
            record.Photos = (await _diseasePhotoRepository.GetAllAsync(
                query => query
                    .Where(photo => photo.PlantDiseaseRecordId == record.Id && photo.OwnerId == ownerId)
                    .OrderBy(photo => photo.Id),
                cancellationToken: cancellationToken)).ToList();
        }

        return records;
    }

    /// <inheritdoc />
    public async Task<PlantDiseaseRecord> CreateRecordAsync(int plantInstanceId, PlantDiseaseRecord record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);
        ValidateDiseaseRecord(record);
        await EnsureDiseaseExistsAsync(record.DiseaseId, cancellationToken);

        var plantInstance = await EnsurePlantInstanceOwnedAsync(plantInstanceId, record.OwnerId, cancellationToken);
        record.PlantInstanceId = plantInstanceId;
        record.OwnerId = plantInstance.OwnerId;
        record.TreatmentUsed = record.TreatmentUsed?.Trim();
        record.Notes = record.Notes?.Trim();
        AuditableEntityHelper.StampForCreate(record);

        var created = await _plantDiseaseRecordRepository.CreateAsync(record, cancellationToken: cancellationToken);
        await _staticCacheManager.RemoveByPrefixAsync(DiseaseCacheDefaults.PlantDiseaseRecordPrefix);

        return created;
    }

    /// <inheritdoc />
    public async Task<PlantDiseaseRecord> UpdateRecordAsync(int plantInstanceId, PlantDiseaseRecord record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);
        ValidateDiseaseRecord(record);
        await EnsureDiseaseExistsAsync(record.DiseaseId, cancellationToken);

        await EnsurePlantInstanceOwnedAsync(plantInstanceId, record.OwnerId, cancellationToken);
        var existingRecord = await EnsureRecordOwnedAsync(plantInstanceId, record.Id, record.OwnerId, cancellationToken);
        existingRecord.DiseaseId = record.DiseaseId;
        existingRecord.DetectedDate = record.DetectedDate;
        existingRecord.ResolvedDate = record.ResolvedDate;
        existingRecord.Severity = record.Severity;
        existingRecord.TreatmentUsed = record.TreatmentUsed?.Trim();
        existingRecord.Outcome = record.Outcome;
        existingRecord.Notes = record.Notes?.Trim();
        AuditableEntityHelper.StampForUpdate(existingRecord);

        var updated = await _plantDiseaseRecordRepository.UpdateAsync(existingRecord, cancellationToken: cancellationToken);
        await _staticCacheManager.RemoveByPrefixAsync(DiseaseCacheDefaults.PlantDiseaseRecordPrefix);

        return updated;
    }

    /// <inheritdoc />
    public async Task DeleteRecordAsync(int plantInstanceId, int recordId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        await EnsurePlantInstanceOwnedAsync(plantInstanceId, ownerId, cancellationToken);
        var record = await EnsureRecordOwnedAsync(plantInstanceId, recordId, ownerId, cancellationToken);
        await _plantDiseaseRecordRepository.DeleteAsync(record, cancellationToken: cancellationToken);
        await _staticCacheManager.RemoveByPrefixAsync(DiseaseCacheDefaults.PlantDiseaseRecordPrefix);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DiseasePhoto>> GetPhotosAsync(int plantInstanceId, int recordId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        await EnsureRecordOwnedAsync(plantInstanceId, recordId, ownerId, cancellationToken);

        var cacheKey = _staticCacheManager.PrepareKey(
            DiseaseCacheDefaults.DiseasePhotosByRecordKey, ownerId.ToString("N"), recordId);

        return await _staticCacheManager.GetAsync(cacheKey, () =>
            _diseasePhotoRepository.GetAllAsync(
                query => query.Where(photo => photo.PlantDiseaseRecordId == recordId && photo.OwnerId == ownerId).OrderBy(photo => photo.Id),
                cancellationToken: cancellationToken));
    }

    /// <inheritdoc />
    public async Task<DiseasePhoto> CreatePhotoAsync(int plantInstanceId, int recordId, DiseasePhoto photo, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(photo);

        var record = await EnsureRecordOwnedAsync(plantInstanceId, recordId, photo.OwnerId, cancellationToken);
        photo.PlantDiseaseRecordId = record.Id;
        photo.OwnerId = record.OwnerId;
        photo.Notes = photo.Notes?.Trim();
        AuditableEntityHelper.StampForCreate(photo);

        var created = await _diseasePhotoRepository.CreateAsync(photo, cancellationToken: cancellationToken);
        await _staticCacheManager.RemoveByPrefixAsync(DiseaseCacheDefaults.PlantDiseaseRecordPrefix);

        return created;
    }

    /// <inheritdoc />
    public async Task<DiseasePhoto> UpdatePhotoAsync(int plantInstanceId, int recordId, DiseasePhoto photo, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(photo);

        await EnsureRecordOwnedAsync(plantInstanceId, recordId, photo.OwnerId, cancellationToken);
        var existingPhoto = await EnsurePhotoOwnedAsync(recordId, photo.Id, photo.OwnerId, cancellationToken);
        existingPhoto.PictureId = photo.PictureId;
        existingPhoto.TakenDate = photo.TakenDate;
        existingPhoto.Notes = photo.Notes?.Trim();
        existingPhoto.DisplayOrder = photo.DisplayOrder;
        AuditableEntityHelper.StampForUpdate(existingPhoto);

        var updated = await _diseasePhotoRepository.UpdateAsync(existingPhoto, cancellationToken: cancellationToken);
        await _staticCacheManager.RemoveByPrefixAsync(DiseaseCacheDefaults.PlantDiseaseRecordPrefix);

        return updated;
    }

    /// <inheritdoc />
    public async Task DeletePhotoAsync(int plantInstanceId, int recordId, int photoId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        await EnsureRecordOwnedAsync(plantInstanceId, recordId, ownerId, cancellationToken);
        var photo = await EnsurePhotoOwnedAsync(recordId, photoId, ownerId, cancellationToken);
        await _diseasePhotoRepository.DeleteAsync(photo, cancellationToken: cancellationToken);
        await _staticCacheManager.RemoveByPrefixAsync(DiseaseCacheDefaults.PlantDiseaseRecordPrefix);
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

    private async Task<PlantDiseaseRecord> EnsureRecordOwnedAsync(int plantInstanceId, int recordId, Guid ownerId, CancellationToken cancellationToken)
    {
        var record = await _plantDiseaseRecordRepository.FirstOrDefaultAsync(
            entity => entity.Id == recordId && entity.PlantInstanceId == plantInstanceId && entity.OwnerId == ownerId,
            cancellationToken: cancellationToken);

        return record ?? throw new DomainException($"PlantDiseaseRecord with id '{recordId}' was not found for plant instance '{plantInstanceId}'.");
    }

    private async Task<DiseasePhoto> EnsurePhotoOwnedAsync(int recordId, int photoId, Guid ownerId, CancellationToken cancellationToken)
    {
        var photo = await _diseasePhotoRepository.FirstOrDefaultAsync(
            entity => entity.Id == photoId && entity.PlantDiseaseRecordId == recordId && entity.OwnerId == ownerId,
            cancellationToken: cancellationToken);

        return photo ?? throw new DomainException($"DiseasePhoto with id '{photoId}' was not found for disease record '{recordId}'.");
    }

    private async Task EnsureDiseaseExistsAsync(int diseaseId, CancellationToken cancellationToken)
    {
        var disease = await _diseaseRepository.GetByIdAsync(diseaseId, cancellationToken: cancellationToken);
        if (disease is null)
            throw new DomainException($"Disease with id '{diseaseId}' was not found.");
    }

    private static void ValidateOwnerId(Guid ownerId)
    {
        if (ownerId == Guid.Empty)
            throw new DomainException("OwnerId is required.");
    }

    private static void ValidateDiseaseRecord(PlantDiseaseRecord record)
    {
        ValidateOwnerId(record.OwnerId);

        if (record.ResolvedDate.HasValue && record.ResolvedDate.Value < record.DetectedDate)
            throw new DomainException("ResolvedDate cannot be earlier than DetectedDate.");
    }

    #endregion
}