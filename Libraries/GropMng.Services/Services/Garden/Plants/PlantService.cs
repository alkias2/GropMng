using GropMng.Core;
using GropMng.Core.Common.Exceptions;
using GropMng.Core.Domain.Garden;
using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Plants;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.Garden.Plants;

namespace GropMng.Services.Services.Garden.Plants;

/// <summary>
/// Provides aggregate-root operations for the plant catalog.
/// </summary>
public class PlantService : IPlantService
{
    #region Fields

    private readonly IRepository<Plant> _plantRepository;
    private readonly IRepository<PlantInstance> _plantInstanceRepository;

    #endregion

    #region Ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="PlantService" /> class.
    /// </summary>
    /// <param name="plantRepository">The repository used to manage plant catalog entities.</param>
    /// <param name="plantInstanceRepository">The repository used to validate dependent plant instances.</param>
    public PlantService(IRepository<Plant> plantRepository, IRepository<PlantInstance> plantInstanceRepository)
    {
        _plantRepository = plantRepository ?? throw new ArgumentNullException(nameof(plantRepository));
        _plantInstanceRepository = plantInstanceRepository ?? throw new ArgumentNullException(nameof(plantInstanceRepository));
    }

    #endregion

    #region Public

    /// <inheritdoc />
    public Task<IPagedList<Plant>> GetPlantsAsync(string? searchTerm = null, PlantCategory? category = null, int pageIndex = 0, int pageSize = int.MaxValue, CancellationToken cancellationToken = default)
    {
        return _plantRepository.GetPagedAsync(
            query =>
            {
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var term = searchTerm.Trim().ToLowerInvariant();
                    query = query.Where(plant =>
                        plant.CommonName.ToLower().Contains(term)
                        || plant.ScientificName.ToLower().Contains(term)
                        || (plant.Family != null && plant.Family.ToLower().Contains(term)));
                }

                if (category.HasValue)
                    query = query.Where(plant => plant.Category == category.Value);

                return query
                    .OrderBy(plant => plant.CommonName)
                    .ThenBy(plant => plant.ScientificName)
                    .ThenBy(plant => plant.Id);
            },
            pageIndex,
            pageSize,
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Plant?> GetPlantByIdAsync(int plantId, bool includeInstances = false, CancellationToken cancellationToken = default)
    {
        var plant = await _plantRepository.GetByIdAsync(plantId, cancellationToken: cancellationToken);
        if (plant is null || !includeInstances)
            return plant;

        plant.PlantInstances = (await _plantInstanceRepository.GetAllAsync(
            query => query.Where(instance => instance.PlantId == plantId).OrderBy(instance => instance.Id),
            cancellationToken: cancellationToken)).ToList();

        return plant;
    }

    /// <inheritdoc />
    public async Task<Plant> CreatePlantAsync(Plant plant, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(plant);
        ValidatePlant(plant);
        await EnsureScientificNameIsUniqueAsync(plant.ScientificName, null, cancellationToken);

        StampForCreate(plant);
        return await _plantRepository.CreateAsync(plant, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Plant> UpdatePlantAsync(Plant plant, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(plant);
        ValidatePlant(plant);

        var existingPlant = await EnsurePlantExistsAsync(plant.Id, cancellationToken);
        await EnsureScientificNameIsUniqueAsync(plant.ScientificName, plant.Id, cancellationToken);

        existingPlant.CommonName = plant.CommonName.Trim();
        existingPlant.ScientificName = plant.ScientificName.Trim();
        existingPlant.Family = plant.Family?.Trim();
        existingPlant.Category = plant.Category;
        existingPlant.GrowthType = plant.GrowthType;
        existingPlant.SunRequirement = plant.SunRequirement;
        existingPlant.WaterRequirement = plant.WaterRequirement;
        existingPlant.MinTempCelsius = plant.MinTempCelsius;
        existingPlant.MaxTempCelsius = plant.MaxTempCelsius;
        existingPlant.IsEdible = plant.IsEdible;
        existingPlant.IsMedicinal = plant.IsMedicinal;
        existingPlant.IsToxic = plant.IsToxic;
        existingPlant.PictureId = plant.PictureId;
        existingPlant.GeneralNotes = plant.GeneralNotes?.Trim();
        StampForUpdate(existingPlant);

        return await _plantRepository.UpdateAsync(existingPlant, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeletePlantAsync(int plantId, CancellationToken cancellationToken = default)
    {
        var plant = await EnsurePlantExistsAsync(plantId, cancellationToken);
        var instanceCount = await _plantInstanceRepository.CountAsync(entity => entity.PlantId == plantId, cancellationToken: cancellationToken);

        if (instanceCount > 0)
            throw new DomainException($"Plant with id '{plantId}' cannot be deleted because it is referenced by existing plant instances.");

        await _plantRepository.DeleteAsync(plant, cancellationToken: cancellationToken);
    }

    #endregion

    #region Privates

    private async Task<Plant> EnsurePlantExistsAsync(int plantId, CancellationToken cancellationToken)
    {
        var plant = await _plantRepository.GetByIdAsync(plantId, cancellationToken: cancellationToken);
        return plant ?? throw new DomainException($"Plant with id '{plantId}' was not found.");
    }

    private async Task EnsureScientificNameIsUniqueAsync(string scientificName, int? excludedPlantId, CancellationToken cancellationToken)
    {
        var normalizedName = scientificName.Trim().ToLowerInvariant();
        var exists = await _plantRepository.AnyAsync(
            entity => entity.ScientificName.ToLower() == normalizedName
                && (!excludedPlantId.HasValue || entity.Id != excludedPlantId.Value),
            cancellationToken: cancellationToken);

        if (exists)
            throw new DomainException($"A plant with scientific name '{scientificName}' already exists.");
    }

    private static void ValidatePlant(Plant plant)
    {
        if (string.IsNullOrWhiteSpace(plant.CommonName))
            throw new DomainException("CommonName is required.");

        if (string.IsNullOrWhiteSpace(plant.ScientificName))
            throw new DomainException("ScientificName is required.");
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