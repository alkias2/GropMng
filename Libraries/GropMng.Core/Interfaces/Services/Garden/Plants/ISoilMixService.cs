using GropMng.Core.Domain.Garden.Plants;

namespace GropMng.Core.Interfaces.Services.Garden.Plants;

/// <summary>
/// Service interface for managing SoilMix catalog entries and their ingredient rows.
/// </summary>
public interface ISoilMixService
{
    Task<IPagedList<SoilMix>> GetSoilMixesAsync(string? searchTerm = null, int pageIndex = 0, int pageSize = 10, CancellationToken cancellationToken = default);

    Task<SoilMix?> GetSoilMixByIdAsync(int soilMixId, CancellationToken cancellationToken = default);

    Task<SoilMix> CreateSoilMixAsync(SoilMix soilMix, CancellationToken cancellationToken = default);

    Task<SoilMix> UpdateSoilMixAsync(SoilMix soilMix, CancellationToken cancellationToken = default);

    Task DeleteSoilMixAsync(int soilMixId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SoilIngredient>> GetSoilIngredientsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SoilMixIngredient>> GetSoilMixIngredientsAsync(int soilMixId, CancellationToken cancellationToken = default);

    Task<SoilMixIngredient> AddSoilMixIngredientAsync(int soilMixId, SoilMixIngredient ingredient, CancellationToken cancellationToken = default);

    Task<SoilMixIngredient> UpdateSoilMixIngredientAsync(int soilMixId, int soilMixIngredientId, decimal percentageByVolume, string? notes, CancellationToken cancellationToken = default);

    Task DeleteSoilMixIngredientAsync(int soilMixId, int soilMixIngredientId, CancellationToken cancellationToken = default);

    Task<SoilIngredient> CreateSoilIngredientAsync(SoilIngredient ingredient, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<int>> GetUsedSoilIngredientIdsAsync(int soilMixId, CancellationToken cancellationToken = default);
}
