using GropMng.Core;
using GropMng.Core.Common.Exceptions;
using GropMng.Core.Domain.Garden;
using GropMng.Core.Domain.Garden.Care;
using GropMng.Core.Domain.Garden.Locations;
using GropMng.Core.Domain.Garden.Plants;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.Garden.Care;
using GropMng.Core.Interfaces.Services.Garden.Health;
using GropMng.Core.Interfaces.Services.Garden.Plants;

namespace GropMng.Services.Services.Garden.Plants;

/// <summary>
/// Provides aggregate-root operations for plant instances.
/// </summary>
public class PlantInstanceService : IPlantInstanceService
{
    #region Fields

    private readonly IRepository<PlantInstance> _plantInstanceRepository;
    private readonly IRepository<Plant> _plantRepository;
    private readonly IRepository<GardenSpot> _gardenSpotRepository;
    private readonly IRepository<Container> _containerRepository;
    private readonly IRepository<SoilMix> _soilMixRepository;
    private readonly IRepository<RepottingLog> _repottingLogRepository;
    private readonly IContainerService _containerService;
    private readonly ISoilMixService _soilMixService;
    private readonly IWateringService _wateringService;
    private readonly IFertilizingService _fertilizingService;
    private readonly IPlantPhotoService _plantPhotoService;
    private readonly IPlantNoteService _plantNoteService;
    private readonly IPlantDiseaseService _plantDiseaseService;

    #endregion

    #region Ctor

    public PlantInstanceService(
        IRepository<PlantInstance> plantInstanceRepository,
        IRepository<Plant> plantRepository,
        IRepository<GardenSpot> gardenSpotRepository,
        IRepository<Container> containerRepository,
        IRepository<SoilMix> soilMixRepository,
        IRepository<RepottingLog> repottingLogRepository,
        IContainerService containerService,
        ISoilMixService soilMixService,
        IWateringService wateringService,
        IFertilizingService fertilizingService,
        IPlantPhotoService plantPhotoService,
        IPlantNoteService plantNoteService,
        IPlantDiseaseService plantDiseaseService)
    {
        _plantInstanceRepository = plantInstanceRepository ?? throw new ArgumentNullException(nameof(plantInstanceRepository));
        _plantRepository = plantRepository ?? throw new ArgumentNullException(nameof(plantRepository));
        _gardenSpotRepository = gardenSpotRepository ?? throw new ArgumentNullException(nameof(gardenSpotRepository));
        _containerRepository = containerRepository ?? throw new ArgumentNullException(nameof(containerRepository));
        _soilMixRepository = soilMixRepository ?? throw new ArgumentNullException(nameof(soilMixRepository));
        _repottingLogRepository = repottingLogRepository ?? throw new ArgumentNullException(nameof(repottingLogRepository));
        _containerService = containerService ?? throw new ArgumentNullException(nameof(containerService));
        _soilMixService = soilMixService ?? throw new ArgumentNullException(nameof(soilMixService));
        _wateringService = wateringService ?? throw new ArgumentNullException(nameof(wateringService));
        _fertilizingService = fertilizingService ?? throw new ArgumentNullException(nameof(fertilizingService));
        _plantPhotoService = plantPhotoService ?? throw new ArgumentNullException(nameof(plantPhotoService));
        _plantNoteService = plantNoteService ?? throw new ArgumentNullException(nameof(plantNoteService));
        _plantDiseaseService = plantDiseaseService ?? throw new ArgumentNullException(nameof(plantDiseaseService));
    }

    #endregion

    #region Public

    /// <inheritdoc />
    public Task<IPagedList<PlantInstance>> GetPlantInstancesAsync(
        Guid ownerId,
        int? plantId = null,
        int? gardenSpotId = null,
        int? locationId = null,
        bool activeOnly = false,
        int pageIndex = 0,
        int pageSize = int.MaxValue,
        CancellationToken cancellationToken = default
    )
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
    public async Task<PlantInstance?> GetPlantInstanceByIdAsync(
        int plantInstanceId,
        Guid ownerId,
        bool includeDetails = false,
        CancellationToken cancellationToken = default
    )
    {
        ValidateOwnerId(ownerId);

        var plantInstance = await _plantInstanceRepository.FirstOrDefaultAsync(
            entity => entity.Id == plantInstanceId && entity.OwnerId == ownerId,
            cancellationToken: cancellationToken);

        if (plantInstance is null || !includeDetails)
            return plantInstance;

        plantInstance.Photos = (await _plantPhotoService.GetPhotosAsync(plantInstanceId, ownerId, cancellationToken)).ToList();
        plantInstance.NotesEntries = (await _plantNoteService.GetNotesAsync(plantInstanceId, ownerId, cancellationToken)).ToList();
        plantInstance.WateringSchedules = (await _wateringService.GetSchedulesAsync(plantInstanceId, ownerId, cancellationToken)).ToList();
        plantInstance.FertilizingSchedules = (await _fertilizingService.GetSchedulesAsync(plantInstanceId, ownerId, cancellationToken)).ToList();
        plantInstance.DiseaseRecords = (await _plantDiseaseService.GetRecordsAsync(plantInstanceId, ownerId, includePhotos: true, cancellationToken)).ToList();

        plantInstance.Container = await GetCurrentContainerAsync(plantInstanceId, ownerId, cancellationToken);

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

        var requestedContainerId = plantInstance.ContainerId;
        plantInstance.Nickname = plantInstance.Nickname?.Trim();
        plantInstance.Notes = plantInstance.Notes?.Trim();
        StampForCreate(plantInstance);

        var createdPlantInstance = await _plantInstanceRepository.CreateAsync(plantInstance, saveNow: false, cancellationToken: cancellationToken);

        await _plantInstanceRepository.SaveChangesAsync(cancellationToken);

        await SyncContainerAssignmentAsync(createdPlantInstance.Id, createdPlantInstance.OwnerId, requestedContainerId, cancellationToken);
        await _plantInstanceRepository.SaveChangesAsync(cancellationToken);

        return createdPlantInstance;
    }

    /// <inheritdoc />
    public async Task<PlantInstance> UpdatePlantInstanceAsync(PlantInstance plantInstance, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(plantInstance);

        var requestedContainerId = plantInstance.ContainerId;
        var existingPlantInstance = await EnsurePlantInstanceOwnedAsync(plantInstance.Id, plantInstance.OwnerId, cancellationToken);
        await ValidatePlantInstanceRelationshipsAsync(plantInstance, cancellationToken);

        existingPlantInstance.PlantId = plantInstance.PlantId;
        existingPlantInstance.GardenSpotId = plantInstance.GardenSpotId;
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

        await SyncContainerAssignmentAsync(existingPlantInstance.Id, existingPlantInstance.OwnerId, requestedContainerId, cancellationToken);

        var updatedPlantInstance = await _plantInstanceRepository.UpdateAsync(existingPlantInstance, saveNow: false, cancellationToken: cancellationToken);
        await _plantInstanceRepository.SaveChangesAsync(cancellationToken);

        return updatedPlantInstance;
    }

    /// <inheritdoc />
    public async Task DeletePlantInstanceAsync(int plantInstanceId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        var plantInstance = await EnsurePlantInstanceOwnedAsync(plantInstanceId, ownerId, cancellationToken);
        await _plantInstanceRepository.DeleteAsync(plantInstance, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<RepottingLog> RepotPlantAsync(int plantInstanceId, RepottingLog repottingLog, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(repottingLog);
        ValidateOwnerId(repottingLog.OwnerId);

        var plantInstance = await EnsurePlantInstanceOwnedAsync(plantInstanceId, repottingLog.OwnerId, cancellationToken);

        if (repottingLog.NewContainerId.HasValue)
            await EnsureContainerOwnedAsync(repottingLog.NewContainerId.Value, repottingLog.OwnerId, cancellationToken);

        if (repottingLog.NewSoilMixId.HasValue)
            await EnsureSoilMixExistsAsync(repottingLog.NewSoilMixId.Value, cancellationToken);

        var currentContainer = await GetCurrentContainerAsync(plantInstanceId, repottingLog.OwnerId, cancellationToken);

        repottingLog.PlantInstanceId = plantInstanceId;
        repottingLog.OwnerId = plantInstance.OwnerId;
        repottingLog.PreviousContainerId = currentContainer?.Id;
        repottingLog.PreviousSoilMixId = plantInstance.SoilMixId;
        repottingLog.ContainerChanged = repottingLog.NewContainerId != repottingLog.PreviousContainerId;
        repottingLog.SoilMixChanged = repottingLog.NewSoilMixId != repottingLog.PreviousSoilMixId;
        repottingLog.Notes = repottingLog.Notes?.Trim();

        if (!repottingLog.ContainerChanged && !repottingLog.SoilMixChanged)
            throw new DomainException("No container or soil mix change detected for repotting.");

        if (repottingLog.RepottedAtUtc == default)
            repottingLog.RepottedAtUtc = DateTime.UtcNow;

        plantInstance.SoilMixId = repottingLog.NewSoilMixId;

        StampForCreate(repottingLog);
        StampForUpdate(plantInstance);

        var created = await _repottingLogRepository.CreateAsync(repottingLog, saveNow: false, cancellationToken: cancellationToken);
        await SyncContainerAssignmentAsync(plantInstance.Id, plantInstance.OwnerId, repottingLog.NewContainerId, cancellationToken);
        await _plantInstanceRepository.UpdateAsync(plantInstance, saveNow: false, cancellationToken: cancellationToken);
        await _plantInstanceRepository.SaveChangesAsync(cancellationToken);

        return created;
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

    private async Task<Container?> GetCurrentContainerAsync(int plantInstanceId, Guid ownerId, CancellationToken cancellationToken)
    {
        ValidateOwnerId(ownerId);

        return await _containerRepository.FirstOrDefaultAsync(
            entity => entity.PlantInstanceId == plantInstanceId && entity.OwnerId == ownerId,
            cancellationToken: cancellationToken);
    }

    private async Task SyncContainerAssignmentAsync(int plantInstanceId, Guid ownerId, int? newContainerId, CancellationToken cancellationToken)
    {
        var currentContainer = await GetCurrentContainerAsync(plantInstanceId, ownerId, cancellationToken);
        if (currentContainer?.Id == newContainerId)
            return;

        Container? requestedContainer = null;
        if (newContainerId.HasValue)
        {
            requestedContainer = await EnsureContainerOwnedAsync(newContainerId.Value, ownerId, cancellationToken);
            if (requestedContainer.PlantInstanceId.HasValue && requestedContainer.PlantInstanceId.Value != plantInstanceId)
                throw new DomainException($"Container with id '{newContainerId.Value}' is already assigned to another plant instance.");
        }

        if (currentContainer is not null)
        {
            currentContainer.PlantInstanceId = null;
            StampForUpdate(currentContainer);
            await _containerRepository.UpdateAsync(currentContainer, saveNow: true, cancellationToken: cancellationToken);
        }

        if (requestedContainer is not null)
        {
            requestedContainer.PlantInstanceId = plantInstanceId;
            StampForUpdate(requestedContainer);
            await _containerRepository.UpdateAsync(requestedContainer, saveNow: false, cancellationToken: cancellationToken);
        }
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

    private static void ValidateOwnerId(Guid ownerId)
    {
        if (ownerId == Guid.Empty)
            throw new DomainException("OwnerId is required.");
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