using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.Garden.Health;
using GropMng.Web.Areas.Admin.Models;

namespace GropMng.Web.Areas.Admin.Factories;

/// <summary>
/// Default implementation for AdminNotification model preparation.
/// </summary>
public class AdminNotificationModelFactory : IAdminNotificationModelFactory
{
    private readonly IAdminNotificationService _notificationService;
    private readonly IRepository<GropMng.Core.Domain.Garden.Plants.PlantInstance> _plantInstanceRepository;

    public AdminNotificationModelFactory(
        IAdminNotificationService notificationService,
        IRepository<GropMng.Core.Domain.Garden.Plants.PlantInstance> plantInstanceRepository)
    {
        _notificationService = notificationService;
        _plantInstanceRepository = plantInstanceRepository;
    }

    /// <inheritdoc />
    public Task SyncMissingAsync(CancellationToken cancellationToken = default)
    {
        return _notificationService.SyncMissingNotificationsAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<List<AdminNotificationListModel>> PrepareListModelAsync(
        bool showResolved = false,
        CancellationToken cancellationToken = default)
    {
        var notifications = await _notificationService.GetAllAsync(showResolved, cancellationToken);

        if (notifications.Count == 0)
            return new List<AdminNotificationListModel>();

        var instanceIds = notifications.Select(n => n.PlantInstanceId).Distinct().ToList();
        var instances = await _plantInstanceRepository.GetByIdsAsync(instanceIds, cancellationToken: cancellationToken);
        var instanceMap = instances.ToDictionary(i => i.Id);

        return notifications.Select(n => new AdminNotificationListModel
        {
            Id = n.Id,
            ProblemName = n.ProblemName,
            OwnerName = n.OwnerId.ToString("N"),
            PlantInstanceName = instanceMap.TryGetValue(n.PlantInstanceId, out var instance)
                ? instance.Nickname ?? instance.Plant?.ScientificName ?? $"#{n.PlantInstanceId}"
                : $"#{n.PlantInstanceId}",
            CreatedAtUtc = n.CreatedAtUtc,
            IsResolved = n.IsResolved,
            DiseaseKnowledgeId = n.DiseaseKnowledgeId,
            ShowResolved = showResolved
        }).ToList();
    }
}