using GropMng.Core;
using GropMng.Core.Domain.Garden;
using GropMng.Core.Common.Exceptions;
using GropMng.Core.Domain.Garden.AI;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.Garden.AI;

namespace GropMng.Services.Services.Garden.AI;

/// <summary>
/// Service implementation for managing AIQueryTemplate aggregate root.
/// Handles AI prompt template catalog with CRUD operations.
/// </summary>
public class AIQueryTemplateService : IAIQueryTemplateService
{
    #region Fields

    private readonly IRepository<AIQueryTemplate> _templateRepository;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the AIQueryTemplateService class.
    /// </summary>
    public AIQueryTemplateService(IRepository<AIQueryTemplate> templateRepository)
    {
        _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
    }

    #endregion

    #region Public Methods - CRUD Operations

    /// <summary>
    /// Gets a paginated list of AI query templates with optional search filtering.
    /// </summary>
    public async Task<IPagedList<AIQueryTemplate>> GetTemplatesAsync(string? searchTerm = null, int pageIndex = 0, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        return await _templateRepository.GetPagedAsync(
            query =>
            {
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchTermLower = searchTerm.Trim().ToLower();
                    query = query.Where(t => t.TemplateName.Contains(searchTermLower) ||
                                             (t.PromptTemplate != null && t.PromptTemplate.Contains(searchTermLower)));
                }

                return query.OrderBy(t => t.SortOrder).ThenBy(t => t.TemplateName);
            },
            pageIndex,
            pageSize,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Gets a specific AI query template by unique identifier.
    /// </summary>
    public async Task<AIQueryTemplate?> GetTemplateByIdAsync(int templateId, CancellationToken cancellationToken = default)
    {
        if (templateId <= 0)
            throw new DomainException("Template ID must be greater than zero.");

        return await _templateRepository.GetByIdAsync(templateId, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Creates a new AI query template with validation for name uniqueness.
    /// </summary>
    public async Task<AIQueryTemplate> CreateTemplateAsync(AIQueryTemplate template, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(template);

        if (string.IsNullOrWhiteSpace(template.TemplateName))
            throw new DomainException("Template name is required and cannot be empty.");

        if (string.IsNullOrWhiteSpace(template.PromptTemplate))
            throw new DomainException("Prompt template is required and cannot be empty.");

        await EnsureTemplateNameIsUniqueAsync(template.TemplateName, cancellationToken);

        StampForCreate(template);

        await _templateRepository.CreateAsync(template, true, cancellationToken);

        return template;
    }

    /// <summary>
    /// Updates an existing AI query template with validation.
    /// </summary>
    public async Task<AIQueryTemplate> UpdateTemplateAsync(AIQueryTemplate template, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(template);

        if (template.Id <= 0)
            throw new DomainException("Template ID is required for update.");

        if (string.IsNullOrWhiteSpace(template.TemplateName))
            throw new DomainException("Template name is required and cannot be empty.");

        if (string.IsNullOrWhiteSpace(template.PromptTemplate))
            throw new DomainException("Prompt template is required and cannot be empty.");

        var existingTemplate = await EnsureTemplateExistsAsync(template.Id, cancellationToken);

        if (existingTemplate.TemplateName.ToLower() != template.TemplateName.Trim().ToLower())
            await EnsureTemplateNameIsUniqueAsync(template.TemplateName, cancellationToken);

        existingTemplate.TemplateName = template.TemplateName.Trim();
        existingTemplate.Scenario = template.Scenario;
        existingTemplate.Language = template.Language;
        existingTemplate.PromptTemplate = template.PromptTemplate.Trim();
        existingTemplate.IsActive = template.IsActive;
        existingTemplate.SortOrder = template.SortOrder;

        StampForUpdate(existingTemplate);

        await _templateRepository.UpdateAsync(existingTemplate, true, cancellationToken);

        return existingTemplate;
    }

    /// <summary>
    /// Deletes an AI query template by unique identifier.
    /// </summary>
    public async Task DeleteTemplateAsync(int templateId, CancellationToken cancellationToken = default)
    {
        if (templateId <= 0)
            throw new DomainException("Template ID must be greater than zero.");

        var template = await EnsureTemplateExistsAsync(templateId, cancellationToken);

    await _templateRepository.DeleteAsync(template, true, true, cancellationToken);
    }

    #endregion

    #region Private Methods - Validation & Domain Guards

    /// <summary>
    /// Validates that a template exists, throwing an exception if not found.
    /// </summary>
    private async Task<AIQueryTemplate> EnsureTemplateExistsAsync(int templateId, CancellationToken cancellationToken = default)
    {
        var template = await _templateRepository.GetByIdAsync(templateId, cancellationToken: cancellationToken);

        if (template == null)
            throw new DomainException($"Template with ID '{templateId}' not found.");

        return template;
    }

    /// <summary>
    /// Validates that a template name is unique across all templates.
    /// </summary>
    private async Task EnsureTemplateNameIsUniqueAsync(string templateName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(templateName))
            throw new DomainException("Template name cannot be empty for uniqueness validation.");

        var normalizedName = templateName.Trim().ToLower();
        var existingTemplates = await _templateRepository.FindAsync(
            t => t.TemplateName.ToLower() == normalizedName,
            false,
            true,
            cancellationToken);

        if (existingTemplates.Count > 0)
            throw new DomainException($"A template with the name '{templateName}' already exists.");
    }

    /// <summary>
    /// Stamps an entity for creation with audit fields.
    /// </summary>
    private void StampForCreate(BaseEntity entity)
    {
        if (entity is AuditableEntity auditableEntity)
        {
            auditableEntity.CreatedAtUtc = DateTime.UtcNow;
            auditableEntity.UpdatedAtUtc = DateTime.UtcNow;
            auditableEntity.IsDeleted = false;
        }
    }

    /// <summary>
    /// Stamps an entity for update with audit fields.
    /// </summary>
    private void StampForUpdate(BaseEntity entity)
    {
        if (entity is AuditableEntity auditableEntity)
        {
            auditableEntity.UpdatedAtUtc = DateTime.UtcNow;
        }
    }

    #endregion
}
