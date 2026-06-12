using GropMng.Core.Caching;
using GropMng.Core.Common.Exceptions;
using GropMng.Core.Domain.Garden.Health;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.Garden.Health;
using GropMng.Services.Caching;

namespace GropMng.Services.Services.Garden.Health;

/// <summary>
/// Manages the disease knowledge base (admin encyclopedia).
/// </summary>
public class DiseaseKnowledgeService : IDiseaseKnowledgeService
{
    #region Fields

    private readonly IRepository<DiseaseKnowledge> _knowledgeRepository;
    private readonly IRepository<DiseaseKnowledgePhoto> _photoRepository;
    private readonly IRepository<PlantProblemRecord> _problemRecordRepository;
    private readonly IRepository<AdminNotification> _notificationRepository;
    private readonly IGropStaticCacheManager _staticCacheManager;

    #endregion

    #region Ctor

    public DiseaseKnowledgeService(
        IRepository<DiseaseKnowledge> knowledgeRepository,
        IRepository<DiseaseKnowledgePhoto> photoRepository,
        IRepository<PlantProblemRecord> problemRecordRepository,
        IRepository<AdminNotification> notificationRepository,
        IGropStaticCacheManager staticCacheManager)
    {
        _knowledgeRepository = knowledgeRepository ?? throw new ArgumentNullException(nameof(knowledgeRepository));
        _photoRepository = photoRepository ?? throw new ArgumentNullException(nameof(photoRepository));
        _problemRecordRepository = problemRecordRepository ?? throw new ArgumentNullException(nameof(problemRecordRepository));
        _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
        _staticCacheManager = staticCacheManager ?? throw new ArgumentNullException(nameof(staticCacheManager));
    }

    #endregion

    #region Public

    /// <inheritdoc />
    public async Task<List<DiseaseKnowledge>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var results = await _knowledgeRepository.GetAllAsync(
            query => query.Where(d => !d.IsDeleted).OrderBy(d => d.CommonName),
            cancellationToken: cancellationToken);

        return results.ToList();
    }

    /// <inheritdoc />
    public async Task<DiseaseKnowledge> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var knowledge = await _knowledgeRepository.FirstOrDefaultAsync(
            entity => entity.Id == id && !entity.IsDeleted,
            cancellationToken: cancellationToken);

        return knowledge ?? throw new DomainException($"DiseaseKnowledge with id '{id}' was not found.");
    }

    /// <inheritdoc />
    public async Task<List<DiseaseKnowledge>> SearchAsync(string term, int? plantId = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(term);

        var normalizedTerm = term.Trim().ToLowerInvariant();

        var results = await _knowledgeRepository.GetAllAsync(
            query => query.Where(d => !d.IsDeleted
                && (d.CommonName.ToLower().Contains(normalizedTerm)
                    || (d.ScientificName != null && d.ScientificName.ToLower().Contains(normalizedTerm))))
                .OrderBy(d => d.CommonName),
            cancellationToken: cancellationToken);

        return results.ToList();
    }

    /// <inheritdoc />
    public async Task<DiseaseKnowledge> CreateAsync(DiseaseKnowledge knowledge, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(knowledge);
        ValidateKnowledge(knowledge);

        await EnsureCommonNameIsUniqueAsync(knowledge.CommonName, null, cancellationToken);

        var now = DateTime.UtcNow;
        knowledge.CreatedAtUtc = now;
        knowledge.UpdatedAtUtc = now;
        knowledge.IsDeleted = false;
        knowledge.DeletedAtUtc = null;

        var created = await _knowledgeRepository.CreateAsync(knowledge, cancellationToken: cancellationToken);
        await ClearKnowledgeCacheAsync();
        return created;
    }

    /// <inheritdoc />
    public async Task<DiseaseKnowledge> UpdateAsync(DiseaseKnowledge knowledge, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(knowledge);
        ValidateKnowledge(knowledge);

        var existing = await GetByIdAsync(knowledge.Id, cancellationToken);

        await EnsureCommonNameIsUniqueAsync(knowledge.CommonName, knowledge.Id, cancellationToken);

        existing.CommonName = knowledge.CommonName.Trim();
        existing.ScientificName = knowledge.ScientificName?.Trim();
        existing.Description = knowledge.Description;
        existing.TreatmentGuidelines = knowledge.TreatmentGuidelines;
        existing.UpdatedAtUtc = DateTime.UtcNow;

        var updated = await _knowledgeRepository.UpdateAsync(existing, cancellationToken: cancellationToken);
        await ClearKnowledgeCacheAsync();
        return updated;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var knowledge = await GetByIdAsync(id, cancellationToken);

        var now = DateTime.UtcNow;

        // Unlink all PlantProblemRecords that reference this knowledge entry
        var linkedRecords = await _problemRecordRepository.GetAllAsync(
            query => query.Where(r => r.DiseaseKnowledgeId == id && !r.IsDeleted),
            cancellationToken: cancellationToken);

        foreach (var record in linkedRecords)
        {
            record.DiseaseKnowledgeId = null;
            record.UpdatedAtUtc = now;
            await _problemRecordRepository.UpdateAsync(record, cancellationToken: cancellationToken);
        }

        // Unresolve all AdminNotifications that reference this knowledge entry
        var linkedNotifications = await _notificationRepository.GetAllAsync(
            query => query.Where(n => n.DiseaseKnowledgeId == id && !n.IsDeleted),
            cancellationToken: cancellationToken);

        foreach (var notification in linkedNotifications)
        {
            notification.IsResolved = false;
            notification.ResolvedAtUtc = null;
            notification.DiseaseKnowledgeId = null;
            notification.UpdatedAtUtc = now;
            await _notificationRepository.UpdateAsync(notification, cancellationToken: cancellationToken);
        }

        knowledge.IsDeleted = true;
        knowledge.DeletedAtUtc = now;
        knowledge.UpdatedAtUtc = now;

        await _knowledgeRepository.UpdateAsync(knowledge, cancellationToken: cancellationToken);
        await ClearKnowledgeCacheAsync();
    }

    /// <inheritdoc />
    public async Task<DiseaseKnowledgePhoto> AddPhotoAsync(DiseaseKnowledgePhoto photo, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(photo);

        await GetByIdAsync(photo.DiseaseKnowledgeId, cancellationToken);

        var now = DateTime.UtcNow;
        photo.CreatedAtUtc = now;
        photo.UpdatedAtUtc = now;
        photo.IsDeleted = false;
        photo.DeletedAtUtc = null;

        var created = await _photoRepository.CreateAsync(photo, cancellationToken: cancellationToken);
        await ClearKnowledgeCacheAsync();
        return created;
    }

    /// <inheritdoc />
    public async Task DeletePhotoAsync(int id, CancellationToken cancellationToken = default)
    {
        var photo = await _photoRepository.FirstOrDefaultAsync(
            entity => entity.Id == id && !entity.IsDeleted,
            cancellationToken: cancellationToken);

        if (photo is null)
            throw new DomainException($"DiseaseKnowledgePhoto with id '{id}' was not found.");

        photo.IsDeleted = true;
        photo.DeletedAtUtc = DateTime.UtcNow;
        photo.UpdatedAtUtc = DateTime.UtcNow;

        await _photoRepository.UpdateAsync(photo, cancellationToken: cancellationToken);
        await ClearKnowledgeCacheAsync();
    }

    #endregion

    #region Privates

    private static void ValidateKnowledge(DiseaseKnowledge knowledge)
    {
        if (string.IsNullOrWhiteSpace(knowledge.CommonName))
            throw new DomainException("Common name is required.");

        if (knowledge.CommonName.Length > 300)
            throw new DomainException($"Common name must not exceed 300 characters (was {knowledge.CommonName.Length}).");

        if (string.IsNullOrWhiteSpace(knowledge.Description))
            throw new DomainException("Description is required.");

        if (string.IsNullOrWhiteSpace(knowledge.TreatmentGuidelines))
            throw new DomainException("Treatment guidelines are required.");
    }

    private async Task EnsureCommonNameIsUniqueAsync(string commonName, int? excludedId, CancellationToken cancellationToken)
    {
        var normalizedName = commonName.Trim().ToLowerInvariant();

        var exists = await _knowledgeRepository.AnyAsync(
            entity => entity.CommonName.ToLower() == normalizedName
                && !entity.IsDeleted
                && (!excludedId.HasValue || entity.Id != excludedId.Value),
            cancellationToken: cancellationToken);

        if (exists)
            throw new DomainException($"A disease with common name '{commonName}' already exists.");
    }

    private async Task ClearKnowledgeCacheAsync()
    {
        await _staticCacheManager.RemoveByPrefixAsync(ProblemCacheDefaults.ProblemRecordPrefix);
    }

    #endregion
}