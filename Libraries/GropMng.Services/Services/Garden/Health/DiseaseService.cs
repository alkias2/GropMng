using GropMng.Core;
using GropMng.Core.Common.Exceptions;
using GropMng.Core.Domain.Garden;
using GropMng.Core.Domain.Garden.Health;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.Garden.Health;

namespace GropMng.Services.Services.Garden.Health;

/// <summary>
/// Provides aggregate-root operations for diseases and their remedy links.
/// </summary>
public class DiseaseService : IDiseaseService
{
    #region Fields

    private readonly IRepository<Disease> _diseaseRepository;
    private readonly IRepository<DiseaseRemedyLink> _remedyLinkRepository;
    private readonly IRepository<Pesticide> _pesticideRepository;

    #endregion

    #region Ctor

    /// <summary>
    /// Initializes a new instance of the <see cref="DiseaseService" /> class.
    /// </summary>
    /// <param name="diseaseRepository">The repository used to manage diseases.</param>
    /// <param name="remedyLinkRepository">The repository used to manage disease remedy links.</param>
    /// <param name="pesticideRepository">The repository used to validate pesticides referenced by remedy links.</param>
    public DiseaseService(
        IRepository<Disease> diseaseRepository,
        IRepository<DiseaseRemedyLink> remedyLinkRepository,
        IRepository<Pesticide> pesticideRepository)
    {
        _diseaseRepository = diseaseRepository ?? throw new ArgumentNullException(nameof(diseaseRepository));
        _remedyLinkRepository = remedyLinkRepository ?? throw new ArgumentNullException(nameof(remedyLinkRepository));
        _pesticideRepository = pesticideRepository ?? throw new ArgumentNullException(nameof(pesticideRepository));
    }

    #endregion

    #region Public

    /// <inheritdoc />
    public Task<IPagedList<Disease>> GetDiseasesAsync(string? searchTerm = null, int pageIndex = 0, int pageSize = int.MaxValue, CancellationToken cancellationToken = default)
    {
        return _diseaseRepository.GetPagedAsync(
            query =>
            {
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var term = searchTerm.Trim().ToLowerInvariant();
                    query = query.Where(disease =>
                        disease.Name.ToLower().Contains(term)
                        || (disease.Symptoms != null && disease.Symptoms.ToLower().Contains(term))
                        || (disease.Notes != null && disease.Notes.ToLower().Contains(term)));
                }

                return query.OrderBy(disease => disease.Name).ThenBy(disease => disease.Id);
            },
            pageIndex,
            pageSize,
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Disease?> GetDiseaseByIdAsync(int diseaseId, bool includeRemedyLinks = false, CancellationToken cancellationToken = default)
    {
        var disease = await _diseaseRepository.GetByIdAsync(diseaseId, cancellationToken: cancellationToken);
        if (disease is null || !includeRemedyLinks)
            return disease;

        disease.RemedyLinks = (await _remedyLinkRepository.GetAllAsync(
            query => query.Where(link => link.DiseaseId == diseaseId).OrderBy(link => link.Id),
            cancellationToken: cancellationToken)).ToList();

        return disease;
    }

    /// <inheritdoc />
    public async Task<Disease> CreateDiseaseAsync(Disease disease, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(disease);
        ValidateDisease(disease);
        await EnsureDiseaseNameIsUniqueAsync(disease.Name, null, cancellationToken);

        StampForCreate(disease);
        return await _diseaseRepository.CreateAsync(disease, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Disease> UpdateDiseaseAsync(Disease disease, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(disease);
        ValidateDisease(disease);

        var existingDisease = await EnsureDiseaseExistsAsync(disease.Id, cancellationToken);
        await EnsureDiseaseNameIsUniqueAsync(disease.Name, disease.Id, cancellationToken);

        existingDisease.Name = disease.Name.Trim();
        existingDisease.DiseaseType = disease.DiseaseType;
        existingDisease.Symptoms = disease.Symptoms?.Trim();
        existingDisease.PreventionNotes = disease.PreventionNotes?.Trim();
        existingDisease.AffectedParts = disease.AffectedParts?.Trim();
        existingDisease.Notes = disease.Notes?.Trim();
        StampForUpdate(existingDisease);

        return await _diseaseRepository.UpdateAsync(existingDisease, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteDiseaseAsync(int diseaseId, CancellationToken cancellationToken = default)
    {
        var disease = await EnsureDiseaseExistsAsync(diseaseId, cancellationToken);
        await _diseaseRepository.DeleteAsync(disease, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DiseaseRemedyLink>> GetRemedyLinksAsync(int diseaseId, CancellationToken cancellationToken = default)
    {
        await EnsureDiseaseExistsAsync(diseaseId, cancellationToken);

        return await _remedyLinkRepository.GetAllAsync(
            query => query.Where(link => link.DiseaseId == diseaseId).OrderBy(link => link.Id),
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<DiseaseRemedyLink> AddRemedyLinkAsync(int diseaseId, DiseaseRemedyLink remedyLink, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(remedyLink);

        await EnsureDiseaseExistsAsync(diseaseId, cancellationToken);
        await EnsurePesticideExistsAsync(remedyLink.PesticideId, cancellationToken);
        await EnsureRemedyLinkIsUniqueAsync(diseaseId, remedyLink.PesticideId, null, cancellationToken);

        remedyLink.DiseaseId = diseaseId;
        remedyLink.Dosage = remedyLink.Dosage?.Trim();
        remedyLink.Frequency = remedyLink.Frequency?.Trim();
        remedyLink.Notes = remedyLink.Notes?.Trim();
        StampForCreate(remedyLink);

        return await _remedyLinkRepository.CreateAsync(remedyLink, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<DiseaseRemedyLink> UpdateRemedyLinkAsync(int diseaseId, DiseaseRemedyLink remedyLink, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(remedyLink);

        await EnsureDiseaseExistsAsync(diseaseId, cancellationToken);
        await EnsurePesticideExistsAsync(remedyLink.PesticideId, cancellationToken);
        var existingRemedyLink = await EnsureRemedyLinkExistsAsync(diseaseId, remedyLink.Id, cancellationToken);
        await EnsureRemedyLinkIsUniqueAsync(diseaseId, remedyLink.PesticideId, remedyLink.Id, cancellationToken);

        existingRemedyLink.PesticideId = remedyLink.PesticideId;
        existingRemedyLink.TreatmentType = remedyLink.TreatmentType;
        existingRemedyLink.Dosage = remedyLink.Dosage?.Trim();
        existingRemedyLink.Frequency = remedyLink.Frequency?.Trim();
        existingRemedyLink.Notes = remedyLink.Notes?.Trim();
        StampForUpdate(existingRemedyLink);

        return await _remedyLinkRepository.UpdateAsync(existingRemedyLink, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteRemedyLinkAsync(int diseaseId, int remedyLinkId, CancellationToken cancellationToken = default)
    {
        await EnsureDiseaseExistsAsync(diseaseId, cancellationToken);
        var remedyLink = await EnsureRemedyLinkExistsAsync(diseaseId, remedyLinkId, cancellationToken);
        await _remedyLinkRepository.DeleteAsync(remedyLink, cancellationToken: cancellationToken);
    }

    #endregion

    #region Privates

    private async Task<Disease> EnsureDiseaseExistsAsync(int diseaseId, CancellationToken cancellationToken)
    {
        var disease = await _diseaseRepository.GetByIdAsync(diseaseId, cancellationToken: cancellationToken);
        return disease ?? throw new DomainException($"Disease with id '{diseaseId}' was not found.");
    }

    private async Task<DiseaseRemedyLink> EnsureRemedyLinkExistsAsync(int diseaseId, int remedyLinkId, CancellationToken cancellationToken)
    {
        var remedyLink = await _remedyLinkRepository.FirstOrDefaultAsync(
            entity => entity.Id == remedyLinkId && entity.DiseaseId == diseaseId,
            cancellationToken: cancellationToken);

        return remedyLink ?? throw new DomainException($"Remedy link with id '{remedyLinkId}' was not found for disease '{diseaseId}'.");
    }

    private async Task EnsurePesticideExistsAsync(int pesticideId, CancellationToken cancellationToken)
    {
        var pesticide = await _pesticideRepository.GetByIdAsync(pesticideId, cancellationToken: cancellationToken);
        if (pesticide is null)
            throw new DomainException($"Pesticide with id '{pesticideId}' was not found.");
    }

    private async Task EnsureDiseaseNameIsUniqueAsync(string diseaseName, int? excludedDiseaseId, CancellationToken cancellationToken)
    {
        var normalizedName = diseaseName.Trim().ToLowerInvariant();
        var exists = await _diseaseRepository.AnyAsync(
            entity => entity.Name.ToLower() == normalizedName
                && (!excludedDiseaseId.HasValue || entity.Id != excludedDiseaseId.Value),
            cancellationToken: cancellationToken);

        if (exists)
            throw new DomainException($"A disease with name '{diseaseName}' already exists.");
    }

    private async Task EnsureRemedyLinkIsUniqueAsync(int diseaseId, int pesticideId, int? excludedRemedyLinkId, CancellationToken cancellationToken)
    {
        var exists = await _remedyLinkRepository.AnyAsync(
            entity => entity.DiseaseId == diseaseId
                && entity.PesticideId == pesticideId
                && (!excludedRemedyLinkId.HasValue || entity.Id != excludedRemedyLinkId.Value),
            cancellationToken: cancellationToken);

        if (exists)
            throw new DomainException($"Pesticide '{pesticideId}' is already linked to disease '{diseaseId}'.");
    }

    private static void ValidateDisease(Disease disease)
    {
        if (string.IsNullOrWhiteSpace(disease.Name))
            throw new DomainException("Name is required.");
    }

    private static void StampForCreate(AuditableEntity entity)
    {
        var now = DateTime.UtcNow;
        entity.CreatedAtUtc = now;
        entity.UpdatedAtUtc = now;
        entity.IsDeleted = false;
        entity.DeletedAtUtc = null;
    }

    private static void StampForUpdate(AuditableEntity entity)
    {
        entity.UpdatedAtUtc = DateTime.UtcNow;
    }

    #endregion
}