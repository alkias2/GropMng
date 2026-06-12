using GropMng.Core.Caching;
using GropMng.Core.Common.Exceptions;
using GropMng.Core.Domain.Garden.Health;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.Garden.Health;
using GropMng.Services.Caching;

namespace GropMng.Services.Services.Garden.Health;

/// <summary>
/// Manages admin notifications triggered by unknown disease reports.
/// </summary>
public class AdminNotificationService : IAdminNotificationService
{
    #region Fields

    private readonly IRepository<AdminNotification> _notificationRepository;
    private readonly IRepository<PlantProblemRecord> _problemRecordRepository;
    private readonly IGropStaticCacheManager _staticCacheManager;

    #endregion

    #region Ctor

    public AdminNotificationService(
        IRepository<AdminNotification> notificationRepository,
        IRepository<PlantProblemRecord> problemRecordRepository,
        IGropStaticCacheManager staticCacheManager)
    {
        _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
        _problemRecordRepository = problemRecordRepository ?? throw new ArgumentNullException(nameof(problemRecordRepository));
        _staticCacheManager = staticCacheManager ?? throw new ArgumentNullException(nameof(staticCacheManager));
    }

    #endregion

    #region Public

    /// <inheritdoc />
    public async Task<List<AdminNotification>> GetPendingAsync(CancellationToken cancellationToken = default)
    {
        var results = await _notificationRepository.GetAllAsync(
            query => query.Where(n => !n.IsResolved && !n.IsDeleted).OrderByDescending(n => n.CreatedAtUtc),
            cancellationToken: cancellationToken);
        return results.ToList();
    }

    /// <inheritdoc />
    public async Task<List<AdminNotification>> GetAllAsync(bool includeResolved, CancellationToken cancellationToken = default)
    {
        var results = await _notificationRepository.GetAllAsync(
            query =>
            {
                query = query.Where(n => !n.IsDeleted);
                if (!includeResolved)
                    query = query.Where(n => !n.IsResolved);
                return query.OrderByDescending(n => n.CreatedAtUtc);
            },
            cancellationToken: cancellationToken);
        return results.ToList();
    }

    /// <inheritdoc />
    public async Task<AdminNotification> CreateAsync(AdminNotification notification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);
        ValidateNotification(notification);

        var now = DateTime.UtcNow;
        notification.CreatedAtUtc = now;
        notification.UpdatedAtUtc = now;
        notification.IsDeleted = false;
        notification.DeletedAtUtc = null;

        var created = await _notificationRepository.CreateAsync(notification, cancellationToken: cancellationToken);
        await ClearCacheAsync();
        return created;
    }

    /// <inheritdoc />
    public async Task ResolveAsync(int id, int diseaseKnowledgeId, CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepository.FirstOrDefaultAsync(
            entity => entity.Id == id && !entity.IsDeleted,
            cancellationToken: cancellationToken);

        if (notification is null)
            throw new DomainException($"AdminNotification with id '{id}' was not found.");

        notification.IsResolved = true;
        notification.ResolvedAtUtc = DateTime.UtcNow;
        notification.DiseaseKnowledgeId = diseaseKnowledgeId;
        notification.UpdatedAtUtc = DateTime.UtcNow;

        await _notificationRepository.UpdateAsync(notification, cancellationToken: cancellationToken);
        await ClearCacheAsync();
    }

    /// <inheritdoc />
    public async Task SyncMissingNotificationsAsync(CancellationToken cancellationToken = default)
    {
        var records = await _problemRecordRepository.GetAllAsync(
            query => query.Where(r => r.NotifyAdmin && !r.IsDeleted),
            cancellationToken: cancellationToken);

        var existingNotifications = await _notificationRepository.GetAllAsync(
            query => query.Where(n => !n.IsDeleted),
            cancellationToken: cancellationToken);

        // Build lookup: problem name (lowercase) → record
        var recordByProblemName = records
            .GroupBy(r => r.ProblemName.Trim().ToLowerInvariant())
            .ToDictionary(g => g.Key, g => g.First());

        var now = DateTime.UtcNow;
        bool changed = false;

        // Phase 1: Sync existing notifications — resolve/unresolve based on record state
        foreach (var notification in existingNotifications)
        {
            var key = notification.ProblemName.Trim().ToLowerInvariant();

            if (!recordByProblemName.TryGetValue(key, out var record))
                continue;

            var recordHasKnowledge = record.DiseaseKnowledgeId.HasValue;

            // If the record has been linked to a DiseaseKnowledge but the notification is not resolved yet
            if (recordHasKnowledge && !notification.IsResolved)
            {
                notification.IsResolved = true;
                notification.ResolvedAtUtc = now;
                notification.DiseaseKnowledgeId = record.DiseaseKnowledgeId;
                notification.UpdatedAtUtc = now;
                await _notificationRepository.UpdateAsync(notification, cancellationToken: cancellationToken);
                changed = true;
            }
            // If the record no longer has a DiseaseKnowledgeId but the notification is still resolved
            else if (!recordHasKnowledge && notification.IsResolved)
            {
                notification.IsResolved = false;
                notification.ResolvedAtUtc = null;
                notification.DiseaseKnowledgeId = null;
                notification.UpdatedAtUtc = now;
                await _notificationRepository.UpdateAsync(notification, cancellationToken: cancellationToken);
                changed = true;
            }
            // If the record has a different DiseaseKnowledgeId than what's stored in the notification
            else if (recordHasKnowledge && notification.IsResolved
                && notification.DiseaseKnowledgeId != record.DiseaseKnowledgeId)
            {
                notification.DiseaseKnowledgeId = record.DiseaseKnowledgeId;
                notification.UpdatedAtUtc = now;
                await _notificationRepository.UpdateAsync(notification, cancellationToken: cancellationToken);
                changed = true;
            }
        }

        // Phase 2: Create missing notifications for records that don't have one
        var existingProblemNames = existingNotifications
            .Select(n => n.ProblemName.Trim().ToLowerInvariant())
            .ToHashSet();

        foreach (var record in records)
        {
            var key = record.ProblemName.Trim().ToLowerInvariant();
            if (existingProblemNames.Contains(key))
                continue;

            var notification = new AdminNotification
            {
                OwnerId = record.OwnerId,
                PlantInstanceId = record.PlantInstanceId,
                ProblemName = record.ProblemName.Trim(),
                IsResolved = record.DiseaseKnowledgeId.HasValue,
                DiseaseKnowledgeId = record.DiseaseKnowledgeId,
                CreatedAtUtc = record.CreatedAtUtc,
                UpdatedAtUtc = now,
                IsDeleted = false
            };

            if (record.DiseaseKnowledgeId.HasValue)
            {
                notification.ResolvedAtUtc = now;
            }

            await _notificationRepository.CreateAsync(notification, cancellationToken: cancellationToken);
            changed = true;
        }

        if (changed)
            await ClearCacheAsync();
    }

    #endregion

    #region Privates

    private static void ValidateNotification(AdminNotification notification)
    {
        if (string.IsNullOrWhiteSpace(notification.ProblemName))
            throw new DomainException("Problem name is required.");

        if (notification.ProblemName.Length > 300)
            throw new DomainException($"Problem name must not exceed 300 characters (was {notification.ProblemName.Length}).");
    }

    private async Task ClearCacheAsync()
    {
        await _staticCacheManager.RemoveByPrefixAsync(ProblemCacheDefaults.ProblemRecordPrefix);
    }

    #endregion
}