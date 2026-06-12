using GropMng.Web.Areas.Admin.Models;

namespace GropMng.Web.Areas.Admin.Factories;

/// <summary>
/// Defines the contract for preparing view models for the admin notification list.
/// </summary>
public interface IAdminNotificationModelFactory
{
    /// <summary>
    /// Prepares the list model for the admin notification grid.
    /// </summary>
    /// <param name="showResolved">Whether to include resolved notifications.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of notification list models.</returns>
    Task<List<AdminNotificationListModel>> PrepareListModelAsync(bool showResolved = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Syncs missing admin notifications from PlantProblemRecords that have NotifyAdmin set but no corresponding AdminNotification.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SyncMissingAsync(CancellationToken cancellationToken = default);
}