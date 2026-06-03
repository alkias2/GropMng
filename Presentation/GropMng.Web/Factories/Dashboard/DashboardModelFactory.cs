using GropMng.Core.Domain.Garden.Care;
using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Health;
using GropMng.Core.Domain.Garden.Locations;
using GropMng.Core.Domain.Garden.Plants;
using GropMng.Core.Caching;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.Localization;
using GropMng.Core.Interfaces.Services.Media;
using GropMng.Core.Interfaces.Services.User;
using GropMng.Services.Caching.Dashboard;
using GropMng.Web.Initialization.Options;
using GropMng.Web.Models.Dashboard;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace GropMng.Web.Factories.Dashboard;

/// <summary>
/// Default implementation for preparing the owner dashboard view model.
/// Encapsulates all data loading, schedule resolution, and due-date calculation logic.
/// </summary>
public class DashboardModelFactory : IDashboardModelFactory
{
    /// <summary>
    /// Internal split result used to keep today/overdue actions and upcoming actions clearly separated.
    /// </summary>
    private sealed class ActionBuckets
    {
        public IList<DashboardActionModel> TodayOverdueActions { get; init; } = new List<DashboardActionModel>();

        public IList<DashboardActionModel> UpcomingActions { get; init; } = new List<DashboardActionModel>();
    }

    #region Fields

    private readonly ICurrentOwnerProvider _currentOwnerProvider;
    private readonly IRepository<PlantInstance> _plantInstanceRepository;
    private readonly IRepository<Plant> _plantRepository;
    private readonly IRepository<GardenSpot> _gardenSpotRepository;
    private readonly IRepository<Location> _locationRepository;
    private readonly IRepository<WateringSchedule> _wateringScheduleRepository;
    private readonly IRepository<FertilizingSchedule> _fertilizingScheduleRepository;
    private readonly IRepository<Fertilizer> _fertilizerRepository;
    private readonly IRepository<WateringLog> _wateringLogRepository;
    private readonly IRepository<FertilizingLog> _fertilizingLogRepository;
    private readonly IRepository<PlantDiseaseRecord> _plantDiseaseRecordRepository;
    private readonly IRepository<Disease> _diseaseRepository;
    private readonly IRepository<ActionSkip> _actionSkipRepository;
    private readonly IRepository<PlantPhoto> _plantPhotoRepository;
    private readonly IGropStaticCacheManager _staticCacheManager;
    private readonly IPictureService _pictureService;
    private readonly ILocalizationService _localizationService;
    private readonly DashboardOptions _dashboardOptions;

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
        IRepository<Fertilizer> fertilizerRepository,
        IRepository<WateringLog> wateringLogRepository,
        IRepository<FertilizingLog> fertilizingLogRepository,
        IRepository<PlantDiseaseRecord> plantDiseaseRecordRepository,
        IRepository<Disease> diseaseRepository,
        IRepository<ActionSkip> actionSkipRepository,
        IRepository<PlantPhoto> plantPhotoRepository,
        IGropStaticCacheManager staticCacheManager,
        IPictureService pictureService,
        ILocalizationService localizationService,
        IOptions<DashboardOptions> dashboardOptions)
    {
        _currentOwnerProvider = currentOwnerProvider;
        _plantInstanceRepository = plantInstanceRepository;
        _plantRepository = plantRepository;
        _gardenSpotRepository = gardenSpotRepository;
        _locationRepository = locationRepository;
        _wateringScheduleRepository = wateringScheduleRepository;
        _fertilizingScheduleRepository = fertilizingScheduleRepository;
        _fertilizerRepository = fertilizerRepository;
        _wateringLogRepository = wateringLogRepository;
        _fertilizingLogRepository = fertilizingLogRepository;
        _plantDiseaseRecordRepository = plantDiseaseRecordRepository;
        _diseaseRepository = diseaseRepository;
        _actionSkipRepository = actionSkipRepository;
        _plantPhotoRepository = plantPhotoRepository;
        _staticCacheManager = staticCacheManager;
        _pictureService = pictureService;
        _localizationService = localizationService;
        _dashboardOptions = dashboardOptions.Value;
    }

    #endregion

    #region Public

    /// <inheritdoc />
    public async Task<OwnerDashboardModel> PrepareDashboardModelAsync(
        DashboardQueryModel? query = null,
        CancellationToken cancellationToken = default)
    {
        // 1) Resolve owner/context and normalize UI query input.
        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);
        var model = new OwnerDashboardModel();
        var dashboardQuery = NormalizeQuery(query);
        model.Query = dashboardQuery;

        // 2) Resolve time boundaries used for due-date classification.
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var season = ResolveCurrentSeason(today);
        var upcomingHorizonDays = NormalizeHorizonDays(_dashboardOptions.UpcomingHorizonDays);

        // 3) Load plant instances first; if none exist, return an empty dashboard model.
        var plantInstances = await _staticCacheManager.GetAsync(
            _staticCacheManager.PrepareKey(DashboardCacheDefaults.PlantInstancesCacheKey, ownerId.ToString("N")),
            () => _plantInstanceRepository.GetAllAsync(
                query => query
                    .Where(pi => pi.OwnerId == ownerId && pi.IsActive)
                    .OrderBy(pi => pi.Id),
                cancellationToken: cancellationToken));

        if (plantInstances.Count == 0)
            return model;

        var plantInstanceIds = plantInstances.Select(pi => pi.Id).ToHashSet();
        var plantIds = plantInstances.Select(pi => pi.PlantId).Distinct().ToList();
        var spotIds = plantInstances.Select(pi => pi.GardenSpotId).Distinct().ToList();

        // 4) Load lookup dictionaries required to project dashboard rows.
        var plantsLookupToken = BuildIntSetToken(plantIds);
        var spotsLookupToken = BuildIntSetToken(spotIds);

        var plants = await _staticCacheManager.GetAsync(
            _staticCacheManager.PrepareKey(DashboardCacheDefaults.PlantsLookupCacheKey, plantsLookupToken),
            () => _plantRepository.GetByIdsAsync(plantIds, cancellationToken: cancellationToken));

        var spots = await _staticCacheManager.GetAsync(
            _staticCacheManager.PrepareKey(DashboardCacheDefaults.GardenSpotsLookupCacheKey, spotsLookupToken),
            () => _gardenSpotRepository.GetByIdsAsync(spotIds, cancellationToken: cancellationToken));

        var locationIds = spots.Select(s => s.LocationId).Distinct().ToList();
        var locationsLookupToken = BuildIntSetToken(locationIds);
        var locations = await _staticCacheManager.GetAsync(
            _staticCacheManager.PrepareKey(DashboardCacheDefaults.LocationsLookupCacheKey, locationsLookupToken),
            () => _locationRepository.GetByIdsAsync(locationIds, cancellationToken: cancellationToken));

        var plantMap = plants.ToDictionary(p => p.Id);
        var spotMap = spots.ToDictionary(s => s.Id);
        var locationMap = locations.ToDictionary(l => l.Id);
        var instanceMap = plantInstances.ToDictionary(pi => pi.Id);

        // 5) Load seasonal schedules for watering and fertilizing.
        var ownerToken = ownerId.ToString("N");
        var seasonToken = season.ToString();

        var wateringSchedules = await _staticCacheManager.GetAsync(
            _staticCacheManager.PrepareKey(DashboardCacheDefaults.WateringSchedulesCacheKey, ownerToken, seasonToken),
            () => _wateringScheduleRepository.GetAllAsync(
                query => query.Where(s => s.OwnerId == ownerId
                    && (s.Season == season || s.Season == GardenSeason.AllYear)),
                cancellationToken: cancellationToken));

        wateringSchedules = wateringSchedules
            .Where(schedule => plantInstanceIds.Contains(schedule.PlantInstanceId))
            .ToList();

        var fertilizingSchedules = await _staticCacheManager.GetAsync(
            _staticCacheManager.PrepareKey(DashboardCacheDefaults.FertilizingSchedulesCacheKey, ownerToken, seasonToken),
            () => _fertilizingScheduleRepository.GetAllAsync(
                query => query.Where(s => s.OwnerId == ownerId
                    && (s.Season == season || s.Season == GardenSeason.AllYear)),
                cancellationToken: cancellationToken));

        fertilizingSchedules = fertilizingSchedules
            .Where(schedule => plantInstanceIds.Contains(schedule.PlantInstanceId))
            .ToList();

        var fertilizerIds = fertilizingSchedules.Select(s => s.FertilizerId).Distinct().ToList();
        var fertilizersLookupToken = BuildIntSetToken(fertilizerIds);
        var fertilizers = await _staticCacheManager.GetAsync(
            _staticCacheManager.PrepareKey(DashboardCacheDefaults.FertilizersLookupCacheKey, fertilizersLookupToken),
            () => _fertilizerRepository.GetByIdsAsync(fertilizerIds, cancellationToken: cancellationToken));
        var fertilizerMap = fertilizers.ToDictionary(f => f.Id, f => f.Name);

        // 6) Load latest execution logs used to calculate next due dates.
        var wateringLogs = await _staticCacheManager.GetAsync(
            _staticCacheManager.PrepareKey(DashboardCacheDefaults.WateringLogsCacheKey, ownerToken),
            () => _wateringLogRepository.GetAllAsync(
                query => query.Where(l => l.OwnerId == ownerId),
                cancellationToken: cancellationToken));

        wateringLogs = wateringLogs
            .Where(log => plantInstanceIds.Contains(log.PlantInstanceId))
            .ToList();

        var fertilizingLogs = await _staticCacheManager.GetAsync(
            _staticCacheManager.PrepareKey(DashboardCacheDefaults.FertilizingLogsCacheKey, ownerToken),
            () => _fertilizingLogRepository.GetAllAsync(
                query => query.Where(l => l.OwnerId == ownerId),
                cancellationToken: cancellationToken));

        fertilizingLogs = fertilizingLogs
            .Where(log => plantInstanceIds.Contains(log.PlantInstanceId))
            .ToList();

        var latestWateringByInstance = wateringLogs
            .GroupBy(l => l.PlantInstanceId)
            .ToDictionary(g => g.Key, g => g.MaxBy(x => x.WateredAtUtc)!.WateredAtUtc);

        var latestFertilizingByInstance = fertilizingLogs
            .GroupBy(l => l.PlantInstanceId)
            .ToDictionary(g => g.Key, g => g.MaxBy(x => x.AppliedAtUtc)!.AppliedAtUtc);

        // 7) Load active skips and build a fast lookup to hide skipped actions.
        var activeSkips = await _staticCacheManager.GetAsync(
            _staticCacheManager.PrepareKey(DashboardCacheDefaults.ActiveSkipsCacheKey, ownerToken, today.ToString("yyyyMMdd")),
            () => _actionSkipRepository.GetAllAsync(
                query => query.Where(s => s.OwnerId == ownerId && s.ActiveUntilDate >= today),
                cancellationToken: cancellationToken));

        var skipSet = activeSkips
            .Select(s => (s.PlantInstanceId, s.ActionType))
            .ToHashSet();

        // 8) Build raw watering action rows.
        var wateringRows = new List<DashboardActionModel>();
        foreach (var schedule in wateringSchedules)
        {
            if (!instanceMap.TryGetValue(schedule.PlantInstanceId, out var instance)) continue;
            var dueDate = CalculateDueDate(latestWateringByInstance, schedule.PlantInstanceId, schedule.FrequencyDays, today);
            wateringRows.Add(BuildActionRow(instance, DashboardActionType.Watering, schedule.FrequencyDays, schedule.Season,
                waterAmountL: schedule.WaterAmountL, fertilizerQuantity: null, fertilizerUnit: null,
                fertilizerName: null,
                dueDate, today, plantMap, spotMap, locationMap));
        }

        // 9) Build raw fertilizing action rows.
        var fertilizingRows = new List<DashboardActionModel>();
        foreach (var schedule in fertilizingSchedules)
        {
            if (!instanceMap.TryGetValue(schedule.PlantInstanceId, out var instance)) continue;
            var dueDate = CalculateDueDate(latestFertilizingByInstance, schedule.PlantInstanceId, schedule.FrequencyDays, today);
            fertilizingRows.Add(BuildActionRow(instance, DashboardActionType.Fertilizing, schedule.FrequencyDays, schedule.Season,
                waterAmountL: null, fertilizerQuantity: schedule.Quantity, fertilizerUnit: schedule.Unit,
                fertilizerName: fertilizerMap.TryGetValue(schedule.FertilizerId, out var fertilizerName) ? fertilizerName : null,
                dueDate, today, plantMap, spotMap, locationMap));
        }

        // 10) Remove skipped actions and sort by urgency.
        var actionableWatering = wateringRows
            .Where(a => !skipSet.Contains((a.PlantInstanceId, ActionSkipType.Watering)))
            .OrderBy(a => a.DueStatus)
            .ThenBy(a => a.DueDate)
            .ThenBy(a => a.PlantName)
            .ToList();

        var actionableFertilizing = fertilizingRows
            .Where(a => !skipSet.Contains((a.PlantInstanceId, ActionSkipType.Fertilizing)))
            .OrderBy(a => a.DueStatus)
            .ThenBy(a => a.DueDate)
            .ThenBy(a => a.PlantName)
            .ToList();

        var allSpotsText = await _localizationService.GetResourceAsync("dashboard.owner.filter.allspots");
        var inDaysTemplate = await _localizationService.GetResourceAsync("dashboard.owner.group.inxdays");

        // 11) Build spot filter options from actionable rows.
        var spotFilterSourceActions = actionableWatering.Concat(actionableFertilizing).ToList();
        model.AvailableGardenSpots = BuildGardenSpotFilter(spotFilterSourceActions, spotMap, allSpotsText, dashboardQuery.SpotId);

        // 12) Apply optional spot filter to all actionable rows.
        if (dashboardQuery.SpotId.HasValue)
        {
            actionableWatering = actionableWatering
                .Where(a => a.GardenSpotId == dashboardQuery.SpotId.Value)
                .ToList();

            actionableFertilizing = actionableFertilizing
                .Where(a => a.GardenSpotId == dashboardQuery.SpotId.Value)
                .ToList();
        }

        // 13) EXPLICIT CALCULATION: today's/overdue and upcoming actions for watering.
        var wateringBuckets = SplitActionRowsByTodayAndUpcoming(actionableWatering, upcomingHorizonDays);

        // 14) EXPLICIT CALCULATION: today's/overdue and upcoming actions for fertilizing.
        var fertilizingBuckets = SplitActionRowsByTodayAndUpcoming(actionableFertilizing, upcomingHorizonDays);

        model.WateringTab = BuildWateringTabModel(
            wateringBuckets,
            today,
            inDaysTemplate);

        model.FertilizingTab = BuildFertilizingTabModel(
            fertilizingBuckets,
            today,
            inDaysTemplate);

        model.WateringTab.AvailableGardenSpots = model.AvailableGardenSpots;
        model.FertilizingTab.AvailableGardenSpots = model.AvailableGardenSpots;

        var actionsForPhotos = model.WateringTab.TodayOverdueActions
            .Concat(model.FertilizingTab.TodayOverdueActions)
            .Concat(model.WateringTab.UpcomingActionGroups.SelectMany(g => g.Actions))
            .Concat(model.FertilizingTab.UpcomingActionGroups.SelectMany(g => g.Actions));

        await PopulateActionPhotosAsync(ownerId, actionsForPhotos, cancellationToken);

        // 15) EXPLICIT CALCULATION: disease actions/cases for today and upcoming.
        model.DiseaseTab = await PrepareDiseaseTabAsync(
            ownerId, today, plantInstanceIds, instanceMap, plantMap, spotMap, locationMap, cancellationToken);

        return model;
    }

    #endregion

    #region Private

    /// <summary>
    /// Maps current date to a season used by seasonal schedules.
    /// </summary>
    private static GardenSeason ResolveCurrentSeason(DateOnly today)
        => today.Month switch
        {
            >= 3 and <= 5 => GardenSeason.Spring,
            >= 6 and <= 8 => GardenSeason.Summer,
            >= 9 and <= 11 => GardenSeason.Autumn,
            _ => GardenSeason.Winter
        };

    /// <summary>
    /// Calculates next due date from the latest action log or defaults to today when no history exists.
    /// </summary>
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

    /// <summary>
    /// Normalizes the dashboard query to avoid invalid spot selections.
    /// </summary>
    private static DashboardQueryModel NormalizeQuery(DashboardQueryModel? query)
    {
        if (query is null)
            return new DashboardQueryModel();

        return new DashboardQueryModel
        {
            SpotId = query.SpotId is > 0 ? query.SpotId : null
        };
    }

    /// <summary>
    /// Ensures upcoming horizon configuration stays within safe UI bounds.
    /// </summary>
    private static int NormalizeHorizonDays(int rawHorizonDays)
    {
        if (rawHorizonDays < 1)
            return 14;

        return rawHorizonDays > 60 ? 60 : rawHorizonDays;
    }

    /// <summary>
    /// Splits action rows into today's/overdue bucket and upcoming bucket (bounded by horizon days).
    /// </summary>
    private static ActionBuckets SplitActionRowsByTodayAndUpcoming(
        IList<DashboardActionModel> actions,
        int horizonDays)
    {
        var todayOverdue = actions
            .Where(a => a.DueStatus == DashboardDueStatus.Overdue || a.DueStatus == DashboardDueStatus.Today)
            .OrderBy(a => a.DueStatus)
            .ThenBy(a => a.DueDate)
            .ThenBy(a => a.PlantName)
            .ToList();

        var upcoming = actions
            .Where(a => a.DueStatus == DashboardDueStatus.Upcoming)
            .Where(a => a.DeltaDaysFromToday <= horizonDays)
            .OrderBy(a => a.DueDate)
            .ThenBy(a => a.PlantName)
            .ToList();

        return new ActionBuckets
        {
            TodayOverdueActions = todayOverdue,
            UpcomingActions = upcoming
        };
    }

    /// <summary>
    /// Builds watering tab model from already split action buckets.
    /// </summary>
    private static DashboardWateringTabModel BuildWateringTabModel(
        ActionBuckets buckets,
        DateOnly today,
        string inDaysTemplate)
    {
        var model = new DashboardWateringTabModel();

        model.TodayOverdueActions = buckets.TodayOverdueActions;
        model.TodayOverdueCount = buckets.TodayOverdueActions.Count;
        model.UpcomingCount = buckets.UpcomingActions.Count;
        model.UpcomingActionGroups = BuildUpcomingGroups(buckets.UpcomingActions, today, inDaysTemplate);

        return model;
    }

    /// <summary>
    /// Builds fertilizing tab model from already split action buckets.
    /// </summary>
    private static DashboardFertilizingTabModel BuildFertilizingTabModel(
        ActionBuckets buckets,
        DateOnly today,
        string inDaysTemplate)
    {
        var model = new DashboardFertilizingTabModel();

        model.TodayOverdueActions = buckets.TodayOverdueActions;
        model.TodayOverdueCount = buckets.TodayOverdueActions.Count;
        model.UpcomingCount = buckets.UpcomingActions.Count;
        model.UpcomingActionGroups = BuildUpcomingGroups(buckets.UpcomingActions, today, inDaysTemplate);

        return model;
    }

    /// <summary>
    /// Groups upcoming actions by due date and creates localized group captions.
    /// </summary>
    private static IList<DashboardActionGroupModel> BuildUpcomingGroups(
        IList<DashboardActionModel> upcoming,
        DateOnly today,
        string inDaysTemplate)
    {
        return upcoming
            .GroupBy(a => a.DueDate)
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var deltaDays = g.Key.DayNumber - today.DayNumber;
                return new DashboardActionGroupModel
                {
                    DueDate = g.Key,
                    DeltaDaysFromToday = deltaDays,
                    GroupLabel = string.Format(inDaysTemplate, deltaDays),
                    Actions = g.ToList()
                };
            })
            .ToList();
    }

    /// <summary>
    /// Projects a single action schedule row into the common dashboard action model.
    /// </summary>
    private static DashboardActionModel BuildActionRow(
        PlantInstance instance,
        DashboardActionType type,
        byte frequencyDays,
        GardenSeason season,
        decimal? waterAmountL,
        decimal? fertilizerQuantity,
        GropMng.Core.Domain.Garden.Enums.FertilizerQuantityUnit? fertilizerUnit,
        string? fertilizerName,
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
            DueStatus = dueStatus,
            DeltaDaysFromToday = dueDate.DayNumber - today.DayNumber,
            WaterAmountL = waterAmountL,
            FertilizerQuantity = fertilizerQuantity,
            FertilizerQuantityUnit = fertilizerUnit,
            FertilizerName = fertilizerName
        };
    }

    /// <summary>
    /// Builds garden spot filter options from the currently actionable rows.
    /// </summary>
    private static IList<SelectListItem> BuildGardenSpotFilter(
        IList<DashboardActionModel> actions,
        IReadOnlyDictionary<int, GardenSpot> spotMap,
        string allSpotsText,
        int? selectedSpotId)
    {
        var items = new List<SelectListItem>
        {
            new() { Value = "", Text = allSpotsText, Selected = !selectedSpotId.HasValue }
        };

        var distinctSpots = actions
            .Select(a => a.GardenSpotId)
            .Distinct()
            .Select(id => (id, Name: spotMap.TryGetValue(id, out var s) ? s.Name : "—"))
            .OrderBy(x => x.Name)
            .ToList();

        items.AddRange(distinctSpots.Select(x => new SelectListItem
        {
            Value = x.id.ToString(),
            Text = x.Name,
            Selected = selectedSpotId.HasValue && x.id == selectedSpotId.Value
        }));

        return items;
    }

    /// <summary>
    /// Resolves and attaches main plant photos to each action row.
    /// </summary>
    private async Task PopulateActionPhotosAsync(
        Guid ownerId,
        IEnumerable<DashboardActionModel> actions,
        CancellationToken cancellationToken)
    {
        var actionList = actions.ToList();
        if (actionList.Count == 0) return;

        var instanceIds = actionList.Select(a => a.PlantInstanceId).Distinct().ToHashSet();

        var allPhotos = await _plantPhotoRepository.GetAllAsync(
            query => query
                .Where(p => p.OwnerId == ownerId && instanceIds.Contains(p.PlantInstanceId)),
            cancellationToken: cancellationToken);

        // GroupBy in-memory: EF Core does not reliably translate GroupBy+First to SQL Server
        var photoPictureIdByInstance = allPhotos
            .GroupBy(p => p.PlantInstanceId)
            .ToDictionary(g => g.Key, g => g.OrderBy(p => p.DisplayOrder).First().PictureId);

        foreach (var action in actionList)
        {
            if (photoPictureIdByInstance.TryGetValue(action.PlantInstanceId, out var picId))
                action.PlantMainImageUrl = await _pictureService.GetPictureUrlAsync(picId, targetSize: 500);
        }
    }

    /// <summary>
    /// Loads active disease records and explicitly splits them into today/upcoming buckets.
    /// </summary>
    private async Task<DashboardDiseaseTabModel> PrepareDiseaseTabAsync(
        Guid ownerId,
        DateOnly today,
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

        // Keep ActiveCases for backward compatibility with existing views.
        tab.ActiveCases = activeCases;

        // Explicit disease split for dashboard analytics parity with other tabs.
        tab.TodayCases = activeCases
            .Where(c => c.DiagnosedOn <= today)
            .OrderByDescending(c => c.DiagnosedOn)
            .ThenBy(c => c.PlantName)
            .ToList();

        tab.UpcomingCases = activeCases
            .Where(c => c.DiagnosedOn > today)
            .OrderBy(c => c.DiagnosedOn)
            .ThenBy(c => c.PlantName)
            .ToList();

        tab.TodayCount = tab.TodayCases.Count;
        tab.UpcomingCount = tab.UpcomingCases.Count;

        return tab;
    }

    private static string BuildIntSetToken(IEnumerable<int> ids)
    {
        var values = ids
            .Distinct()
            .OrderBy(id => id)
            .ToArray();

        return values.Length == 0
            ? "none"
            : string.Join('-', values);
    }

    #endregion
}
