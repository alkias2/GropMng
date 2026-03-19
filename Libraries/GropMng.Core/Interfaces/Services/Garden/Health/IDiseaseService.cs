using GropMng.Core.Domain.Garden.Health;

namespace GropMng.Core.Interfaces.Services.Garden.Health;

/// <summary>
/// Defines application services for managing diseases and their remedy links.
/// </summary>
public interface IDiseaseService
{
    /// <summary>
    /// Retrieves a paged list of diseases using an optional search term.
    /// </summary>
    /// <param name="searchTerm">An optional search term applied to disease names and notes.</param>
    /// <param name="pageIndex">The zero-based page index.</param>
    /// <param name="pageSize">The number of items to include in the page.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A paged list of diseases.</returns>
    Task<IPagedList<Disease>> GetDiseasesAsync(string? searchTerm = null, int pageIndex = 0, int pageSize = int.MaxValue, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single disease by identifier.
    /// </summary>
    /// <param name="diseaseId">The identifier of the disease to retrieve.</param>
    /// <param name="includeRemedyLinks">A value indicating whether remedy links should be loaded.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The matching disease when found; otherwise, <see langword="null" />.</returns>
    Task<Disease?> GetDiseaseByIdAsync(int diseaseId, bool includeRemedyLinks = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new disease definition.
    /// </summary>
    /// <param name="disease">The disease to create.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The created disease.</returns>
    Task<Disease> CreateDiseaseAsync(Disease disease, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing disease definition.
    /// </summary>
    /// <param name="disease">The disease entity containing the updated values.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The updated disease.</returns>
    Task<Disease> UpdateDiseaseAsync(Disease disease, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an existing disease definition.
    /// </summary>
    /// <param name="diseaseId">The identifier of the disease to delete.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    Task DeleteDiseaseAsync(int diseaseId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all remedy links configured for a disease.
    /// </summary>
    /// <param name="diseaseId">The identifier of the parent disease.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>A read-only list of remedy links.</returns>
    Task<IReadOnlyList<DiseaseRemedyLink>> GetRemedyLinksAsync(int diseaseId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a remedy link to a disease.
    /// </summary>
    /// <param name="diseaseId">The identifier of the parent disease.</param>
    /// <param name="remedyLink">The remedy link to create.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The created remedy link.</returns>
    Task<DiseaseRemedyLink> AddRemedyLinkAsync(int diseaseId, DiseaseRemedyLink remedyLink, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a remedy link that belongs to a disease.
    /// </summary>
    /// <param name="diseaseId">The identifier of the parent disease.</param>
    /// <param name="remedyLink">The remedy link containing the updated values.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>The updated remedy link.</returns>
    Task<DiseaseRemedyLink> UpdateRemedyLinkAsync(int diseaseId, DiseaseRemedyLink remedyLink, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a remedy link from a disease.
    /// </summary>
    /// <param name="diseaseId">The identifier of the parent disease.</param>
    /// <param name="remedyLinkId">The identifier of the remedy link to delete.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    Task DeleteRemedyLinkAsync(int diseaseId, int remedyLinkId, CancellationToken cancellationToken = default);
}