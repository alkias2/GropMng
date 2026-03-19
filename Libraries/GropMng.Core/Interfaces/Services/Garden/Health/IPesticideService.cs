using GropMng.Core.Domain.Garden.Health;

namespace GropMng.Core.Interfaces.Services.Garden.Health;

/// <summary>
/// Service interface for managing Pesticide aggregate root and DiseaseRemedyLink children.
/// Handles pesticide catalog and disease-pesticide remedy mappings.
/// </summary>
public interface IPesticideService
{
    #region Pesticide CRUD Operations

    /// <summary>
    /// Gets a paginated list of pesticides with optional search filtering.
    /// </summary>
    /// <param name="searchTerm">Optional search term to filter by name or active ingredient</param>
    /// <param name="pageIndex">Zero-based page index</param>
    /// <param name="pageSize">Number of records per page</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Paged list of pesticides</returns>
    Task<IPagedList<Pesticide>> GetPesticidesAsync(string? searchTerm = null, int pageIndex = 0, int pageSize = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific pesticide by unique identifier.
    /// </summary>
    /// <param name="pesticideId">The pesticide ID (int)</param>
    /// <param name="includeRemedyLinks">Whether to load associated DiseaseRemedyLink children (unused in current implementation)</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Pesticide entity or null if not found</returns>
    Task<Pesticide?> GetPesticideByIdAsync(int pesticideId, bool includeRemedyLinks = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new pesticide entity with validation for name uniqueness.
    /// </summary>
    /// <param name="pesticide">The pesticide entity to create</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>The created pesticide with database-generated ID</returns>
    /// <exception cref="DomainException">Thrown when pesticide name is not unique or required fields are empty</exception>
    Task<Pesticide> CreatePesticideAsync(Pesticide pesticide, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing pesticide with validation.
    /// </summary>
    /// <param name="pesticide">The pesticide entity with updated values</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>The updated pesticide</returns>
    /// <exception cref="DomainException">Thrown when pesticide not found or name already exists</exception>
    Task<Pesticide> UpdatePesticideAsync(Pesticide pesticide, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a pesticide if it has no active references in DiseaseRemedyLink.
    /// </summary>
    /// <param name="pesticideId">The pesticide ID (int) to delete</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <exception cref="DomainException">Thrown when pesticide not found or is referenced by active remedy links</exception>
    Task DeletePesticideAsync(int pesticideId, CancellationToken cancellationToken = default);

    #endregion

    #region DiseaseRemedyLink Child Operations

    /// <summary>
    /// Gets all remedy links for a specific pesticide.
    /// </summary>
    /// <param name="pesticideId">The pesticide ID (int)</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>List of disease-remedy links for this pesticide</returns>
    Task<IList<DiseaseRemedyLink>> GetRemedyLinksAsync(int pesticideId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Links a pesticide to a disease as a remedy option.
    /// </summary>
    /// <param name="pesticideId">The pesticide ID (int)</param>
    /// <param name="remedyLink">The DiseaseRemedyLink entity to create</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>The created remedy link with database-generated ID</returns>
    /// <exception cref="DomainException">Thrown when pesticide/disease not found or link already exists</exception>
    Task<DiseaseRemedyLink> AddRemedyLinkAsync(int pesticideId, DiseaseRemedyLink remedyLink, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing disease-remedy link.
    /// </summary>
    /// <param name="pesticideId">The pesticide ID (int)</param>
    /// <param name="remedyLink">The updated remedy link entity</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>The updated remedy link</returns>
    /// <exception cref="DomainException">Thrown when link not found or pesticide/disease mismatch</exception>
    Task<DiseaseRemedyLink> UpdateRemedyLinkAsync(int pesticideId, DiseaseRemedyLink remedyLink, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a disease-remedy link.
    /// </summary>
    /// <param name="pesticideId">The pesticide ID (int)</param>
    /// <param name="remedyLinkId">The remedy link ID (int) to delete</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <exception cref="DomainException">Thrown when link not found or pesticide ID mismatch</exception>
    Task DeleteRemedyLinkAsync(int pesticideId, int remedyLinkId, CancellationToken cancellationToken = default);

    #endregion
}
