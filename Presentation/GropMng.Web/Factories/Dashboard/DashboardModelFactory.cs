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

    /// <summary>
    /// Immutable snapshot of the resolved owner context and lookup data required to build the dashboard.
    /// </summary>
    private sealed record DashboardContext(
        Guid OwnerId,
        DateOnly Today,
        GardenSeason Season,
        int HorizonDays,
        IReadOnlyList<PlantInstance> PlantInstances,
        IReadOnlyDictionary<int, Plant> PlantMap,
        IReadOnlyDictionary<int, GardenSpot> SpotMap,
        IReadOnlyDictionary<int, Location> LocationMap,
        HashSet<int> PlantInstanceIds);

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

    /// <summary>
    /// Loads only the counts needed for the dashboard counter cards.
    /// No projections, no photos, no SelectListItems, no group building.
    /// Uses the same cache keys as the individual tab methods to avoid duplicate DB queries.
    /// </summary>
    public async Task<DashboardCountersModel> PrepareCountersAsync(CancellationToken ct = default)
    {
        var model = new DashboardCountersModel();

        var context = await LoadDashboardContextAsync(ct);
        if (context is null)
            return model;

        var ownerToken = context.OwnerId.ToString("N");
        var seasonToken = context.Season.ToString();
        var todayToken = context.Today.ToString("yyyyMMdd");
        var plantInstanceIds = context.PlantInstanceIds;

        // ── Watering schedules ───────────────────────────────────────────────
        // Identical cache key to PrepareWateringTabAsync — cache hit guaranteed
        // if watering tab was already loaded in the same request cycle.
        var wateringSchedules = await _staticCacheManager.GetAsync(
            _staticCacheManager.PrepareKey(DashboardCacheDefaults.WateringSchedulesCacheKey, ownerToken, seasonToken),
            () => _wateringScheduleRepository.GetAllAsync(
                query => query.Where(s => s.OwnerId == context.OwnerId
                    && (s.Season == context.Season || s.Season == GardenSeason.AllYear)),
                cancellationToken: ct));

        wateringSchedules = wateringSchedules
            .Where(s => plantInstanceIds.Contains(s.PlantInstanceId))
            .ToList();

        // ── Fertilizing schedules ─────────────────────────────────────────────
        // Identical cache key to PrepareFertilizingTabAsync.
        var fertilizingSchedules = await _staticCacheManager.GetAsync(
            _staticCacheManager.PrepareKey(DashboardCacheDefaults.FertilizingSchedulesCacheKey, ownerToken, seasonToken),
            () => _fertilizingScheduleRepository.GetAllAsync(
                query => query.Where(s => s.OwnerId == context.OwnerId
                    && (s.Season == context.Season || s.Season == GardenSeason.AllYear)),
                cancellationToken: ct));

        fertilizingSchedules = fertilizingSchedules
            .Where(s => plantInstanceIds.Contains(s.PlantInstanceId))
            .ToList();

        // ── Watering logs ─────────────────────────────────────────────────────
        var wateringLogs = await _staticCacheManager.GetAsync(
            _staticCacheManager.PrepareKey(DashboardCacheDefaults.WateringLogsCacheKey, ownerToken),
            () => _wateringLogRepository.GetAllAsync(
                query => query.Where(l => l.OwnerId == context.OwnerId),
                cancellationToken: ct));

        wateringLogs = wateringLogs
            .Where(l => plantInstanceIds.Contains(l.PlantInstanceId))
            .ToList();

        // ── Fertilizing logs ──────────────────────────────────────────────────
        var fertilizingLogs = await _staticCacheManager.GetAsync(
            _staticCacheManager.PrepareKey(DashboardCacheDefaults.FertilizingLogsCacheKey, ownerToken),
            () => _fertilizingLogRepository.GetAllAsync(
                query => query.Where(l => l.OwnerId == context.OwnerId),
                cancellationToken: ct));

        fertilizingLogs = fertilizingLogs
            .Where(l => plantInstanceIds.Contains(l.PlantInstanceId))
            .ToList();

        // ── Active skips ──────────────────────────────────────────────────────
        // Identical cache key to both tab methods.
        var activeSkips = await _staticCacheManager.GetAsync(
            _staticCacheManager.PrepareKey(DashboardCacheDefaults.ActiveSkipsCacheKey, ownerToken, todayToken),
            () => _actionSkipRepository.GetAllAsync(
                query => query.Where(s => s.OwnerId == context.OwnerId && s.ActiveUntilDate >= context.Today),
                cancellationToken: ct));

        var skipSet = activeSkips
            .Select(s => (s.PlantInstanceId, s.ActionType))
            .ToHashSet();

        // ── Watering counter ──────────────────────────────────────────────────
        // Replicates exactly the due date logic of PrepareWateringTabAsync
        // but projects only to a count instead of full DashboardActionModel.
        var latestWateringByInstance = wateringLogs
            .GroupBy(l => l.PlantInstanceId)
            .ToDictionary(g => g.Key, g => g.MaxBy(x => x.WateredAtUtc)!.WateredAtUtc);

        model.WateringTodayOverdueCount = wateringSchedules
            .Where(s => !skipSet.Contains((s.PlantInstanceId, ActionSkipType.Watering)))
            .Count(s =>
            {
                var dueDate = CalculateDueDate(latestWateringByInstance, s.PlantInstanceId, s.FrequencyDays, context.Today);
                return dueDate <= context.Today;
            });

        // ── Fertilizing counter ───────────────────────────────────────────────
        var latestFertilizingByInstance = fertilizingLogs
            .GroupBy(l => l.PlantInstanceId)
            .ToDictionary(g => g.Key, g => g.MaxBy(x => x.AppliedAtUtc)!.AppliedAtUtc);

        model.FertilizingTodayOverdueCount = fertilizingSchedules
            .Where(s => !skipSet.Contains((s.PlantInstanceId, ActionSkipType.Fertilizing)))
            .Count(s =>
            {
                var dueDate = CalculateDueDate(latestFertilizingByInstance, s.PlantInstanceId, s.FrequencyDays, context.Today);
                return dueDate <= context.Today;
            });

        // ── Disease counter ───────────────────────────────────────────────────
        // Replicates the query of PrepareDiseaseTabInternalAsync
        // but projects only to a count — no disease name lookup needed.
        var diseaseRecords = await _plantDiseaseRecordRepository.GetAllAsync(
            query => query.Where(r => r.OwnerId == context.OwnerId
                && plantInstanceIds.Contains(r.PlantInstanceId)
                && (r.Outcome == null || r.Outcome == PlantDiseaseOutcome.Ongoing)),
            cancellationToken: ct);

        model.DiseaseTodayCount = diseaseRecords
            .Count(r => r.DetectedDate <= context.Today);

        return model;
    }

    /// <inheritdoc />
    public async Task<DashboardWateringTabModel> PrepareWateringTabAsync(
        DashboardQueryModel? query = null,
        CancellationToken ct = default)
    {
        var model = new DashboardWateringTabModel();
        var dashboardQuery = NormalizeQuery(query);

        var context = await LoadDashboardContextAsync(ct);
        if (context is null)
            return model;

        var ownerToken = context.OwnerId.ToString("N");
        var seasonToken = context.Season.ToString();
        var plantInstanceIds = context.PlantInstanceIds;
        var instanceMap = context.PlantInstances.ToDictionary(pi => pi.Id);

        var wateringSchedules = await _staticCacheManager.GetAsync(
            _staticCacheManager.PrepareKey(DashboardCacheDefaults.WateringSchedulesCacheKey, ownerToken, seasonToken),
            () => _wateringScheduleRepository.GetAllAsync(
                query => query.Where(s => s.OwnerId == context.OwnerId
                    && (s.Season == context.Season || s.Season == GardenSeason.AllYear)),
                cancellationToken: ct));

        wateringSchedules = wateringSchedules
            .Where(s => plantInstanceIds.Contains(s.PlantInstanceId))
            .ToList();

        var wateringLogs = await _staticCacheManager.GetAsync(
            _staticCacheManager.PrepareKey(DashboardCacheDefaults.WateringLogsCacheKey, ownerToken),
            () => _wateringLogRepository.GetAllAsync(
                query => query.Where(l => l.OwnerId == context.OwnerId),
                cancellationToken: ct));

        wateringLogs = wateringLogs
            .Where(l => plantInstanceIds.Contains(l.PlantInstanceId))
            .ToList();

        var activeSkips = await _staticCacheManager.GetAsync(
            _staticCacheManager.PrepareKey(DashboardCacheDefaults.ActiveSkipsCacheKey, ownerToken, context.Today.ToString("yyyyMMdd")),
            () => _actionSkipRepository.GetAllAsync(
                query => query.Where(s => s.OwnerId == context.OwnerId && s.ActiveUntilDate >= context.Today),
                cancellationToken: ct));

        var skipSet = activeSkips
            .Select(s => (s.PlantInstanceId, s.ActionType))
            .ToHashSet();

        var latestWateringByInstance = wateringLogs
            .GroupBy(l => l.PlantInstanceId)
            .ToDictionary(g => g.Key, g => g.MaxBy(x => x.WateredAtUtc)!.WateredAtUtc);

        var wateringRows = new List<DashboardActionModel>();
        foreach (var schedule in wateringSchedules)
        {
            if (!instanceMap.TryGetValue(schedule.PlantInstanceId, out var instance)) continue;
            var dueDate = CalculateDueDate(latestWateringByInstance, schedule.PlantInstanceId, schedule.FrequencyDays, context.Today);
            wateringRows.Add(BuildActionRow(instance, DashboardActionType.Watering, schedule.FrequencyDays, schedule.Season,
                waterAmountL: schedule.WaterAmountL, fertilizerQuantity: null, fertilizerUnit: null,
                fertilizerName: null,
                dueDate, context.Today, context.PlantMap, context.SpotMap, context.LocationMap));
        }

        var allSpotsText = await _localizationService.GetResourceAsync("dashboard.owner.filter.allspots");
        var inDaysTemplate = await _localizationService.GetResourceAsync("dashboard.owner.group.inxdays");

        var actionableWatering = wateringRows
            .Where(a => !skipSet.Contains((a.PlantInstanceId, ActionSkipType.Watering)))
            .OrderBy(a => a.DueStatus)
            .ThenBy(a => a.DueDate)
            .ThenBy(a => a.PlantName)
            .ToList();

        model.AvailableGardenSpots = BuildGardenSpotFilter(actionableWatering, context.SpotMap, allSpotsText, dashboardQuery.SpotId);

        if (dashboardQuery.SpotId.HasValue)
            actionableWatering = actionableWatering
                .Where(a => a.GardenSpotId == dashboardQuery.SpotId.Value)
                .ToList();

        var buckets = SplitActionRowsByTodayAndUpcoming(actionableWatering, context.HorizonDays);
        model = BuildWateringTabModel(buckets, context.Today, inDaysTemplate);
        model.AvailableGardenSpots = BuildGardenSpotFilter(actionableWatering, context.SpotMap, allSpotsText, dashboardQuery.SpotId);

        var actionsForPhotos = model.TodayOverdueActions
            .Concat(model.UpcomingActionGroups.SelectMany(g => g.Actions));
        await PopulateActionPhotosAsync(context.OwnerId, actionsForPhotos, ct);

        return model;
    }

    /// <inheritdoc />
    public async Task<DashboardFertilizingTabModel> PrepareFertilizingTabAsync(
        DashboardQueryModel? query = null,
        CancellationToken ct = default)
    {
        var model = new DashboardFertilizingTabModel();
        var dashboardQuery = NormalizeQuery(query);

        var context = await LoadDashboardContextAsync(ct);
        if (context is null)
            return model;

        var ownerToken = context.OwnerId.ToString("N");
        var seasonToken = context.Season.ToString();
        var plantInstanceIds = context.PlantInstanceIds;
        var instanceMap = context.PlantInstances.ToDictionary(pi => pi.Id);

        var fertilizingSchedules = await _staticCacheManager.GetAsync(
            _staticCacheManager.PrepareKey(DashboardCacheDefaults.FertilizingSchedulesCacheKey, ownerToken, seasonToken),
            () => _fertilizingScheduleRepository.GetAllAsync(
                query => query.Where(s => s.OwnerId == context.OwnerId
                    && (s.Season == context.Season || s.Season == GardenSeason.AllYear)),
                cancellationToken: ct));

        fertilizingSchedules = fertilizingSchedules
            .Where(s => plantInstanceIds.Contains(s.PlantInstanceId))
            .ToList();

        var fertilizerIds = fertilizingSchedules.Select(s => s.FertilizerId).Distinct().ToList();
        var fertilizersLookupToken = BuildIntSetToken(fertilizerIds);
        var fertilizers = await _staticCacheManager.GetAsync(
            _staticCacheManager.PrepareKey(DashboardCacheDefaults.FertilizersLookupCacheKey, fertilizersLookupToken),
            () => _fertilizerRepository.GetByIdsAsync(fertilizerIds, cancellationToken: ct));
        var fertilizerMap = fertilizers.ToDictionary(f => f.Id, f => f.Name);

        var fertilizingLogs = await _staticCacheManager.GetAsync(
            _staticCacheManager.PrepareKey(DashboardCacheDefaults.FertilizingLogsCacheKey, ownerToken),
            () => _fertilizingLogRepository.GetAllAsync(
                query => query.Where(l => l.OwnerId == context.OwnerId),
                cancellationToken: ct));

        fertilizingLogs = fertilizingLogs
            .Where(l => plantInstanceIds.Contains(l.PlantInstanceId))
            .ToList();

        var activeSkips = await _staticCacheManager.GetAsync(
            _staticCacheManager.PrepareKey(DashboardCacheDefaults.ActiveSkipsCacheKey, ownerToken, context.Today.ToString("yyyyMMdd")),
            () => _actionSkipRepository.GetAllAsync(
                query => query.Where(s => s.OwnerId == context.OwnerId && s.ActiveUntilDate >= context.Today),
                cancellationToken: ct));

        var skipSet = activeSkips
            .Select(s => (s.PlantInstanceId, s.ActionType))
            .ToHashSet();

        var latestFertilizingByInstance = fertilizingLogs
            .GroupBy(l => l.PlantInstanceId)
            .ToDictionary(g => g.Key, g => g.MaxBy(x => x.AppliedAtUtc)!.AppliedAtUtc);

        var fertilizingRows = new List<DashboardActionModel>();
        foreach (var schedule in fertilizingSchedules)
        {
            if (!instanceMap.TryGetValue(schedule.PlantInstanceId, out var instance)) continue;
            var dueDate = CalculateDueDate(latestFertilizingByInstance, schedule.PlantInstanceId, schedule.FrequencyDays, context.Today);
            fertilizingRows.Add(BuildActionRow(instance, DashboardActionType.Fertilizing, schedule.FrequencyDays, schedule.Season,
                waterAmountL: null, fertilizerQuantity: schedule.Quantity, fertilizerUnit: schedule.Unit,
                fertilizerName: fertilizerMap.TryGetValue(schedule.FertilizerId, out var fertilizerName) ? fertilizerName : null,
                dueDate, context.Today, context.PlantMap, context.SpotMap, context.LocationMap));
        }

        var allSpotsText = await _localizationService.GetResourceAsync("dashboard.owner.filter.allspots");
        var inDaysTemplate = await _localizationService.GetResourceAsync("dashboard.owner.group.inxdays");

        var actionableFertilizing = fertilizingRows
            .Where(a => !skipSet.Contains((a.PlantInstanceId, ActionSkipType.Fertilizing)))
            .OrderBy(a => a.DueStatus)
            .ThenBy(a => a.DueDate)
            .ThenBy(a => a.PlantName)
            .ToList();

        model.AvailableGardenSpots = BuildGardenSpotFilter(actionableFertilizing, context.SpotMap, allSpotsText, dashboardQuery.SpotId);

        if (dashboardQuery.SpotId.HasValue)
            actionableFertilizing = actionableFertilizing
                .Where(a => a.GardenSpotId == dashboardQuery.SpotId.Value)
                .ToList();

        var buckets = SplitActionRowsByTodayAndUpcoming(actionableFertilizing, context.HorizonDays);
        model = BuildFertilizingTabModel(buckets, context.Today, inDaysTemplate);
        model.AvailableGardenSpots = BuildGardenSpotFilter(actionableFertilizing, context.SpotMap, allSpotsText, dashboardQuery.SpotId);

        var actionsForPhotos = model.TodayOverdueActions
            .Concat(model.UpcomingActionGroups.SelectMany(g => g.Actions));
        await PopulateActionPhotosAsync(context.OwnerId, actionsForPhotos, ct);

        return model;
    }

    /// <inheritdoc />
    public async Task<DashboardDiseaseTabModel> PrepareDiseaseTabAsync(
        CancellationToken ct = default)
    {
        var context = await LoadDashboardContextAsync(ct);
        if (context is null)
            return new DashboardDiseaseTabModel();

        var instanceMap = context.PlantInstances.ToDictionary(pi => pi.Id);

        return await PrepareDiseaseTabInternalAsync(
            context.OwnerId,
            context.Today,
            context.PlantInstanceIds,
            instanceMap,
            context.PlantMap,
            context.SpotMap,
            context.LocationMap,
            ct);
    }

    #endregion

    #region Private

    /// <summary>
    /// Loads owner identity, resolves time boundaries, and fetches all lookup dictionaries
    /// required to build the dashboard. Returns <c>null</c> when the owner has no active plant instances.
    /// </summary>
    private async Task<DashboardContext?> LoadDashboardContextAsync(CancellationToken ct)
    {
        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(ct);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var season = ResolveCurrentSeason(today);
        var horizonDays = NormalizeHorizonDays(_dashboardOptions.UpcomingHorizonDays);

        var plantInstances = await _staticCacheManager.GetAsync(
            _staticCacheManager.PrepareKey(DashboardCacheDefaults.PlantInstancesCacheKey, ownerId.ToString("N")),
            () => _plantInstanceRepository.GetAllAsync(
                query => query
                    .Where(pi => pi.OwnerId == ownerId && pi.IsActive)
                    .OrderBy(pi => pi.Id),
                cancellationToken: ct));

        if (plantInstances.Count == 0)
            return null;

        var plantInstanceIds = plantInstances.Select(pi => pi.Id).ToHashSet();
        var plantIds = plantInstances.Select(pi => pi.PlantId).Distinct().ToList();
        var spotIds = plantInstances.Select(pi => pi.GardenSpotId).Distinct().ToList();

        var plantsLookupToken = BuildIntSetToken(plantIds);
        var spotsLookupToken = BuildIntSetToken(spotIds);

        var plants = await _staticCacheManager.GetAsync(
            _staticCacheManager.PrepareKey(DashboardCacheDefaults.PlantsLookupCacheKey, plantsLookupToken),
            () => _plantRepository.GetByIdsAsync(plantIds, cancellationToken: ct));

        var spots = await _staticCacheManager.GetAsync(
            _staticCacheManager.PrepareKey(DashboardCacheDefaults.GardenSpotsLookupCacheKey, spotsLookupToken),
            () => _gardenSpotRepository.GetByIdsAsync(spotIds, cancellationToken: ct));

        var locationIds = spots.Select(s => s.LocationId).Distinct().ToList();
        var locationsLookupToken = BuildIntSetToken(locationIds);
        var locations = await _staticCacheManager.GetAsync(
            _staticCacheManager.PrepareKey(DashboardCacheDefaults.LocationsLookupCacheKey, locationsLookupToken),
            () => _locationRepository.GetByIdsAsync(locationIds, cancellationToken: ct));

        return new DashboardContext(
            OwnerId: ownerId,
            Today: today,
            Season: season,
            HorizonDays: horizonDays,
            PlantInstances: plantInstances,
            PlantMap: plants.ToDictionary(p => p.Id),
            SpotMap: spots.ToDictionary(s => s.Id),
            LocationMap: locations.ToDictionary(l => l.Id),
            PlantInstanceIds: plantInstanceIds);
    }

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
    private async Task<DashboardDiseaseTabModel> PrepareDiseaseTabInternalAsync(
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
