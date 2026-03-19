using GropMng.Core;
using GropMng.Core.Domain.Garden;
using GropMng.Core.Common.Exceptions;
using GropMng.Core.Domain.Garden.Health;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.Garden.Health;

namespace GropMng.Services.Services.Garden.Health;

/// <summary>
/// Service implementation for managing Pesticide aggregate root and DiseaseRemedyLink children.
/// Implements CRUD operations and child-entity management with validation.
/// </summary>
public class PesticideService : IPesticideService
{
    #region Fields

    private readonly IRepository<Pesticide> _pesticideRepository;
    private readonly IRepository<Disease> _diseaseRepository;
    private readonly IRepository<DiseaseRemedyLink> _remedyLinkRepository;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the PesticideService class.
    /// </summary>
    public PesticideService(
        IRepository<Pesticide> pesticideRepository,
        IRepository<Disease> diseaseRepository,
        IRepository<DiseaseRemedyLink> remedyLinkRepository)
    {
        _pesticideRepository = pesticideRepository ?? throw new ArgumentNullException(nameof(pesticideRepository));
        _diseaseRepository = diseaseRepository ?? throw new ArgumentNullException(nameof(diseaseRepository));
        _remedyLinkRepository = remedyLinkRepository ?? throw new ArgumentNullException(nameof(remedyLinkRepository));
    }

    #endregion

    #region Public Methods - Pesticide CRUD Operations

    /// <summary>
    /// Gets a paginated list of pesticides with optional search filtering.
    /// </summary>
    public async Task<IPagedList<Pesticide>> GetPesticidesAsync(string? searchTerm = null, int pageIndex = 0, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        return await _pesticideRepository.GetPagedAsync(
            query =>
            {
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchTermLower = searchTerm.Trim().ToLower();
                    query = query.Where(p => p.Name.Contains(searchTermLower) ||
                                             (p.ActiveIngredient != null && p.ActiveIngredient.Contains(searchTermLower)));
                }

                return query.OrderBy(p => p.Name);
            },
            pageIndex,
            pageSize,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Gets a specific pesticide by unique identifier with optional remedy links.
    /// </summary>
    public async Task<Pesticide?> GetPesticideByIdAsync(int pesticideId, bool includeRemedyLinks = false, CancellationToken cancellationToken = default)
    {
        if (pesticideId <= 0)
            throw new DomainException("Pesticide ID must be greater than zero.");

        return await _pesticideRepository.GetByIdAsync(pesticideId, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Creates a new pesticide entity with validation for name uniqueness.
    /// </summary>
    public async Task<Pesticide> CreatePesticideAsync(Pesticide pesticide, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pesticide);

        if (string.IsNullOrWhiteSpace(pesticide.Name))
            throw new DomainException("Pesticide name is required and cannot be empty.");

        await EnsurePesticideNameIsUniqueAsync(pesticide.Name, cancellationToken);

        StampForCreate(pesticide);

        await _pesticideRepository.CreateAsync(pesticide, true, cancellationToken);

        return pesticide;
    }

    /// <summary>
    /// Updates an existing pesticide with validation.
    /// </summary>
    public async Task<Pesticide> UpdatePesticideAsync(Pesticide pesticide, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pesticide);

        if (pesticide.Id <= 0)
            throw new DomainException("Pesticide ID is required for update.");

        if (string.IsNullOrWhiteSpace(pesticide.Name))
            throw new DomainException("Pesticide name is required and cannot be empty.");

        var existingPesticide = await EnsurePesticideExistsAsync(pesticide.Id, cancellationToken);

        if (existingPesticide.Name.ToLower() != pesticide.Name.Trim().ToLower())
            await EnsurePesticideNameIsUniqueAsync(pesticide.Name, cancellationToken);

        existingPesticide.Name = pesticide.Name.Trim();
        existingPesticide.ActiveIngredient = pesticide.ActiveIngredient?.Trim();
        existingPesticide.SafetyNotes = pesticide.SafetyNotes?.Trim();

        StampForUpdate(existingPesticide);

        await _pesticideRepository.UpdateAsync(existingPesticide, true, cancellationToken);

        return existingPesticide;
    }

    /// <summary>
    /// Deletes a pesticide if it has no active references in DiseaseRemedyLink.
    /// </summary>
    public async Task DeletePesticideAsync(int pesticideId, CancellationToken cancellationToken = default)
    {
        if (pesticideId <= 0)
            throw new DomainException("Pesticide ID must be greater than zero.");

        var pesticide = await EnsurePesticideExistsAsync(pesticideId, cancellationToken);

        var referencingLinks = await _remedyLinkRepository.FindAsync(
            rl => rl.PesticideId == pesticideId,
            false,
            true,
            cancellationToken);

        if (referencingLinks.Count > 0)
            throw new DomainException(
                $"Cannot delete pesticide '{pesticide.Name}' because it is referenced by {referencingLinks.Count} active remedy link(s).");

        await _pesticideRepository.DeleteAsync(pesticide, true, true, cancellationToken);
    }

    #endregion

    #region Public Methods - DiseaseRemedyLink Child Operations

    /// <summary>
    /// Gets all remedy links for a specific pesticide.
    /// </summary>
    public async Task<IList<DiseaseRemedyLink>> GetRemedyLinksAsync(int pesticideId, CancellationToken cancellationToken = default)
    {
        await EnsurePesticideExistsAsync(pesticideId, cancellationToken);

        var remedyLinks = await _remedyLinkRepository.FindAsync(
            rl => rl.PesticideId == pesticideId,
            false,
            true,
            cancellationToken);

        return remedyLinks.ToList();
    }

    /// <summary>
    /// Links a pesticide to a disease as a remedy option.
    /// </summary>
    public async Task<DiseaseRemedyLink> AddRemedyLinkAsync(int pesticideId, DiseaseRemedyLink remedyLink, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(remedyLink);

        await EnsurePesticideExistsAsync(pesticideId, cancellationToken);
        await EnsureDiseaseExistsAsync(remedyLink.DiseaseId, cancellationToken);

        remedyLink.PesticideId = pesticideId;
        remedyLink.TreatmentType = remedyLink.TreatmentType;

        await EnsureRemedyLinkIsUniqueAsync(pesticideId, remedyLink.DiseaseId, cancellationToken);
        StampForCreate(remedyLink);

        await _remedyLinkRepository.CreateAsync(remedyLink, true, cancellationToken);

        return remedyLink;
    }

    /// <summary>
    /// Updates an existing disease-remedy link.
    /// </summary>
    public async Task<DiseaseRemedyLink> UpdateRemedyLinkAsync(int pesticideId, DiseaseRemedyLink remedyLink, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(remedyLink);

        if (remedyLink.Id <= 0)
            throw new DomainException("Remedy link ID is required for update.");

        var existingLink = await _remedyLinkRepository.GetByIdAsync(remedyLink.Id, cancellationToken: cancellationToken);

        if (existingLink == null || existingLink.PesticideId != pesticideId)
            throw new DomainException("Remedy link not found or pesticide ID mismatch.");

        if (existingLink.DiseaseId != remedyLink.DiseaseId)
        {
            await EnsureDiseaseExistsAsync(remedyLink.DiseaseId, cancellationToken);
            await EnsureRemedyLinkIsUniqueAsync(pesticideId, remedyLink.DiseaseId, cancellationToken);
        }

        existingLink.DiseaseId = remedyLink.DiseaseId;
        existingLink.TreatmentType = remedyLink.TreatmentType;
        existingLink.Dosage = remedyLink.Dosage?.Trim();
        existingLink.Frequency = remedyLink.Frequency?.Trim();
        existingLink.Notes = remedyLink.Notes?.Trim();

        StampForUpdate(existingLink);

        await _remedyLinkRepository.UpdateAsync(existingLink, true, cancellationToken);

        return existingLink;
    }

    /// <summary>
    /// Removes a disease-remedy link.
    /// </summary>
    public async Task DeleteRemedyLinkAsync(int pesticideId, int remedyLinkId, CancellationToken cancellationToken = default)
    {
        if (remedyLinkId <= 0)
            throw new DomainException("Remedy link ID must be greater than zero.");

        var remedyLink = await _remedyLinkRepository.GetByIdAsync(remedyLinkId, cancellationToken: cancellationToken);

        if (remedyLink == null || remedyLink.PesticideId != pesticideId)
            throw new DomainException("Remedy link not found or pesticide ID mismatch.");

        await _remedyLinkRepository.DeleteAsync(remedyLink, true, true, cancellationToken);
    }

    #endregion

    #region Private Methods - Validation & Domain Guards

    /// <summary>
    /// Validates that a pesticide exists, throwing an exception if not found.
    /// </summary>
    private async Task<Pesticide> EnsurePesticideExistsAsync(int pesticideId, CancellationToken cancellationToken = default)
    {
        var pesticide = await _pesticideRepository.GetByIdAsync(pesticideId, cancellationToken: cancellationToken);

        if (pesticide == null)
            throw new DomainException($"Pesticide with ID '{pesticideId}' not found.");

        return pesticide;
    }

    /// <summary>
    /// Validates that a disease exists, throwing an exception if not found.
    /// </summary>
    private async Task<Disease> EnsureDiseaseExistsAsync(int diseaseId, CancellationToken cancellationToken = default)
    {
        var disease = await _diseaseRepository.GetByIdAsync(diseaseId, cancellationToken: cancellationToken);

        if (disease == null)
            throw new DomainException($"Disease with ID '{diseaseId}' not found.");

        return disease;
    }

    /// <summary>
    /// Validates that a pesticide name is unique across all pesticides.
    /// </summary>
    private async Task EnsurePesticideNameIsUniqueAsync(string pesticideName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(pesticideName))
            throw new DomainException("Pesticide name cannot be empty for uniqueness validation.");

        var normalizedName = pesticideName.Trim().ToLower();
        var existingPesticides = await _pesticideRepository.FindAsync(
            p => p.Name.ToLower() == normalizedName,
            false,
            true,
            cancellationToken);

        if (existingPesticides.Count > 0)
            throw new DomainException($"A pesticide with the name '{pesticideName}' already exists.");
    }

    /// <summary>
    /// Validates that a disease-pesticide remedy link is unique.
    /// </summary>
    private async Task EnsureRemedyLinkIsUniqueAsync(int pesticideId, int diseaseId, CancellationToken cancellationToken = default)
    {
        var existingLinks = await _remedyLinkRepository.FindAsync(
            rl => rl.PesticideId == pesticideId && rl.DiseaseId == diseaseId,
            false,
            true,
            cancellationToken);

        if (existingLinks.Count > 0)
            throw new DomainException("A remedy link already exists for this pesticide-disease pair.");
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
