using GropMng.Core;
using GropMng.Core.Common.Exceptions;
using GropMng.Core.Domain.Garden;
using GropMng.Core.Domain.Garden.Care;
using GropMng.Core.Domain.Garden.Health;
using GropMng.Core.Domain.Garden.Locations;
using GropMng.Core.Domain.Garden.Plants;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.Garden.Plants;

namespace GropMng.Services.Services.Garden.Plants;

/// <summary>
/// Provides aggregate-root operations for plant instances and their supporting entities.
/// </summary>
public class PlantInstanceService : IPlantInstanceService
{
    #region Fields

    private readonly IRepository<PlantInstance> _plantInstanceRepository;
    private readonly IRepository<Plant> _plantRepository;
    private readonly IRepository<GardenSpot> _gardenSpotRepository;
    private readonly IRepository<Container> _containerRepository;
    private readonly IRepository<SoilMix> _soilMixRepository;
    private readonly IRepository<WateringSchedule> _wateringScheduleRepository;
    private readonly IRepository<FertilizingSchedule> _fertilizingScheduleRepository;
    private readonly IRepository<PlantPhoto> _plantPhotoRepository;
    private readonly IRepository<PlantNote> _plantNoteRepository;
    private readonly IRepository<PlantDiseaseRecord> _plantDiseaseRecordRepository;
    private readonly IRepository<DiseasePhoto> _diseasePhotoRepository;
    private readonly IRepository<Disease> _diseaseRepository;
    private readonly IRepository<Fertilizer> _fertilizerRepository;

    #endregion

    #region Ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="PlantInstanceService" /> class.
    /// </summary>
    public PlantInstanceService(
        IRepository<PlantInstance> plantInstanceRepository,
        IRepository<Plant> plantRepository,
        IRepository<GardenSpot> gardenSpotRepository,
        IRepository<Container> containerRepository,
        IRepository<SoilMix> soilMixRepository,
        IRepository<WateringSchedule> wateringScheduleRepository,
        IRepository<FertilizingSchedule> fertilizingScheduleRepository,
        IRepository<PlantPhoto> plantPhotoRepository,
        IRepository<PlantNote> plantNoteRepository,
        IRepository<PlantDiseaseRecord> plantDiseaseRecordRepository,
        IRepository<DiseasePhoto> diseasePhotoRepository,
        IRepository<Disease> diseaseRepository,
        IRepository<Fertilizer> fertilizerRepository)
    {
        _plantInstanceRepository = plantInstanceRepository ?? throw new ArgumentNullException(nameof(plantInstanceRepository));
        _plantRepository = plantRepository ?? throw new ArgumentNullException(nameof(plantRepository));
        _gardenSpotRepository = gardenSpotRepository ?? throw new ArgumentNullException(nameof(gardenSpotRepository));
        _containerRepository = containerRepository ?? throw new ArgumentNullException(nameof(containerRepository));
        _soilMixRepository = soilMixRepository ?? throw new ArgumentNullException(nameof(soilMixRepository));
        _wateringScheduleRepository = wateringScheduleRepository ?? throw new ArgumentNullException(nameof(wateringScheduleRepository));
        _fertilizingScheduleRepository = fertilizingScheduleRepository ?? throw new ArgumentNullException(nameof(fertilizingScheduleRepository));
        _plantPhotoRepository = plantPhotoRepository ?? throw new ArgumentNullException(nameof(plantPhotoRepository));
        _plantNoteRepository = plantNoteRepository ?? throw new ArgumentNullException(nameof(plantNoteRepository));
        _plantDiseaseRecordRepository = plantDiseaseRecordRepository ?? throw new ArgumentNullException(nameof(plantDiseaseRecordRepository));
        _diseasePhotoRepository = diseasePhotoRepository ?? throw new ArgumentNullException(nameof(diseasePhotoRepository));
        _diseaseRepository = diseaseRepository ?? throw new ArgumentNullException(nameof(diseaseRepository));
        _fertilizerRepository = fertilizerRepository ?? throw new ArgumentNullException(nameof(fertilizerRepository));
    }

    #endregion

    #region Public

    /// <inheritdoc />
    public Task<IPagedList<PlantInstance>> GetPlantInstancesAsync(Guid ownerId, int? plantId = null, int? gardenSpotId = null, int? locationId = null, bool activeOnly = false, int pageIndex = 0, int pageSize = int.MaxValue, CancellationToken cancellationToken = default)
    {
        ValidateOwnerId(ownerId);

        return _plantInstanceRepository.GetPagedAsync(
            query =>
            {
                query = query.Where(instance => instance.OwnerId == ownerId);

                if (plantId.HasValue)
                    query = query.Where(instance => instance.PlantId == plantId.Value);

                if (gardenSpotId.HasValue)
                    query = query.Where(instance => instance.GardenSpotId == gardenSpotId.Value);

                if (locationId.HasValue)
                    query = query.Where(instance => instance.GardenSpot.LocationId == locationId.Value);

                if (activeOnly)
                    query = query.Where(instance => instance.IsActive);

                return query
                    .OrderBy(instance => instance.Nickname ?? string.Empty)
                    .ThenBy(instance => instance.Id);
            },
            pageIndex,
            pageSize,
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PlantInstance?> GetPlantInstanceByIdAsync(int plantInstanceId, Guid ownerId, bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        ValidateOwnerId(ownerId);

        var plantInstance = await _plantInstanceRepository.FirstOrDefaultAsync(
            entity => entity.Id == plantInstanceId && entity.OwnerId == ownerId,
            cancellationToken: cancellationToken);

        if (plantInstance is null || !includeDetails)
            return plantInstance;

        plantInstance.Photos = (await GetPlantPhotosAsync(plantInstanceId, ownerId, cancellationToken)).ToList();
        plantInstance.NotesEntries = (await GetPlantNotesAsync(plantInstanceId, ownerId, cancellationToken)).ToList();
        plantInstance.WateringSchedules = (await GetWateringSchedulesAsync(plantInstanceId, ownerId, cancellationToken)).ToList();
        plantInstance.FertilizingSchedules = (await GetFertilizingSchedulesAsync(plantInstanceId, ownerId, cancellationToken)).ToList();
        plantInstance.DiseaseRecords = (await GetDiseaseRecordsAsync(plantInstanceId, ownerId, includePhotos: true, cancellationToken)).ToList();

        if (plantInstance.ContainerId.HasValue)
            plantInstance.Container = await _containerRepository.FirstOrDefaultAsync(entity => entity.Id == plantInstance.ContainerId.Value && entity.OwnerId == ownerId, cancellationToken: cancellationToken);

        if (plantInstance.SoilMixId.HasValue)
            plantInstance.SoilMix = await _soilMixRepository.GetByIdAsync(plantInstance.SoilMixId.Value, cancellationToken: cancellationToken);

        plantInstance.Plant = await EnsurePlantExistsAsync(plantInstance.PlantId, cancellationToken);
        plantInstance.GardenSpot = await EnsureGardenSpotOwnedAsync(plantInstance.GardenSpotId, ownerId, cancellationToken);

        return plantInstance;
    }

    /// <inheritdoc />
    public async Task<PlantInstance> CreatePlantInstanceAsync(PlantInstance plantInstance, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(plantInstance);
        await ValidatePlantInstanceRelationshipsAsync(plantInstance, cancellationToken);

        plantInstance.Nickname = plantInstance.Nickname?.Trim();
        plantInstance.Notes = plantInstance.Notes?.Trim();
        StampForCreate(plantInstance);

        return await _plantInstanceRepository.CreateAsync(plantInstance, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PlantInstance> UpdatePlantInstanceAsync(PlantInstance plantInstance, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(plantInstance);

        var existingPlantInstance = await EnsurePlantInstanceOwnedAsync(plantInstance.Id, plantInstance.OwnerId, cancellationToken);
        await ValidatePlantInstanceRelationshipsAsync(plantInstance, cancellationToken);

        existingPlantInstance.PlantId = plantInstance.PlantId;
        existingPlantInstance.GardenSpotId = plantInstance.GardenSpotId;
        existingPlantInstance.ContainerId = plantInstance.ContainerId;
        existingPlantInstance.SoilMixId = plantInstance.SoilMixId;
        existingPlantInstance.Nickname = plantInstance.Nickname?.Trim();
        existingPlantInstance.PlantedDate = plantInstance.PlantedDate;
        existingPlantInstance.SizeCategory = plantInstance.SizeCategory;
        existingPlantInstance.HeightCm = plantInstance.HeightCm;
        existingPlantInstance.SpreadCm = plantInstance.SpreadCm;
        existingPlantInstance.HealthStatus = plantInstance.HealthStatus;
        existingPlantInstance.IsActive = plantInstance.IsActive;
        existingPlantInstance.Notes = plantInstance.Notes?.Trim();
        StampForUpdate(existingPlantInstance);

        return await _plantInstanceRepository.UpdateAsync(existingPlantInstance, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeletePlantInstanceAsync(int plantInstanceId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        var plantInstance = await EnsurePlantInstanceOwnedAsync(plantInstanceId, ownerId, cancellationToken);
        await _plantInstanceRepository.DeleteAsync(plantInstance, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Container>> GetContainersAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        ValidateOwnerId(ownerId);

        return _containerRepository.GetAllAsync(
            query => query.Where(container => container.OwnerId == ownerId).OrderBy(container => container.Id),
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Container> CreateContainerAsync(Container container, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(container);
        ValidateOwnerId(container.OwnerId);
        StampForCreate(container);

        return await _containerRepository.CreateAsync(container, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Container> UpdateContainerAsync(Container container, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(container);

        var existingContainer = await EnsureContainerOwnedAsync(container.Id, container.OwnerId, cancellationToken);
        existingContainer.ContainerType = container.ContainerType;
        existingContainer.Material = container.Material?.Trim();
        existingContainer.LengthCm = container.LengthCm;
        existingContainer.WidthCm = container.WidthCm;
        existingContainer.DepthCm = container.DepthCm;
        existingContainer.DiameterCm = container.DiameterCm;
        existingContainer.VolumeL = container.VolumeL;
        existingContainer.Color = container.Color?.Trim();
        existingContainer.HasDrainageHole = container.HasDrainageHole;
        existingContainer.Notes = container.Notes?.Trim();
        StampForUpdate(existingContainer);

        return await _containerRepository.UpdateAsync(existingContainer, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteContainerAsync(int containerId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        var container = await EnsureContainerOwnedAsync(containerId, ownerId, cancellationToken);
        var linkedCount = await _plantInstanceRepository.CountAsync(entity => entity.ContainerId == containerId && entity.OwnerId == ownerId, cancellationToken: cancellationToken);

        if (linkedCount > 0)
            throw new DomainException($"Container with id '{containerId}' cannot be deleted because it is referenced by plant instances.");

        await _containerRepository.DeleteAsync(container, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<SoilMix>> GetSoilMixesAsync(CancellationToken cancellationToken = default)
    {
        return _soilMixRepository.GetAllAsync(
            query => query.OrderBy(soilMix => soilMix.Name).ThenBy(soilMix => soilMix.Id),
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<SoilMix> CreateSoilMixAsync(SoilMix soilMix, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(soilMix);
        ValidateSoilMix(soilMix);
        StampForCreate(soilMix);

        return await _soilMixRepository.CreateAsync(soilMix, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<SoilMix> UpdateSoilMixAsync(SoilMix soilMix, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(soilMix);
        ValidateSoilMix(soilMix);

        var existingSoilMix = await EnsureSoilMixExistsAsync(soilMix.Id, cancellationToken);
        existingSoilMix.Name = soilMix.Name.Trim();
        existingSoilMix.Composition = soilMix.Composition?.Trim();
        existingSoilMix.PhMin = soilMix.PhMin;
        existingSoilMix.PhMax = soilMix.PhMax;
        existingSoilMix.Texture = soilMix.Texture;
        existingSoilMix.Drainage = soilMix.Drainage;
        existingSoilMix.Notes = soilMix.Notes?.Trim();
        StampForUpdate(existingSoilMix);

        return await _soilMixRepository.UpdateAsync(existingSoilMix, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteSoilMixAsync(int soilMixId, CancellationToken cancellationToken = default)
    {
        var soilMix = await EnsureSoilMixExistsAsync(soilMixId, cancellationToken);
        var linkedCount = await _plantInstanceRepository.CountAsync(entity => entity.SoilMixId == soilMixId, cancellationToken: cancellationToken);

        if (linkedCount > 0)
            throw new DomainException($"Soil mix with id '{soilMixId}' cannot be deleted because it is referenced by plant instances.");

        await _soilMixRepository.DeleteAsync(soilMix, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<WateringSchedule>> GetWateringSchedulesAsync(int plantInstanceId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        await EnsurePlantInstanceOwnedAsync(plantInstanceId, ownerId, cancellationToken);

        return await _wateringScheduleRepository.GetAllAsync(
            query => query
                .Where(schedule => schedule.PlantInstanceId == plantInstanceId && schedule.OwnerId == ownerId)
                .OrderBy(schedule => schedule.Season)
                .ThenBy(schedule => schedule.Id),
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<WateringSchedule> AddWateringScheduleAsync(int plantInstanceId, WateringSchedule schedule, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(schedule);
        ValidateScheduleFrequency(schedule.FrequencyDays, nameof(schedule.FrequencyDays));

        var plantInstance = await EnsurePlantInstanceOwnedAsync(plantInstanceId, schedule.OwnerId, cancellationToken);
        schedule.PlantInstanceId = plantInstanceId;
        schedule.OwnerId = plantInstance.OwnerId;
        schedule.Notes = schedule.Notes?.Trim();
        StampForCreate(schedule);

        return await _wateringScheduleRepository.CreateAsync(schedule, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<WateringSchedule> UpdateWateringScheduleAsync(int plantInstanceId, WateringSchedule schedule, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(schedule);
        ValidateScheduleFrequency(schedule.FrequencyDays, nameof(schedule.FrequencyDays));

        await EnsurePlantInstanceOwnedAsync(plantInstanceId, schedule.OwnerId, cancellationToken);
        var existingSchedule = await EnsureWateringScheduleOwnedAsync(plantInstanceId, schedule.Id, schedule.OwnerId, cancellationToken);
        existingSchedule.Season = schedule.Season;
        existingSchedule.FrequencyDays = schedule.FrequencyDays;
        existingSchedule.WaterAmountL = schedule.WaterAmountL;
        existingSchedule.TimeOfDay = schedule.TimeOfDay;
        existingSchedule.Notes = schedule.Notes?.Trim();
        StampForUpdate(existingSchedule);

        return await _wateringScheduleRepository.UpdateAsync(existingSchedule, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteWateringScheduleAsync(int plantInstanceId, int scheduleId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        await EnsurePlantInstanceOwnedAsync(plantInstanceId, ownerId, cancellationToken);
        var schedule = await EnsureWateringScheduleOwnedAsync(plantInstanceId, scheduleId, ownerId, cancellationToken);
        await _wateringScheduleRepository.DeleteAsync(schedule, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<FertilizingSchedule>> GetFertilizingSchedulesAsync(int plantInstanceId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        await EnsurePlantInstanceOwnedAsync(plantInstanceId, ownerId, cancellationToken);

        return await _fertilizingScheduleRepository.GetAllAsync(
            query => query
                .Where(schedule => schedule.PlantInstanceId == plantInstanceId && schedule.OwnerId == ownerId)
                .OrderBy(schedule => schedule.Season)
                .ThenBy(schedule => schedule.Id),
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<FertilizingSchedule> AddFertilizingScheduleAsync(int plantInstanceId, FertilizingSchedule schedule, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(schedule);
        ValidateScheduleFrequency(schedule.FrequencyDays, nameof(schedule.FrequencyDays));
        await EnsureFertilizerExistsAsync(schedule.FertilizerId, cancellationToken);

        var plantInstance = await EnsurePlantInstanceOwnedAsync(plantInstanceId, schedule.OwnerId, cancellationToken);
        schedule.PlantInstanceId = plantInstanceId;
        schedule.OwnerId = plantInstance.OwnerId;
        schedule.Notes = schedule.Notes?.Trim();
        StampForCreate(schedule);

        return await _fertilizingScheduleRepository.CreateAsync(schedule, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<FertilizingSchedule> UpdateFertilizingScheduleAsync(int plantInstanceId, FertilizingSchedule schedule, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(schedule);
        ValidateScheduleFrequency(schedule.FrequencyDays, nameof(schedule.FrequencyDays));
        await EnsureFertilizerExistsAsync(schedule.FertilizerId, cancellationToken);

        await EnsurePlantInstanceOwnedAsync(plantInstanceId, schedule.OwnerId, cancellationToken);
        var existingSchedule = await EnsureFertilizingScheduleOwnedAsync(plantInstanceId, schedule.Id, schedule.OwnerId, cancellationToken);
        existingSchedule.FertilizerId = schedule.FertilizerId;
        existingSchedule.Season = schedule.Season;
        existingSchedule.FrequencyDays = schedule.FrequencyDays;
        existingSchedule.Quantity = schedule.Quantity;
        existingSchedule.Unit = schedule.Unit;
        existingSchedule.Notes = schedule.Notes?.Trim();
        StampForUpdate(existingSchedule);

        return await _fertilizingScheduleRepository.UpdateAsync(existingSchedule, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteFertilizingScheduleAsync(int plantInstanceId, int scheduleId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        await EnsurePlantInstanceOwnedAsync(plantInstanceId, ownerId, cancellationToken);
        var schedule = await EnsureFertilizingScheduleOwnedAsync(plantInstanceId, scheduleId, ownerId, cancellationToken);
        await _fertilizingScheduleRepository.DeleteAsync(schedule, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PlantPhoto>> GetPlantPhotosAsync(int plantInstanceId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        await EnsurePlantInstanceOwnedAsync(plantInstanceId, ownerId, cancellationToken);

        return await _plantPhotoRepository.GetAllAsync(
            query => query
                .Where(photo => photo.PlantInstanceId == plantInstanceId && photo.OwnerId == ownerId)
                .OrderBy(photo => photo.SortOrder)
                .ThenBy(photo => photo.Id),
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PlantPhoto> AddPlantPhotoAsync(int plantInstanceId, PlantPhoto photo, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(photo);
        ValidateRequired(photo.FilePath, nameof(photo.FilePath));

        var plantInstance = await EnsurePlantInstanceOwnedAsync(plantInstanceId, photo.OwnerId, cancellationToken);
        photo.PlantInstanceId = plantInstanceId;
        photo.OwnerId = plantInstance.OwnerId;
        photo.FilePath = photo.FilePath.Trim();
        photo.ThumbnailPath = photo.ThumbnailPath?.Trim();
        photo.Caption = photo.Caption?.Trim();
        StampForCreate(photo);

        return await _plantPhotoRepository.CreateAsync(photo, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PlantPhoto> UpdatePlantPhotoAsync(int plantInstanceId, PlantPhoto photo, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(photo);
        ValidateRequired(photo.FilePath, nameof(photo.FilePath));

        await EnsurePlantInstanceOwnedAsync(plantInstanceId, photo.OwnerId, cancellationToken);
        var existingPhoto = await EnsurePlantPhotoOwnedAsync(plantInstanceId, photo.Id, photo.OwnerId, cancellationToken);
        existingPhoto.FilePath = photo.FilePath.Trim();
        existingPhoto.ThumbnailPath = photo.ThumbnailPath?.Trim();
        existingPhoto.TakenDate = photo.TakenDate;
        existingPhoto.Caption = photo.Caption?.Trim();
        existingPhoto.SortOrder = photo.SortOrder;
        StampForUpdate(existingPhoto);

        return await _plantPhotoRepository.UpdateAsync(existingPhoto, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeletePlantPhotoAsync(int plantInstanceId, int photoId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        await EnsurePlantInstanceOwnedAsync(plantInstanceId, ownerId, cancellationToken);
        var photo = await EnsurePlantPhotoOwnedAsync(plantInstanceId, photoId, ownerId, cancellationToken);
        await _plantPhotoRepository.DeleteAsync(photo, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PlantNote>> GetPlantNotesAsync(int plantInstanceId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        await EnsurePlantInstanceOwnedAsync(plantInstanceId, ownerId, cancellationToken);

        return await _plantNoteRepository.GetAllAsync(
            query => query.Where(note => note.PlantInstanceId == plantInstanceId && note.OwnerId == ownerId).OrderByDescending(note => note.Id),
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PlantNote> AddPlantNoteAsync(int plantInstanceId, PlantNote note, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(note);
        ValidateRequired(note.RichHtmlContent, nameof(note.RichHtmlContent));

        var plantInstance = await EnsurePlantInstanceOwnedAsync(plantInstanceId, note.OwnerId, cancellationToken);
        note.PlantInstanceId = plantInstanceId;
        note.OwnerId = plantInstance.OwnerId;
        note.Title = note.Title?.Trim();
        note.RichHtmlContent = note.RichHtmlContent.Trim();
        note.Tags = note.Tags?.Trim();
        StampForCreate(note);

        return await _plantNoteRepository.CreateAsync(note, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PlantNote> UpdatePlantNoteAsync(int plantInstanceId, PlantNote note, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(note);
        ValidateRequired(note.RichHtmlContent, nameof(note.RichHtmlContent));

        await EnsurePlantInstanceOwnedAsync(plantInstanceId, note.OwnerId, cancellationToken);
        var existingNote = await EnsurePlantNoteOwnedAsync(plantInstanceId, note.Id, note.OwnerId, cancellationToken);
        existingNote.Title = note.Title?.Trim();
        existingNote.RichHtmlContent = note.RichHtmlContent.Trim();
        existingNote.Tags = note.Tags?.Trim();
        StampForUpdate(existingNote);

        return await _plantNoteRepository.UpdateAsync(existingNote, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeletePlantNoteAsync(int plantInstanceId, int noteId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        await EnsurePlantInstanceOwnedAsync(plantInstanceId, ownerId, cancellationToken);
        var note = await EnsurePlantNoteOwnedAsync(plantInstanceId, noteId, ownerId, cancellationToken);
        await _plantNoteRepository.DeleteAsync(note, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PlantDiseaseRecord>> GetDiseaseRecordsAsync(int plantInstanceId, Guid ownerId, bool includePhotos = false, CancellationToken cancellationToken = default)
    {
        await EnsurePlantInstanceOwnedAsync(plantInstanceId, ownerId, cancellationToken);

        var records = await _plantDiseaseRecordRepository.GetAllAsync(
            query => query.Where(record => record.PlantInstanceId == plantInstanceId && record.OwnerId == ownerId).OrderByDescending(record => record.DetectedDate).ThenBy(record => record.Id),
            cancellationToken: cancellationToken);

        if (!includePhotos)
            return records;

        foreach (var record in records)
        {
            record.Photos = (await _diseasePhotoRepository.GetAllAsync(
                query => query.Where(photo => photo.PlantDiseaseRecordId == record.Id && photo.OwnerId == ownerId).OrderBy(photo => photo.Id),
                cancellationToken: cancellationToken)).ToList();
        }

        return records;
    }

    /// <inheritdoc />
    public async Task<PlantDiseaseRecord> AddDiseaseRecordAsync(int plantInstanceId, PlantDiseaseRecord record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);
        ValidateDiseaseRecord(record);
        await EnsureDiseaseExistsAsync(record.DiseaseId, cancellationToken);

        var plantInstance = await EnsurePlantInstanceOwnedAsync(plantInstanceId, record.OwnerId, cancellationToken);
        record.PlantInstanceId = plantInstanceId;
        record.OwnerId = plantInstance.OwnerId;
        record.TreatmentUsed = record.TreatmentUsed?.Trim();
        record.Notes = record.Notes?.Trim();
        StampForCreate(record);

        return await _plantDiseaseRecordRepository.CreateAsync(record, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PlantDiseaseRecord> UpdateDiseaseRecordAsync(int plantInstanceId, PlantDiseaseRecord record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);
        ValidateDiseaseRecord(record);
        await EnsureDiseaseExistsAsync(record.DiseaseId, cancellationToken);

        await EnsurePlantInstanceOwnedAsync(plantInstanceId, record.OwnerId, cancellationToken);
        var existingRecord = await EnsureDiseaseRecordOwnedAsync(plantInstanceId, record.Id, record.OwnerId, cancellationToken);
        existingRecord.DiseaseId = record.DiseaseId;
        existingRecord.DetectedDate = record.DetectedDate;
        existingRecord.ResolvedDate = record.ResolvedDate;
        existingRecord.Severity = record.Severity;
        existingRecord.TreatmentUsed = record.TreatmentUsed?.Trim();
        existingRecord.Outcome = record.Outcome;
        existingRecord.Notes = record.Notes?.Trim();
        StampForUpdate(existingRecord);

        return await _plantDiseaseRecordRepository.UpdateAsync(existingRecord, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteDiseaseRecordAsync(int plantInstanceId, int recordId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        await EnsurePlantInstanceOwnedAsync(plantInstanceId, ownerId, cancellationToken);
        var record = await EnsureDiseaseRecordOwnedAsync(plantInstanceId, recordId, ownerId, cancellationToken);
        await _plantDiseaseRecordRepository.DeleteAsync(record, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DiseasePhoto>> GetDiseasePhotosAsync(int plantInstanceId, int recordId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        await EnsureDiseaseRecordOwnedAsync(plantInstanceId, recordId, ownerId, cancellationToken);

        return await _diseasePhotoRepository.GetAllAsync(
            query => query.Where(photo => photo.PlantDiseaseRecordId == recordId && photo.OwnerId == ownerId).OrderBy(photo => photo.Id),
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<DiseasePhoto> AddDiseasePhotoAsync(int plantInstanceId, int recordId, DiseasePhoto photo, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(photo);
        ValidateRequired(photo.FilePath, nameof(photo.FilePath));

        var record = await EnsureDiseaseRecordOwnedAsync(plantInstanceId, recordId, photo.OwnerId, cancellationToken);
        photo.PlantDiseaseRecordId = record.Id;
        photo.OwnerId = record.OwnerId;
        photo.FilePath = photo.FilePath.Trim();
        photo.ThumbnailPath = photo.ThumbnailPath?.Trim();
        photo.Notes = photo.Notes?.Trim();
        StampForCreate(photo);

        return await _diseasePhotoRepository.CreateAsync(photo, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<DiseasePhoto> UpdateDiseasePhotoAsync(int plantInstanceId, int recordId, DiseasePhoto photo, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(photo);
        ValidateRequired(photo.FilePath, nameof(photo.FilePath));

        await EnsureDiseaseRecordOwnedAsync(plantInstanceId, recordId, photo.OwnerId, cancellationToken);
        var existingPhoto = await EnsureDiseasePhotoOwnedAsync(recordId, photo.Id, photo.OwnerId, cancellationToken);
        existingPhoto.FilePath = photo.FilePath.Trim();
        existingPhoto.ThumbnailPath = photo.ThumbnailPath?.Trim();
        existingPhoto.TakenDate = photo.TakenDate;
        existingPhoto.Notes = photo.Notes?.Trim();
        StampForUpdate(existingPhoto);

        return await _diseasePhotoRepository.UpdateAsync(existingPhoto, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteDiseasePhotoAsync(int plantInstanceId, int recordId, int photoId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        await EnsureDiseaseRecordOwnedAsync(plantInstanceId, recordId, ownerId, cancellationToken);
        var photo = await EnsureDiseasePhotoOwnedAsync(recordId, photoId, ownerId, cancellationToken);
        await _diseasePhotoRepository.DeleteAsync(photo, cancellationToken: cancellationToken);
    }

    #endregion

    #region Privates

    private async Task ValidatePlantInstanceRelationshipsAsync(PlantInstance plantInstance, CancellationToken cancellationToken)
    {
        ValidateOwnerId(plantInstance.OwnerId);
        await EnsurePlantExistsAsync(plantInstance.PlantId, cancellationToken);

        var gardenSpot = await EnsureGardenSpotOwnedAsync(plantInstance.GardenSpotId, plantInstance.OwnerId, cancellationToken);
        if (gardenSpot.OwnerId != plantInstance.OwnerId)
            throw new DomainException("PlantInstance owner must match the selected GardenSpot owner.");

        if (plantInstance.ContainerId.HasValue)
            await EnsureContainerOwnedAsync(plantInstance.ContainerId.Value, plantInstance.OwnerId, cancellationToken);

        if (plantInstance.SoilMixId.HasValue)
            await EnsureSoilMixExistsAsync(plantInstance.SoilMixId.Value, cancellationToken);
    }

    private async Task<Plant> EnsurePlantExistsAsync(int plantId, CancellationToken cancellationToken)
    {
        var plant = await _plantRepository.GetByIdAsync(plantId, cancellationToken: cancellationToken);
        return plant ?? throw new DomainException($"Plant with id '{plantId}' was not found.");
    }

    private async Task<GardenSpot> EnsureGardenSpotOwnedAsync(int gardenSpotId, Guid ownerId, CancellationToken cancellationToken)
    {
        ValidateOwnerId(ownerId);

        var gardenSpot = await _gardenSpotRepository.FirstOrDefaultAsync(
            entity => entity.Id == gardenSpotId && entity.OwnerId == ownerId,
            cancellationToken: cancellationToken);

        return gardenSpot ?? throw new DomainException($"GardenSpot with id '{gardenSpotId}' was not found for owner '{ownerId}'.");
    }

    private async Task<PlantInstance> EnsurePlantInstanceOwnedAsync(int plantInstanceId, Guid ownerId, CancellationToken cancellationToken)
    {
        ValidateOwnerId(ownerId);

        var plantInstance = await _plantInstanceRepository.FirstOrDefaultAsync(
            entity => entity.Id == plantInstanceId && entity.OwnerId == ownerId,
            cancellationToken: cancellationToken);

        return plantInstance ?? throw new DomainException($"PlantInstance with id '{plantInstanceId}' was not found for owner '{ownerId}'.");
    }

    private async Task<Container> EnsureContainerOwnedAsync(int containerId, Guid ownerId, CancellationToken cancellationToken)
    {
        ValidateOwnerId(ownerId);

        var container = await _containerRepository.FirstOrDefaultAsync(
            entity => entity.Id == containerId && entity.OwnerId == ownerId,
            cancellationToken: cancellationToken);

        return container ?? throw new DomainException($"Container with id '{containerId}' was not found for owner '{ownerId}'.");
    }

    private async Task<SoilMix> EnsureSoilMixExistsAsync(int soilMixId, CancellationToken cancellationToken)
    {
        var soilMix = await _soilMixRepository.GetByIdAsync(soilMixId, cancellationToken: cancellationToken);
        return soilMix ?? throw new DomainException($"SoilMix with id '{soilMixId}' was not found.");
    }

    private async Task<WateringSchedule> EnsureWateringScheduleOwnedAsync(int plantInstanceId, int scheduleId, Guid ownerId, CancellationToken cancellationToken)
    {
        var schedule = await _wateringScheduleRepository.FirstOrDefaultAsync(
            entity => entity.Id == scheduleId && entity.PlantInstanceId == plantInstanceId && entity.OwnerId == ownerId,
            cancellationToken: cancellationToken);

        return schedule ?? throw new DomainException($"WateringSchedule with id '{scheduleId}' was not found for plant instance '{plantInstanceId}'.");
    }

    private async Task<FertilizingSchedule> EnsureFertilizingScheduleOwnedAsync(int plantInstanceId, int scheduleId, Guid ownerId, CancellationToken cancellationToken)
    {
        var schedule = await _fertilizingScheduleRepository.FirstOrDefaultAsync(
            entity => entity.Id == scheduleId && entity.PlantInstanceId == plantInstanceId && entity.OwnerId == ownerId,
            cancellationToken: cancellationToken);

        return schedule ?? throw new DomainException($"FertilizingSchedule with id '{scheduleId}' was not found for plant instance '{plantInstanceId}'.");
    }

    private async Task<PlantPhoto> EnsurePlantPhotoOwnedAsync(int plantInstanceId, int photoId, Guid ownerId, CancellationToken cancellationToken)
    {
        var photo = await _plantPhotoRepository.FirstOrDefaultAsync(
            entity => entity.Id == photoId && entity.PlantInstanceId == plantInstanceId && entity.OwnerId == ownerId,
            cancellationToken: cancellationToken);

        return photo ?? throw new DomainException($"PlantPhoto with id '{photoId}' was not found for plant instance '{plantInstanceId}'.");
    }

    private async Task<PlantNote> EnsurePlantNoteOwnedAsync(int plantInstanceId, int noteId, Guid ownerId, CancellationToken cancellationToken)
    {
        var note = await _plantNoteRepository.FirstOrDefaultAsync(
            entity => entity.Id == noteId && entity.PlantInstanceId == plantInstanceId && entity.OwnerId == ownerId,
            cancellationToken: cancellationToken);

        return note ?? throw new DomainException($"PlantNote with id '{noteId}' was not found for plant instance '{plantInstanceId}'.");
    }

    private async Task<PlantDiseaseRecord> EnsureDiseaseRecordOwnedAsync(int plantInstanceId, int recordId, Guid ownerId, CancellationToken cancellationToken)
    {
        var record = await _plantDiseaseRecordRepository.FirstOrDefaultAsync(
            entity => entity.Id == recordId && entity.PlantInstanceId == plantInstanceId && entity.OwnerId == ownerId,
            cancellationToken: cancellationToken);

        return record ?? throw new DomainException($"PlantDiseaseRecord with id '{recordId}' was not found for plant instance '{plantInstanceId}'.");
    }

    private async Task<DiseasePhoto> EnsureDiseasePhotoOwnedAsync(int recordId, int photoId, Guid ownerId, CancellationToken cancellationToken)
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

    private static void ValidateRequired(string value, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"{propertyName} is required.");
    }

    private static void ValidateScheduleFrequency(byte frequencyDays, string propertyName)
    {
        if (frequencyDays == 0)
            throw new DomainException($"{propertyName} must be greater than zero.");
    }

    private static void ValidateSoilMix(SoilMix soilMix)
    {
        ValidateRequired(soilMix.Name, nameof(soilMix.Name));

        if (soilMix.PhMin.HasValue && soilMix.PhMax.HasValue && soilMix.PhMin.Value > soilMix.PhMax.Value)
            throw new DomainException("PhMin cannot be greater than PhMax.");
    }

    private static void ValidateDiseaseRecord(PlantDiseaseRecord record)
    {
        ValidateOwnerId(record.OwnerId);

        if (record.ResolvedDate.HasValue && record.ResolvedDate.Value < record.DetectedDate)
            throw new DomainException("ResolvedDate cannot be earlier than DetectedDate.");
    }

    private static void StampForCreate(AuditableEntity entity)
    {
        var now = DateTime.UtcNow;
        entity.CreatedAtUtc = now;
        entity.UpdatedAtUtc = now;
        entity.IsDeleted = false;
        entity.DeletedAtUtc = null;
    }

    private static void StampForUpdate(AuditableEntity entity)
    {
        entity.UpdatedAtUtc = DateTime.UtcNow;
    }

    #endregion
}


