using GropMng.Core.Domain.Garden.Health;

namespace GropMng.Core.Interfaces.Services.Garden.Health;

/// <summary>
/// Provides operations for managing admin notifications triggered by unknown disease reports.
/// </summary>
public interface IAdminNotificationService
{
    /// <summary>
    /// Retrieves all pending (unresolved) notifications.
    /// </summary>
    Task<List<AdminNotification>> GetPendingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all notifications, optionally including resolved ones.
    /// </summary>
    Task<List<AdminNotification>> GetAllAsync(bool includeResolved, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new admin notification for an unknown disease report.
    /// </summary>
    Task<AdminNotification> CreateAsync(AdminNotification notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a notification as resolved and links it to the created <see cref="DiseaseKnowledge"/> entry.
    /// Sets <see cref="AdminNotification.IsResolved"/> to <c>true</c>,
    /// <see cref="AdminNotification.ResolvedAtUtc"/> to <see cref="DateTime.UtcNow"/>,
    /// and <see cref="AdminNotification.DiseaseKnowledgeId"/> to <paramref name="diseaseKnowledgeId"/>.
    /// </summary>
    Task ResolveAsync(int id, int diseaseKnowledgeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ensures that every <see cref="PlantProblemRecord"/> with <see cref="PlantProblemRecord.NotifyAdmin"/> = <c>true</c>
    /// has a corresponding <see cref="AdminNotification"/>. Creates missing notifications.
    /// </summary>
    Task SyncMissingNotificationsAsync(CancellationToken cancellationToken = default);
}