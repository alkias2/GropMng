using GropMng.Core;
using GropMng.Core.Domain.Garden;
using GropMng.Core.Common.Exceptions;
using GropMng.Core.Domain.Garden.Care;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.Garden.Care;

namespace GropMng.Services.Services.Garden.Care;

/// <summary>
/// Service implementation for managing Fertilizer aggregate root.
/// Implements CRUD operations with validation for uniqueness and referential integrity.
/// </summary>
public class FertilizerService : IFertilizerService
{
    #region Fields

    private readonly IRepository<Fertilizer> _fertilizerRepository;
    private readonly IRepository<FertilizingSchedule> _fertilizingScheduleRepository;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the FertilizerService class.
    /// </summary>
    public FertilizerService(
        IRepository<Fertilizer> fertilizerRepository,
        IRepository<FertilizingSchedule> fertilizingScheduleRepository)
    {
        _fertilizerRepository = fertilizerRepository ?? throw new ArgumentNullException(nameof(fertilizerRepository));
        _fertilizingScheduleRepository = fertilizingScheduleRepository ?? throw new ArgumentNullException(nameof(fertilizingScheduleRepository));
    }

    #endregion

    #region Public Methods - Fertilizer CRUD Operations

    /// <summary>
    /// Gets a paginated list of fertilizers with optional search filtering.
    /// </summary>
    public async Task<IPagedList<Fertilizer>> GetFertilizersAsync(string? searchTerm = null, int pageIndex = 0, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        return await _fertilizerRepository.GetPagedAsync(
            query =>
            {
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchTermLower = searchTerm.Trim().ToLower();
                    query = query.Where(f => f.Name.Contains(searchTermLower) ||
                                             (f.Brand != null && f.Brand.Contains(searchTermLower)) ||
                                             (f.Notes != null && f.Notes.Contains(searchTermLower)));
                }

                return query.OrderBy(f => f.Name);
            },
            pageIndex,
            pageSize,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Gets a specific fertilizer by unique identifier.
    /// </summary>
    public async Task<Fertilizer?> GetFertilizerByIdAsync(int fertilizerId, CancellationToken cancellationToken = default)
    {
        if (fertilizerId <= 0)
            throw new DomainException("Fertilizer ID must be greater than zero.");

        return await _fertilizerRepository.GetByIdAsync(fertilizerId, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Creates a new fertilizer entity with validation for name uniqueness.
    /// </summary>
    public async Task<Fertilizer> CreateFertilizerAsync(Fertilizer fertilizer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fertilizer);

        if (string.IsNullOrWhiteSpace(fertilizer.Name))
            throw new DomainException("Fertilizer name is required and cannot be empty.");

        await EnsureFertilizerNameIsUniqueAsync(fertilizer.Name, cancellationToken);

        StampForCreate(fertilizer);

        await _fertilizerRepository.CreateAsync(fertilizer, true, cancellationToken);

        return fertilizer;
    }

    /// <summary>
    /// Updates an existing fertilizer with validation for name uniqueness.
    /// </summary>
    public async Task<Fertilizer> UpdateFertilizerAsync(Fertilizer fertilizer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fertilizer);

        if (fertilizer.Id <= 0)
            throw new DomainException("Fertilizer ID is required for update.");

        if (string.IsNullOrWhiteSpace(fertilizer.Name))
            throw new DomainException("Fertilizer name is required and cannot be empty.");




        // Verify fertilizer exists
        var existingFertilizers = await _fertilizerRepository.FindAsync(
            f => f.Id == fertilizer.Id,
            false,
            true,
            cancellationToken);

        if (existingFertilizers.Count == 0)
            throw new DomainException($"Fertilizer with ID '{fertilizer.Id}' not found.");

        var existing = existingFertilizers[0];

        existing.Name = fertilizer.Name.Trim();
        existing.Brand = fertilizer.Brand?.Trim();
        existing.NpkRatio = fertilizer.NpkRatio?.Trim();
        existing.FertilizerType = fertilizer.FertilizerType;
        existing.ApplicationMethod = fertilizer.ApplicationMethod;
        existing.IsOrganic = fertilizer.IsOrganic;
        existing.Notes = fertilizer.Notes?.Trim();

        StampForUpdate(existing);

        await _fertilizerRepository.UpdateAsync(existing, true, cancellationToken);

        return existing;
    }

    /// <summary>
    /// Deletes a fertilizer if it has no active references in FertilizingSchedule.
    /// </summary>
    public async Task DeleteFertilizerAsync(int fertilizerId, CancellationToken cancellationToken = default)
    {
        if (fertilizerId <= 0)
            throw new DomainException("Fertilizer ID must be greater than zero.");

        var fertilizer = await EnsureFertilizerExistsAsync(fertilizerId, cancellationToken);

        var referencingSchedules = await _fertilizingScheduleRepository.FindAsync(
            fs => fs.FertilizerId == fertilizerId,
            false,
            true,
            cancellationToken);

        if (referencingSchedules.Count > 0)
            throw new DomainException(
                $"Cannot delete fertilizer '{fertilizer.Name}' because it is referenced by {referencingSchedules.Count} active schedule(s).");

        await _fertilizerRepository.DeleteAsync(fertilizer, true, true, cancellationToken);
    }

    #endregion

    #region Private Methods - Validation & Domain Guards

    /// <summary>
    /// Validates that a fertilizer exists, throwing an exception if not found.
    /// </summary>
    private async Task<Fertilizer> EnsureFertilizerExistsAsync(int fertilizerId, CancellationToken cancellationToken = default)
    {
        var fertilizer = await _fertilizerRepository.GetByIdAsync(fertilizerId, cancellationToken: cancellationToken);

        if (fertilizer == null)
            throw new DomainException($"Fertilizer with ID '{fertilizerId}' not found.");

        return fertilizer;
    }

    /// <summary>
    /// Validates that a fertilizer name is unique across all fertilizers.
    /// </summary>
    private async Task EnsureFertilizerNameIsUniqueAsync(string fertilizerName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fertilizerName))
            throw new DomainException("Fertilizer name cannot be empty for uniqueness validation.");

        var normalizedName = fertilizerName.Trim().ToLower();
        var existingFertilizers = await _fertilizerRepository.FindAsync(
            f => f.Name.ToLower() == normalizedName,
            false,
            true,
            cancellationToken);

        if (existingFertilizers.Count > 0)
            throw new DomainException($"A fertilizer with the name '{fertilizerName}' already exists.");
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
