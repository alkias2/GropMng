using GropMng.Core.Domain.Garden.Care;

namespace GropMng.Core.Interfaces.Services.Garden.Care;

/// <summary>
/// Service interface for managing Fertilizer aggregate root and related operations.
/// Handles fertilizer catalog management with validation for uniqueness and referential integrity.
/// </summary>
public interface IFertilizerService
{
    #region Fertilizer CRUD Operations

    /// <summary>
    /// Gets a paginated list of fertilizers with optional search filtering.
    /// </summary>
    /// <param name="searchTerm">Optional search term to filter by name, brand, or notes</param>
    /// <param name="pageIndex">Zero-based page index</param>
    /// <param name="pageSize">Number of records per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paged list of fertilizers</returns>
    Task<IPagedList<Fertilizer>> GetFertilizersAsync(string? searchTerm = null, int pageIndex = 0, int pageSize = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific fertilizer by unique identifier.
    /// </summary>
    /// <param name="fertilizerId">The int fertilizer identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Fertilizer entity or null if not found</returns>
    Task<Fertilizer?> GetFertilizerByIdAsync(int fertilizerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new fertilizer entity with validation for name uniqueness.
    /// </summary>
    /// <param name="fertilizer">The fertilizer entity to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created fertilizer with generated ID</returns>
    /// <exception cref="DomainException">Thrown when fertilizer name is not unique or name is empty</exception>
    Task<Fertilizer> CreateFertilizerAsync(Fertilizer fertilizer, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing fertilizer with validation for name uniqueness.
    /// </summary>
    /// <param name="fertilizer">The fertilizer entity with updated values</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated fertilizer</returns>
    /// <exception cref="DomainException">Thrown when fertilizer not found or name already exists for another fertilizer</exception>
    Task<Fertilizer> UpdateFertilizerAsync(Fertilizer fertilizer, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a fertilizer if it has no active references in FertilizingSchedule.
    /// </summary>
    /// <param name="fertilizerId">The int fertilizer identifier to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <exception cref="DomainException">Thrown when fertilizer not found or is referenced by active schedules</exception>
    Task DeleteFertilizerAsync(int fertilizerId, CancellationToken cancellationToken = default);

    #endregion
}
