using GropMng.Core.Domain.Garden.AI;

namespace GropMng.Core.Interfaces.Services.AI;

/// <summary>
/// Service interface for managing AI Query Template aggregate root.
/// Handles storage and retrieval of custom AI query templates for domain-specific operations.
/// </summary>
public interface IAIQueryTemplateService
{
    #region Template CRUD Operations

    /// <summary>
    /// Gets a paginated list of AI query templates with optional search filtering.
    /// </summary>
    /// <param name="searchTerm">Optional search term to filter by name or description</param>
    /// <param name="pageIndex">Zero-based page index</param>
    /// <param name="pageSize">Number of records per page</param>
    /// <returns>Paged list of AI query templates</returns>
    Task<IPagedList<AIQueryTemplate>> GetTemplatesAsync(string? searchTerm = null, int pageIndex = 0, int pageSize = 10);

    /// <summary>
    /// Gets a specific AI query template by unique identifier.
    /// </summary>
    /// <param name="templateId">The template UUID</param>
    /// <returns>Template entity or null if not found</returns>
    Task<AIQueryTemplate> GetTemplateByIdAsync(Guid templateId);

    /// <summary>
    /// Gets AI query templates filtered by category.
    /// </summary>
    /// <param name="category">Template category filter</param>
    /// <returns>List of templates matching the category</returns>
    Task<IList<AIQueryTemplate>> GetTemplatesByCategoryAsync(string category);

    /// <summary>
    /// Creates a new AI query template with validation for name uniqueness.
    /// </summary>
    /// <param name="template">The template entity to create</param>
    /// <returns>The created template with generated ID</returns>
    /// <exception cref="DomainException">Thrown when template name is not unique or required fields are empty</exception>
    Task<AIQueryTemplate> CreateTemplateAsync(AIQueryTemplate template);

    /// <summary>
    /// Updates an existing AI query template.
    /// </summary>
    /// <param name="template">The template entity with updated values</param>
    /// <returns>The updated template</returns>
    /// <exception cref="DomainException">Thrown when template not found or name already exists</exception>
    Task<AIQueryTemplate> UpdateTemplateAsync(AIQueryTemplate template);

    /// <summary>
    /// Deletes an AI query template.
    /// </summary>
    /// <param name="templateId">The template UUID to delete</param>
    /// <exception cref="DomainException">Thrown when template not found</exception>
    Task DeleteTemplateAsync(Guid templateId);

    #endregion
}
