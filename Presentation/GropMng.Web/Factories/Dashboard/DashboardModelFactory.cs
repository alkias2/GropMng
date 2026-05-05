using GropMng.Core.Domain.Garden.Care;
using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Health;
using GropMng.Core.Domain.Garden.Locations;
using GropMng.Core.Domain.Garden.Plants;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.Localization;
using GropMng.Core.Interfaces.Services.Media;
using GropMng.Core.Interfaces.Services.User;
using GropMng.Web.Models.Dashboard;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GropMng.Web.Factories.Dashboard;

/// <summary>
/// Default implementation for preparing the owner dashboard view model.
/// Encapsulates all data loading, schedule resolution, and due-date calculation logic.
/// </summary>
public class DashboardModelFactory : IDashboardModelFactory
{
    #region Fields

    private readonly ICurrentOwnerProvider _currentOwnerProvider;
    private readonly IRepository<PlantInstance> _plantInstanceRepository;
    private readonly IRepository<Plant> _plantRepository;
    private readonly IRepository<GardenSpot> _gardenSpotRepository;
    private readonly IRepository<Location> _locationRepository;
    private readonly IRepository<WateringSchedule> _wateringScheduleRepository;
    private readonly IRepository<FertilizingSchedule> _fertilizingScheduleRepository;
    private readonly IRepository<WateringLog> _wateringLogRepository;
    private readonly IRepository<FertilizingLog> _fertilizingLogRepository;
    private readonly IRepository<PlantDiseaseRecord> _plantDiseaseRecordRepository;
    private readonly IRepository<Disease> _diseaseRepository;
    private readonly IRepository<ActionSkip> _actionSkipRepository;
    private readonly IRepository<PlantPhoto> _plantPhotoRepository;
    private readonly IPictureService _pictureService;
    private readonly ILocalizationService _localizationService;

    #endregion

    #region Ctor

    public DashboardModelFactory(
        ICurrentOwnerProvider currentOwnerProvider,
        IRepository<PlantInstance> plantInstanceRepository,
        IRepository<Plant> plantRepository,
        IRepository<GardenSpot> gardenSpotRepository,
        IRepository<Location> locationRepository,
        IRepository<WateringSchedule> wateringScheduleRepository,
        IRepository<FertilizingSchedule> fertilizingScheduleRepository,
        IRepository<WateringLog> wateringLogRepository,
        IRepository<FertilizingLog> fertilizingLogRepository,
        IRepository<PlantDiseaseRecord> plantDiseaseRecordRepository,
        IRepository<Disease> diseaseRepository,
        IRepository<ActionSkip> actionSkipRepository,
        IRepository<PlantPhoto> plantPhotoRepository,
        IPictureService pictureService,
        ILocalizationService localizationService)
    {
        _currentOwnerProvider = currentOwnerProvider;
        _plantInstanceRepository = plantInstanceRepository;
        _plantRepository = plantRepository;
        _gardenSpotRepository = gardenSpotRepository;
        _locationRepository = locationRepository;
        _wateringScheduleRepository = wateringScheduleRepository;
        _fertilizingScheduleRepository = fertilizingScheduleRepository;
        _wateringLogRepository = wateringLogRepository;
        _fertilizingLogRepository = fertilizingLogRepository;
        _plantDiseaseRecordRepository = plantDiseaseRecordRepository;
        _diseaseRepository = diseaseRepository;
        _actionSkipRepository = actionSkipRepository;
        _plantPhotoRepository = plantPhotoRepository;
        _pictureService = pictureService;
        _localizationService = localizationService;
    }

    #endregion

    #region Public

    /// <inheritdoc />
    public async Task<OwnerDashboardModel> PrepareDashboardModelAsync(CancellationToken cancellationToken = default)
    {
        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);
        var model = new OwnerDashboardModel();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var season = ResolveCurrentSeason(today);

        var plantInstances = await _plantInstanceRepository.GetAllAsync(
            query => query
                .Where(pi => pi.OwnerId == ownerId && pi.IsActive)
                .OrderBy(pi => pi.Id),
            cancellationToken: cancellationToken);

        if (plantInstances.Count == 0)
            return model;

        var plantInstanceIds = plantInstances.Select(pi => pi.Id).ToHashSet();
        var plantIds = plantInstances.Select(pi => pi.PlantId).Distinct().ToList();
        var spotIds = plantInstances.Select(pi => pi.GardenSpotId).Distinct().ToList();

        var plants = await _plantRepository.GetByIdsAsync(plantIds, cancellationToken: cancellationToken);
        var spots = await _gardenSpotRepository.GetByIdsAsync(spotIds, cancellationToken: cancellationToken);
        var locationIds = spots.Select(s => s.LocationId).Distinct().ToList();
        var locations = await _locationRepository.GetByIdsAsync(locationIds, cancellationToken: cancellationToken);

        var plantMap = plants.ToDictionary(p => p.Id);
        var spotMap = spots.ToDictionary(s => s.Id);
        var locationMap = locations.ToDictionary(l => l.Id);
        var instanceMap = plantInstances.ToDictionary(pi => pi.Id);

        // Schedules
        var wateringSchedules = await _wateringScheduleRepository.GetAllAsync(
            query => query.Where(s => s.OwnerId == ownerId
                && plantInstanceIds.Contains(s.PlantInstanceId)
                && (s.Season == season || s.Season == GardenSeason.AllYear)),
            cancellationToken: cancellationToken);

        var fertilizingSchedules = await _fertilizingScheduleRepository.GetAllAsync(
            query => query.Where(s => s.OwnerId == ownerId
                && plantInstanceIds.Contains(s.PlantInstanceId)
                && (s.Season == season || s.Season == GardenSeason.AllYear)),
            cancellationToken: cancellationToken);

        // Last log dates
        var wateringLogs = await _wateringLogRepository.GetAllAsync(
            query => query.Where(l => l.OwnerId == ownerId && plantInstanceIds.Contains(l.PlantInstanceId)),
            cancellationToken: cancellationToken);

        var fertilizingLogs = await _fertilizingLogRepository.GetAllAsync(
            query => query.Where(l => l.OwnerId == ownerId && plantInstanceIds.Contains(l.PlantInstanceId)),
            cancellationToken: cancellationToken);

        var latestWateringByInstance = wateringLogs
            .GroupBy(l => l.PlantInstanceId)
            .ToDictionary(g => g.Key, g => g.MaxBy(x => x.WateredAtUtc)!.WateredAtUtc);

        var latestFertilizingByInstance = fertilizingLogs
            .GroupBy(l => l.PlantInstanceId)
            .ToDictionary(g => g.Key, g => g.MaxBy(x => x.AppliedAtUtc)!.AppliedAtUtc);

        // Active skips
        var activeSkips = await _actionSkipRepository.GetAllAsync(
            query => query.Where(s => s.OwnerId == ownerId && s.ActiveUntilDate >= today),
            cancellationToken: cancellationToken);

        var skipSet = activeSkips
            .Select(s => (s.PlantInstanceId, s.ActionType))
            .ToHashSet();

        // Build watering action rows
        var wateringRows = new List<DashboardActionModel>();
        foreach (var schedule in wateringSchedules)
        {
            if (!instanceMap.TryGetValue(schedule.PlantInstanceId, out var instance)) continue;
            var dueDate = CalculateDueDate(latestWateringByInstance, schedule.PlantInstanceId, schedule.FrequencyDays, today);
            wateringRows.Add(BuildActionRow(instance, DashboardActionType.Watering, schedule.FrequencyDays, schedule.Season, dueDate, today, plantMap, spotMap, locationMap));
        }

        // Build fertilizing action rows
        var fertilizingRows = new List<DashboardActionModel>();
        foreach (var schedule in fertilizingSchedules)
        {
            if (!instanceMap.TryGetValue(schedule.PlantInstanceId, out var instance)) continue;
            var dueDate = CalculateDueDate(latestFertilizingByInstance, schedule.PlantInstanceId, schedule.FrequencyDays, today);
            fertilizingRows.Add(BuildActionRow(instance, DashboardActionType.Fertilizing, schedule.FrequencyDays, schedule.Season, dueDate, today, plantMap, spotMap, locationMap));
        }

        var actionableWatering = wateringRows
            .Where(a => a.DueStatus == DashboardDueStatus.Overdue || a.DueStatus == DashboardDueStatus.Today)
            .Where(a => !skipSet.Contains((a.PlantInstanceId, ActionSkipType.Watering)))
            .OrderBy(a => a.DueStatus)
            .ThenBy(a => a.DueDate)
            .ThenBy(a => a.PlantName)
            .ToList();

        var actionableFertilizing = fertilizingRows
            .Where(a => a.DueStatus == DashboardDueStatus.Overdue || a.DueStatus == DashboardDueStatus.Today)
            .Where(a => !skipSet.Contains((a.PlantInstanceId, ActionSkipType.Fertilizing)))
            .OrderBy(a => a.DueStatus)
            .ThenBy(a => a.DueDate)
            .ThenBy(a => a.PlantName)
            .ToList();

        // Load plant photos in a single batch across both tabs
        await PopulateActionPhotosAsync(ownerId, actionableWatering.Concat(actionableFertilizing), cancellationToken);

        var allSpotsText = await _localizationService.GetResourceAsync("dashboard.owner.filter.allspots");

        model.WateringTab.Actions = actionableWatering;
        model.WateringTab.AvailableGardenSpots = BuildGardenSpotFilter(actionableWatering, spotMap, allSpotsText);

        model.FertilizingTab.Actions = actionableFertilizing;
        model.FertilizingTab.AvailableGardenSpots = BuildGardenSpotFilter(actionableFertilizing, spotMap, allSpotsText);

        model.DiseaseTab = await PrepareDiseaseTabAsync(
            ownerId, plantInstanceIds, instanceMap, plantMap, spotMap, locationMap, cancellationToken);

        return model;
    }

    #endregion

    #region Private

    private static GardenSeason ResolveCurrentSeason(DateOnly today)
        => today.Month switch
        {
            >= 3 and <= 5 => GardenSeason.Spring,
            >= 6 and <= 8 => GardenSeason.Summer,
            >= 9 and <= 11 => GardenSeason.Autumn,
            _ => GardenSeason.Winter
        };

    private static DateOnly CalculateDueDate(
        IReadOnlyDictionary<int, DateTime> latestByInstance,
        int plantInstanceId,
        byte frequencyDays,
        DateOnly today)
    {
        if (!latestByInstance.TryGetValue(plantInstanceId, out var latestDateTime))
            return today;

        return DateOnly.FromDateTime(latestDateTime).AddDays(frequencyDays);
    }

    private static DashboardActionModel BuildActionRow(
        PlantInstance instance,
        DashboardActionType type,
        byte frequencyDays,
        GardenSeason season,
        DateOnly dueDate,
        DateOnly today,
        IReadOnlyDictionary<int, Plant> plantMap,
        IReadOnlyDictionary<int, GardenSpot> spotMap,
        IReadOnlyDictionary<int, Location> locationMap)
    {
        var spot = spotMap.TryGetValue(instance.GardenSpotId, out var spotValue) ? spotValue : null;
        var locationName = spot != null && locationMap.TryGetValue(spot.LocationId, out var location)
            ? location.Name
            : "—";

        var dueStatus = dueDate < today
            ? DashboardDueStatus.Overdue
            : dueDate == today
                ? DashboardDueStatus.Today
                : DashboardDueStatus.Upcoming;

        return new DashboardActionModel
        {
            PlantInstanceId = instance.Id,
            GardenSpotId = instance.GardenSpotId,
            PlantName = plantMap.TryGetValue(instance.PlantId, out var plant) ? plant.ScientificName : "—",
            Nickname = instance.Nickname,
            LocationName = locationName,
            GardenSpotName = spot?.Name ?? "—",
            ActionType = type,
            FrequencyDays = frequencyDays,
            Season = season,
            DueDate = dueDate,
            DueStatus = dueStatus
        };
    }

    private static IList<SelectListItem> BuildGardenSpotFilter(
        IList<DashboardActionModel> actions,
        IReadOnlyDictionary<int, GardenSpot> spotMap,
        string allSpotsText)
    {
        var items = new List<SelectListItem>
        {
            new() { Value = "", Text = allSpotsText, Selected = true }
        };

        var distinctSpots = actions
            .Select(a => a.GardenSpotId)
            .Distinct()
            .Select(id => (id, Name: spotMap.TryGetValue(id, out var s) ? s.Name : "—"))
            .OrderBy(x => x.Name)
            .ToList();

        items.AddRange(distinctSpots.Select(x => new SelectListItem { Value = x.id.ToString(), Text = x.Name }));

        return items;
    }

    private async Task PopulateActionPhotosAsync(
        Guid ownerId,
        IEnumerable<DashboardActionModel> actions,
        CancellationToken cancellationToken)
    {
        var actionList = actions.ToList();
        if (actionList.Count == 0) return;

        var instanceIds = actionList.Select(a => a.PlantInstanceId).Distinct().ToHashSet();

        var mainPhotos = await _plantPhotoRepository.GetAllAsync(
            query => query
                .Where(p => p.OwnerId == ownerId && instanceIds.Contains(p.PlantInstanceId))
                .GroupBy(p => p.PlantInstanceId)
                .Select(g => g.OrderBy(p => p.DisplayOrder).First()),
            cancellationToken: cancellationToken);

        var photoPictureIdByInstance = mainPhotos.ToDictionary(p => p.PlantInstanceId, p => p.PictureId);

        foreach (var action in actionList)
        {
            if (photoPictureIdByInstance.TryGetValue(action.PlantInstanceId, out var picId))
                action.PlantMainImageUrl = await _pictureService.GetPictureUrlAsync(picId, targetSize: 500);
        }
    }

    private async Task<DashboardDiseaseTabModel> PrepareDiseaseTabAsync(
        Guid ownerId,
        HashSet<int> plantInstanceIds,
        IReadOnlyDictionary<int, PlantInstance> instanceMap,
        IReadOnlyDictionary<int, Plant> plantMap,
        IReadOnlyDictionary<int, GardenSpot> spotMap,
        IReadOnlyDictionary<int, Location> locationMap,
        CancellationToken cancellationToken)
    {
        var tab = new DashboardDiseaseTabModel();

        var records = await _plantDiseaseRecordRepository.GetAllAsync(
            query => query.Where(r => r.OwnerId == ownerId
                && plantInstanceIds.Contains(r.PlantInstanceId)
                && (r.Outcome == null || r.Outcome == PlantDiseaseOutcome.Ongoing)),
            cancellationToken: cancellationToken);

        if (records.Count == 0) return tab;

        var diseaseIds = records.Select(r => r.DiseaseId).Distinct().ToList();
        var diseases = await _diseaseRepository.GetByIdsAsync(diseaseIds, cancellationToken: cancellationToken);
        var diseaseMap = diseases.ToDictionary(d => d.Id);

        var activeCases = new List<DashboardDiseaseModel>();
        foreach (var record in records.OrderBy(r => r.DetectedDate))
        {
            if (!instanceMap.TryGetValue(record.PlantInstanceId, out var instance)) continue;

            var spot = spotMap.TryGetValue(instance.GardenSpotId, out var spotValue) ? spotValue : null;
            var locationName = spot != null && locationMap.TryGetValue(spot.LocationId, out var location)
                ? location.Name
                : "—";

            activeCases.Add(new DashboardDiseaseModel
            {
                PlantInstanceId = instance.Id,
                PlantName = plantMap.TryGetValue(instance.PlantId, out var plant) ? plant.ScientificName : "—",
                Nickname = instance.Nickname,
                DiseaseName = diseaseMap.TryGetValue(record.DiseaseId, out var disease) ? disease.Name : "—",
                DiagnosedOn = record.DetectedDate,
                LocationName = locationName,
                GardenSpotName = spot?.Name ?? "—",
                Severity = record.Severity
            });
        }

        tab.ActiveCases = activeCases;
        return tab;
    }

    #endregion
}
