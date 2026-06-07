using GropMng.Core;
using GropMng.Core.Caching;
using GropMng.Core.Common.Exceptions;
using GropMng.Core.Domain.Garden;
using GropMng.Core.Domain.Garden.Plants;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.Garden.Plants;
using GropMng.Services.Caching.Garden;
using GropMng.Services.Helpers;
using Microsoft.EntityFrameworkCore;

namespace GropMng.Services.Services.Garden.Plants;

/// <summary>
/// Service implementation for managing SoilMix aggregate root and ingredient rows.
/// </summary>
public class SoilMixService : ISoilMixService
{
    private readonly IRepository<SoilMix> _soilMixRepository;
    private readonly IRepository<SoilIngredient> _soilIngredientRepository;
    private readonly IRepository<SoilMixIngredient> _soilMixIngredientRepository;
    private readonly IRepository<PlantInstance> _plantInstanceRepository;
    private readonly IGropStaticCacheManager _staticCacheManager;

    public SoilMixService(
        IRepository<SoilMix> soilMixRepository,
        IRepository<SoilIngredient> soilIngredientRepository,
        IRepository<SoilMixIngredient> soilMixIngredientRepository,
        IRepository<PlantInstance> plantInstanceRepository,
        IGropStaticCacheManager staticCacheManager)
    {
        _soilMixRepository = soilMixRepository ?? throw new ArgumentNullException(nameof(soilMixRepository));
        _soilIngredientRepository = soilIngredientRepository ?? throw new ArgumentNullException(nameof(soilIngredientRepository));
        _soilMixIngredientRepository = soilMixIngredientRepository ?? throw new ArgumentNullException(nameof(soilMixIngredientRepository));
        _plantInstanceRepository = plantInstanceRepository ?? throw new ArgumentNullException(nameof(plantInstanceRepository));
        _staticCacheManager = staticCacheManager ?? throw new ArgumentNullException(nameof(staticCacheManager));
    }

    public async Task<IPagedList<SoilMix>> GetSoilMixesAsync(string? searchTerm = null, int pageIndex = 0, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var cacheKey = _staticCacheManager.PrepareKey(SoilMixCacheDefaults.AllSoilMixesKey);

        return await _staticCacheManager.GetAsync(cacheKey, () =>
            _soilMixRepository.GetPagedAsync(
                query =>
                {
                    if (!string.IsNullOrWhiteSpace(searchTerm))
                    {
                        var term = searchTerm.Trim();
                        query = query.Where(m => m.Name.Contains(term) ||
                                                 (m.Composition != null && m.Composition.Contains(term)) ||
                                                 (m.Notes != null && m.Notes.Contains(term)));
                    }

                    return query.OrderBy(m => m.Name).ThenBy(m => m.Id);
                },
                pageIndex,
                pageSize,
                cancellationToken: cancellationToken));
    }

    public async Task<SoilMix?> GetSoilMixByIdAsync(int soilMixId, CancellationToken cancellationToken = default)
    {
        ValidateId(soilMixId, nameof(soilMixId));

        var cacheKey = _staticCacheManager.PrepareKey(SoilMixCacheDefaults.SoilMixByIdKey, soilMixId);

        return await _staticCacheManager.GetAsync(cacheKey, () =>
            _soilMixRepository.GetByIdAsync(soilMixId, cancellationToken: cancellationToken));
    }

    public async Task<SoilMix> CreateSoilMixAsync(SoilMix soilMix, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(soilMix);
        ValidateSoilMix(soilMix);

        await EnsureSoilMixNameIsUniqueAsync(soilMix.Name, cancellationToken);

        AuditableEntityHelper.StampForCreate(soilMix);
        await _soilMixRepository.CreateAsync(soilMix, true, cancellationToken);
        await _staticCacheManager.RemoveByPrefixAsync(SoilMixCacheDefaults.SoilMixPrefix);

        return soilMix;
    }

    public async Task<SoilMix> UpdateSoilMixAsync(SoilMix soilMix, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(soilMix);
        ValidateId(soilMix.Id, nameof(soilMix.Id));
        ValidateSoilMix(soilMix);

        await EnsureSoilMixNameIsUniqueAsync(soilMix.Name, cancellationToken, soilMix.Id);

        var existing = await EnsureSoilMixExistsAsync(soilMix.Id, cancellationToken);
        existing.Name = soilMix.Name.Trim();
        existing.Composition = soilMix.Composition?.Trim();
        existing.PhMin = soilMix.PhMin;
        existing.PhMax = soilMix.PhMax;
        existing.Texture = soilMix.Texture;
        existing.Drainage = soilMix.Drainage;
        existing.Notes = soilMix.Notes?.Trim();

        AuditableEntityHelper.StampForUpdate(existing);
        await _soilMixRepository.UpdateAsync(existing, true, cancellationToken);
        await _staticCacheManager.RemoveByPrefixAsync(SoilMixCacheDefaults.SoilMixPrefix);

        return existing;
    }

    public async Task DeleteSoilMixAsync(int soilMixId, CancellationToken cancellationToken = default)
    {
        var soilMix = await EnsureSoilMixExistsAsync(soilMixId, cancellationToken);

        var references = await _plantInstanceRepository.CountAsync(
            p => p.SoilMixId == soilMixId,
            cancellationToken: cancellationToken);

        if (references > 0)
            throw new DomainException($"Cannot delete soil mix '{soilMix.Name}' because it is referenced by {references} plant instance(s).");

        await _soilMixRepository.DeleteAsync(soilMix, true, true, cancellationToken);
        await _staticCacheManager.RemoveByPrefixAsync(SoilMixCacheDefaults.SoilMixPrefix);
    }

    public Task<IReadOnlyList<SoilIngredient>> GetSoilIngredientsAsync(CancellationToken cancellationToken = default)
    {
        return _soilIngredientRepository.GetAllAsync(
            query => query.OrderBy(i => i.Name).ThenBy(i => i.Id),
            cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<SoilMixIngredient>> GetSoilMixIngredientsAsync(int soilMixId, CancellationToken cancellationToken = default)
    {
        ValidateId(soilMixId, nameof(soilMixId));
        await EnsureSoilMixExistsAsync(soilMixId, cancellationToken);

        return await _soilMixIngredientRepository.GetAllAsync(
            query => query
                .Where(i => i.SoilMixId == soilMixId)
                .Include(i => i.SoilIngredient)
                .OrderBy(i => i.Id),
            cancellationToken: cancellationToken);
    }

    public async Task<SoilMixIngredient> AddSoilMixIngredientAsync(int soilMixId, SoilMixIngredient ingredient, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ingredient);
        ValidateId(soilMixId, nameof(soilMixId));

        var soilMix = await EnsureSoilMixExistsAsync(soilMixId, cancellationToken);
        var soilIngredient = await EnsureSoilIngredientExistsAsync(ingredient.SoilIngredientId, cancellationToken);

        if (ingredient.PercentageByVolume <= 0 || ingredient.PercentageByVolume > 100)
            throw new DomainException("Ingredient percentage must be greater than 0 and at most 100.");

        var duplicateCount = await _soilMixIngredientRepository.CountAsync(
            i => i.SoilMixId == soilMixId && i.SoilIngredientId == ingredient.SoilIngredientId,
            cancellationToken: cancellationToken);

        if (duplicateCount > 0)
            throw new DomainException($"Ingredient '{soilIngredient.Name}' already exists in soil mix '{soilMix.Name}'.");

        var existingRows = await _soilMixIngredientRepository.GetAllAsync(
            query => query.Where(i => i.SoilMixId == soilMixId),
            cancellationToken: cancellationToken);

        var currentTotal = existingRows.Sum(i => i.PercentageByVolume);
        if (currentTotal + ingredient.PercentageByVolume > 100)
            throw new DomainException("Total percentage cannot exceed 100.");

        ingredient.SoilMixId = soilMixId;
        ingredient.Notes = ingredient.Notes?.Trim();
        StampForCreate(ingredient);

        await _soilMixIngredientRepository.CreateAsync(ingredient, true, cancellationToken);
        await _staticCacheManager.RemoveByPrefixAsync(SoilMixCacheDefaults.SoilMixPrefix);

        ingredient.SoilIngredient = soilIngredient;
        return ingredient;
    }

    public async Task<SoilMixIngredient> UpdateSoilMixIngredientAsync(int soilMixId, int soilMixIngredientId, decimal percentageByVolume, string? notes, CancellationToken cancellationToken = default)
    {
        ValidateId(soilMixId, nameof(soilMixId));
        ValidateId(soilMixIngredientId, nameof(soilMixIngredientId));

        await EnsureSoilMixExistsAsync(soilMixId, cancellationToken);

        var row = await _soilMixIngredientRepository.FirstOrDefaultAsync(
            i => i.Id == soilMixIngredientId && i.SoilMixId == soilMixId,
            cancellationToken: cancellationToken)
            ?? throw new DomainException($"Ingredient row '{soilMixIngredientId}' was not found.");

        if (percentageByVolume <= 0 || percentageByVolume > 100)
            throw new DomainException("Ingredient percentage must be greater than 0 and at most 100.");

        var otherRows = await _soilMixIngredientRepository.GetAllAsync(
            query => query.Where(i => i.SoilMixId == soilMixId && i.Id != soilMixIngredientId),
            cancellationToken: cancellationToken);

        var otherTotal = otherRows.Sum(i => i.PercentageByVolume);
        if (otherTotal + percentageByVolume > 100)
            throw new DomainException($"Total percentage cannot exceed 100. Other ingredients already use {otherTotal}%.");

        row.PercentageByVolume = percentageByVolume;
        row.Notes = notes?.Trim();
        StampForUpdate(row);

        await _soilMixIngredientRepository.UpdateAsync(row, true, cancellationToken);
        await _staticCacheManager.RemoveByPrefixAsync(SoilMixCacheDefaults.SoilMixPrefix);

        return row;
    }

    public async Task DeleteSoilMixIngredientAsync(int soilMixId, int soilMixIngredientId, CancellationToken cancellationToken = default)
    {
        ValidateId(soilMixId, nameof(soilMixId));
        ValidateId(soilMixIngredientId, nameof(soilMixIngredientId));

        await EnsureSoilMixExistsAsync(soilMixId, cancellationToken);

        var row = await _soilMixIngredientRepository.FirstOrDefaultAsync(
            i => i.Id == soilMixIngredientId && i.SoilMixId == soilMixId,
            cancellationToken: cancellationToken);

        if (row is null)
            throw new DomainException($"Soil mix ingredient row '{soilMixIngredientId}' was not found for soil mix '{soilMixId}'.");

        await _soilMixIngredientRepository.DeleteAsync(row, true, true, cancellationToken);
        await _staticCacheManager.RemoveByPrefixAsync(SoilMixCacheDefaults.SoilMixPrefix);
    }

    public async Task<SoilIngredient> CreateSoilIngredientAsync(SoilIngredient ingredient, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ingredient);

        if (string.IsNullOrWhiteSpace(ingredient.Name))
            throw new DomainException("Ingredient name is required.");

        var count = await _soilIngredientRepository.CountAsync(
            i => i.Name.ToLower() == ingredient.Name.Trim().ToLower(),
            cancellationToken: cancellationToken);

        if (count > 0)
            throw new DomainException($"A soil ingredient named '{ingredient.Name}' already exists.");

        ingredient.Name = ingredient.Name.Trim();
        ingredient.Description = ingredient.Description?.Trim();
        StampForCreate(ingredient);

        await _soilIngredientRepository.CreateAsync(ingredient, true, cancellationToken);
        await _staticCacheManager.RemoveByPrefixAsync(SoilMixCacheDefaults.SoilMixPrefix);

        return ingredient;
    }

    public async Task<IReadOnlyList<int>> GetUsedSoilIngredientIdsAsync(int soilMixId, CancellationToken cancellationToken = default)
    {
        ValidateId(soilMixId, nameof(soilMixId));

        var rows = await _soilMixIngredientRepository.GetAllAsync(
            query => query.Where(i => i.SoilMixId == soilMixId),
            cancellationToken: cancellationToken);

        return rows.Select(i => i.SoilIngredientId).ToList();
    }

    private async Task<SoilMix> EnsureSoilMixExistsAsync(int soilMixId, CancellationToken cancellationToken)
    {
        var mix = await _soilMixRepository.GetByIdAsync(soilMixId, cancellationToken: cancellationToken);
        return mix ?? throw new DomainException($"SoilMix with id '{soilMixId}' not found.");
    }

    private async Task<SoilIngredient> EnsureSoilIngredientExistsAsync(int soilIngredientId, CancellationToken cancellationToken)
    {
        var ingredient = await _soilIngredientRepository.GetByIdAsync(soilIngredientId, cancellationToken: cancellationToken);
        return ingredient ?? throw new DomainException($"SoilIngredient with id '{soilIngredientId}' not found.");
    }

    private async Task EnsureSoilMixNameIsUniqueAsync(string name, CancellationToken cancellationToken, int? excludingId = null)
    {
        var normalized = name.Trim().ToLower();

        var count = await _soilMixRepository.CountAsync(
            m => m.Name.ToLower() == normalized && (!excludingId.HasValue || m.Id != excludingId.Value),
            cancellationToken: cancellationToken);

        if (count > 0)
            throw new DomainException($"A soil mix with name '{name}' already exists.");
    }

    private static void ValidateId(int id, string field)
    {
        if (id <= 0)
            throw new DomainException($"{field} must be greater than zero.");
    }

    private static void ValidateSoilMix(SoilMix soilMix)
    {
        if (string.IsNullOrWhiteSpace(soilMix.Name))
            throw new DomainException("Soil mix name is required.");

        if (soilMix.PhMin.HasValue && soilMix.PhMax.HasValue && soilMix.PhMin.Value > soilMix.PhMax.Value)
            throw new DomainException("PhMin cannot be greater than PhMax.");
    }

    private static void StampForCreate(AuditableEntity entity)
        => AuditableEntityHelper.StampForCreate(entity);

    private static void StampForUpdate(AuditableEntity entity)
        => AuditableEntityHelper.StampForUpdate(entity);
}