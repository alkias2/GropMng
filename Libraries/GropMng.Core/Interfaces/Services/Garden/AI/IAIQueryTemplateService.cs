using GropMng.Core.Domain.Garden.AI;

namespace GropMng.Core.Interfaces.Services.Garden.AI;

/// <summary>
/// Service interface for managing AIQueryTemplate aggregate root.
/// Handles AI prompt template catalog operations.
/// </summary>
public interface IAIQueryTemplateService
{
    /// <summary>
    /// Gets a paginated list of AI query templates with optional search filtering.
    /// </summary>
    /// <param name="searchTerm">Optional search term to filter by template name or prompt template</param>
    /// <param name="pageIndex">Zero-based page index</param>
    /// <param name="pageSize">Number of records per page</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Paged list of templates</returns>
    Task<IPagedList<AIQueryTemplate>> GetTemplatesAsync(string? searchTerm = null, int pageIndex = 0, int pageSize = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific AI query template by unique identifier.
    /// </summary>
    /// <param name="templateId">The template ID (int)</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>Template entity or null if not found</returns>
    Task<AIQueryTemplate?> GetTemplateByIdAsync(int templateId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new AI query template with validation for name uniqueness.
    /// </summary>
    /// <param name="template">The template entity to create</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>The created template with database-generated ID</returns>
    /// <exception cref="DomainException">Thrown when template name is not unique or required fields are empty</exception>
    Task<AIQueryTemplate> CreateTemplateAsync(AIQueryTemplate template, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing AI query template with validation.
    /// </summary>
    /// <param name="template">The template entity with updated values</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <returns>The updated template</returns>
    /// <exception cref="DomainException">Thrown when template not found or name already exists</exception>
    Task<AIQueryTemplate> UpdateTemplateAsync(AIQueryTemplate template, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an AI query template by unique identifier.
    /// </summary>
    /// <param name="templateId">The template ID (int) to delete</param>
    /// <param name="cancellationToken">Cancellation token for the async operation</param>
    /// <exception cref="DomainException">Thrown when template not found</exception>
    Task DeleteTemplateAsync(int templateId, CancellationToken cancellationToken = default);
}
