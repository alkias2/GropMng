using GropMng.Core.Caching;
using GropMng.Core.Common.Exceptions;
using GropMng.Core.Domain.Garden.Plants;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.Garden.Plants;
using GropMng.Services.Caching.Garden;
using GropMng.Services.Helpers;

namespace GropMng.Services.Services.Garden.Plants;

/// <summary>
/// Provides owner-scoped CRUD for plant photos tied to a plant instance.
/// </summary>
public class PlantPhotoService : IPlantPhotoService
{
    #region Fields

    private readonly IRepository<PlantPhoto> _plantPhotoRepository;
    private readonly IRepository<PlantInstance> _plantInstanceRepository;
    private readonly IGropStaticCacheManager _staticCacheManager;

    #endregion

    #region Ctor

    public PlantPhotoService(
        IRepository<PlantPhoto> plantPhotoRepository,
        IRepository<PlantInstance> plantInstanceRepository,
        IGropStaticCacheManager staticCacheManager)
    {
        _plantPhotoRepository = plantPhotoRepository ?? throw new ArgumentNullException(nameof(plantPhotoRepository));
        _plantInstanceRepository = plantInstanceRepository ?? throw new ArgumentNullException(nameof(plantInstanceRepository));
        _staticCacheManager = staticCacheManager ?? throw new ArgumentNullException(nameof(staticCacheManager));
    }

    #endregion

    #region Public

    /// <inheritdoc />
    public async Task<IReadOnlyList<PlantPhoto>> GetPhotosAsync(int plantInstanceId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        await EnsurePlantInstanceOwnedAsync(plantInstanceId, ownerId, cancellationToken);

        var cacheKey = _staticCacheManager.PrepareKey(
            PlantCacheDefaults.PlantPhotosByInstanceKey, ownerId.ToString("N"), plantInstanceId);

        return await _staticCacheManager.GetAsync(cacheKey, () =>
            _plantPhotoRepository.GetAllAsync(
                query => query
                    .Where(photo => photo.PlantInstanceId == plantInstanceId && photo.OwnerId == ownerId)
                    .OrderBy(photo => photo.DisplayOrder)
                    .ThenBy(photo => photo.Id),
                cancellationToken: cancellationToken));
    }

    /// <inheritdoc />
    public async Task<PlantPhoto?> GetPhotoByIdAsync(int plantInstanceId, int photoId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        await EnsurePlantInstanceOwnedAsync(plantInstanceId, ownerId, cancellationToken);

        var cacheKey = _staticCacheManager.PrepareKey(
            PlantCacheDefaults.PlantPhotoByIdKey, ownerId.ToString("N"), plantInstanceId, photoId);

        return await _staticCacheManager.GetAsync(cacheKey, () =>
            _plantPhotoRepository.FirstOrDefaultAsync(
                entity => entity.Id == photoId && entity.PlantInstanceId == plantInstanceId && entity.OwnerId == ownerId,
                cancellationToken: cancellationToken));
    }

    /// <inheritdoc />
    public async Task<PlantPhoto?> GetMainPhotoAsync(int plantInstanceId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        await EnsurePlantInstanceOwnedAsync(plantInstanceId, ownerId, cancellationToken);

        var photos = await _plantPhotoRepository.GetAllAsync(
            query => query
                .Where(p => p.PlantInstanceId == plantInstanceId && p.OwnerId == ownerId)
                .OrderBy(p => p.DisplayOrder)
                .ThenBy(p => p.Id)
                .Take(1),
            cancellationToken: cancellationToken);

        return photos.FirstOrDefault();
    }

    /// <inheritdoc />
    public async Task<PlantPhoto> CreatePhotoAsync(int plantInstanceId, PlantPhoto photo, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(photo);

        var plantInstance = await EnsurePlantInstanceOwnedAsync(plantInstanceId, photo.OwnerId, cancellationToken);
        photo.PlantInstanceId = plantInstanceId;
        photo.OwnerId = plantInstance.OwnerId;
        photo.Caption = photo.Caption?.Trim();
        AuditableEntityHelper.StampForCreate(photo);

        var created = await _plantPhotoRepository.CreateAsync(photo, cancellationToken: cancellationToken);
        await _staticCacheManager.RemoveByPrefixAsync(PlantCacheDefaults.PlantPhotoPrefix);

        return created;
    }

    /// <inheritdoc />
    public async Task<PlantPhoto> UpdatePhotoAsync(int plantInstanceId, PlantPhoto photo, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(photo);

        await EnsurePlantInstanceOwnedAsync(plantInstanceId, photo.OwnerId, cancellationToken);
        var existingPhoto = await EnsurePhotoOwnedAsync(plantInstanceId, photo.Id, photo.OwnerId, cancellationToken);
        existingPhoto.PictureId = photo.PictureId;
        existingPhoto.TakenDate = photo.TakenDate;
        existingPhoto.Caption = photo.Caption?.Trim();
        existingPhoto.DisplayOrder = photo.DisplayOrder;
        AuditableEntityHelper.StampForUpdate(existingPhoto);

        var updated = await _plantPhotoRepository.UpdateAsync(existingPhoto, cancellationToken: cancellationToken);
        await _staticCacheManager.RemoveByPrefixAsync(PlantCacheDefaults.PlantPhotoPrefix);

        return updated;
    }

    /// <inheritdoc />
    public async Task DeletePhotoAsync(int plantInstanceId, int photoId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        await EnsurePlantInstanceOwnedAsync(plantInstanceId, ownerId, cancellationToken);
        var photo = await EnsurePhotoOwnedAsync(plantInstanceId, photoId, ownerId, cancellationToken);
        await _plantPhotoRepository.DeleteAsync(photo, cancellationToken: cancellationToken);
        await _staticCacheManager.RemoveByPrefixAsync(PlantCacheDefaults.PlantPhotoPrefix);
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

    private async Task<PlantPhoto> EnsurePhotoOwnedAsync(int plantInstanceId, int photoId, Guid ownerId, CancellationToken cancellationToken)
    {
        var photo = await _plantPhotoRepository.FirstOrDefaultAsync(
            entity => entity.Id == photoId && entity.PlantInstanceId == plantInstanceId && entity.OwnerId == ownerId,
            cancellationToken: cancellationToken);

        return photo ?? throw new DomainException($"PlantPhoto with id '{photoId}' was not found for plant instance '{plantInstanceId}'.");
    }

    private static void ValidateOwnerId(Guid ownerId)
    {
        if (ownerId == Guid.Empty)
            throw new DomainException("OwnerId is required.");
    }

    #endregion
}