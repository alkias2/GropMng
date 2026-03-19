using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Plants;

namespace GropMng.Core.Interfaces.Services.Garden.Plants;

/// <summary>
/// Defines application services for managing the plant catalog aggregate root.
/// </summary>
public interface IPlantService
{
    /// <summary>
    /// Retrieves a paged list of plant catalog entries using optional search and category filters.
    /// </summary>
    /// <param name="searchTerm">An optional search term applied to common name, scientific name, and family.</param>
    /// <param name="category">An optional plant category filter.</param>
    /// <param name="pageIndex">The zero-based page index.</param>
    /// <param name="pageSize">The number of items to include in the page.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A paged list of plant catalog entries.</returns>
    Task<IPagedList<Plant>> GetPlantsAsync(string? searchTerm = null, PlantCategory? category = null, int pageIndex = 0, int pageSize = int.MaxValue, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single plant by identifier.
    /// </summary>
    /// <param name="plantId">The identifier of the plant to retrieve.</param>
    /// <param name="includeInstances">A value indicating whether related plant instances should be loaded.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The matching plant when found; otherwise, <see langword="null" />.</returns>
    Task<Plant?> GetPlantByIdAsync(int plantId, bool includeInstances = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new plant catalog entry.
    /// </summary>
    /// <param name="plant">The plant to create.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The created plant entity.</returns>
    Task<Plant> CreatePlantAsync(Plant plant, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing plant catalog entry.
    /// </summary>
    /// <param name="plant">The plant entity containing the updated values.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The updated plant entity.</returns>
    Task<Plant> UpdatePlantAsync(Plant plant, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an existing plant catalog entry.
    /// </summary>
    /// <param name="plantId">The identifier of the plant to delete.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    Task DeletePlantAsync(int plantId, CancellationToken cancellationToken = default);
}