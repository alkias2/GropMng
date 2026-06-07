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

    #region Public Methods

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

    /// <summary>
    /// Builds the watering dashboard tab by loading watering schedules, resolving due dates,
    /// applying skip rules, filtering by garden spot, grouping upcoming actions,
    /// and enriching actions with plant photos.
    /// </summary>
    /// <param name="query">Optional dashboard filtering parameters.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <remarks>
    /// Loads all watering schedules, logs, and skips, then calculates due dates based on the last recorded action.
    /// Results are split into (a) today/overdue items and (b) upcoming items bounded by <see cref="DashboardOptions.UpcomingHorizonDays"/>.
    /// Uses static cache via <see cref="DashboardCacheDefaults"/> to avoid duplicate loads between tabs (e.g., Watering & Counters).
    /// Photos are populated asynchronously after the main data is built.
    /// </remarks>
    public async Task<DashboardWateringTabModel> PrepareWateringTabAsync(
        DashboardQueryModel? query = null,
        CancellationToken ct = default)
    {
        // ================================================================================
        // WATERING TAB - MAIN EXECUTION FLOW
        // ================================================================================
        // This method builds the complete watering dashboard view model.
        // The flow is organized into logical phases (1-10) for maintainability.
        // Each phase is documented with its purpose and key decisions.
        // ================================================================================

        var model = new DashboardWateringTabModel();
        var dashboardQuery = NormalizeQuery(query);

        // =================================================================================
        // PHASE 1: LOAD SHARED CONTEXT
        // =================================================================================
        // LoadDashboardContextAsync is the single source of truth for:
        // - Current owner ID and authentication state
        // - Today's date (UTC, converted to DateOnly)
        // - Current season (Spring/Summer/Autumn/Winter based on month)
        // - All active plant instances with their plant/spot/location lookups
        // - Horizon days (from DashboardOptions, normalized to 1-60 range)
        //
        // If the owner has zero active plant instances, we return an empty model
        // immediately. This is a common early exit path that saves unnecessary
        // database/cache calls.
        // =================================================================================
        var context = await LoadDashboardContextAsync(ct);
        if (context is null)
            return model;

        // Pre-cache frequently used tokens to avoid redundant ToString() calls
        var ownerToken = context.OwnerId.ToString("N");
        var seasonToken = context.Season.ToString();
        var plantInstanceIds = context.PlantInstanceIds;

        // Build a quick lookup dictionary for plant instance details
        // Used heavily during schedule iteration (O(1) lookups instead of O(n) scans)
        var instanceMap = context.PlantInstances.ToDictionary(pi => pi.Id);

        // =================================================================================
        // PHASE 2: FETCH WATERING SCHEDULES (CACHED)
        // =================================================================================
        // Cache Key: WateringSchedulesCacheKey = $"Dashboard.Watering.Schedules.{ownerId}.{season}"
        // 
        // We fetch ALL schedules for the owner that match either:
        //   a) The current season (e.g., Spring schedules in spring)
        //   b) AllYear season (applies regardless of season)
        //
        // Why cache by season? Because seasonal schedules only change when the user
        // modifies them. The current season determines which schedules are active,
        // but the cache key includes the season to isolate different season views.
        //
        // After cache retrieval, we filter to only active plant instances IN MEMORY.
        // This is more efficient than adding a complex join in the SQL query because:
        //   - The active plant set is already cached from DashboardContext
        //   - The schedule list is relatively small (typically 5-50 records per owner)
        // =================================================================================
        var wateringSchedules = await _staticCacheManager.GetAsync(
            _staticCacheManager.PrepareKey(DashboardCacheDefaults.WateringSchedulesCacheKey, ownerToken, seasonToken),
            () => _wateringScheduleRepository.GetAllAsync(
                query => query.Where(s => s.OwnerId == context.OwnerId
                    && (s.Season == context.Season || s.Season == GardenSeason.AllYear)),
                cancellationToken: ct));

        wateringSchedules = wateringSchedules
            .Where(s => plantInstanceIds.Contains(s.PlantInstanceId))
            .ToList();

        // =================================================================================
        // PHASE 3: FETCH WATERING LOGS (CACHED)
        // =================================================================================
        // Cache Key: WateringLogsCacheKey = $"Dashboard.Watering.Logs.{ownerId}"
        //
        // Watering logs are NOT season-filtered at the database level because logs
        // are timestamped. A log from winter is still relevant for calculating
        // the next due date in spring (we need the last watering date regardless
        // of season).
        //
        // After retrieval, we filter to active plant instances only.
        // The logs are then grouped by PlantInstanceId to find the MOST RECENT
        // watering date for each plant. This grouping is done in-memory because:
        //   - EF Core's GroupBy + MaxBy translation to SQL is unreliable across providers
        //   - The log set per owner is typically small (< 1000 records)
        // =================================================================================
        var wateringLogs = await _staticCacheManager.GetAsync(
            _staticCacheManager.PrepareKey(DashboardCacheDefaults.WateringLogsCacheKey, ownerToken),
            () => _wateringLogRepository.GetAllAsync(
                query => query.Where(l => l.OwnerId == context.OwnerId),
                cancellationToken: ct));

        wateringLogs = wateringLogs
            .Where(l => plantInstanceIds.Contains(l.PlantInstanceId))
            .ToList();

        // =================================================================================
        // PHASE 4: FETCH ACTIVE SKIPS (CACHED)
        // =================================================================================
        // Cache Key: ActiveSkipsCacheKey = $"Dashboard.ActionSkips.{ownerId}.{today}"
        //
        // Skips are temporary pauses on actions (e.g., "Don't remind me to water this
        // plant for 2 weeks"). We only care about skips that are STILL ACTIVE today.
        //
        // The skip set is converted to a HashSet of tuples (PlantInstanceId, ActionType)
        // for O(1) lookup performance. This is critical because we will check each
        // schedule against this set during filtering.
        //
        // IMPORTANT: The today date is included in the cache key because skips expire.
        // A skip active on Monday may be expired by Wednesday, so the cache varies by date.
        // =================================================================================
        var activeSkips = await _staticCacheManager.GetAsync(
            _staticCacheManager.PrepareKey(DashboardCacheDefaults.ActiveSkipsCacheKey, ownerToken, context.Today.ToString("yyyyMMdd")),
            () => _actionSkipRepository.GetAllAsync(
                query => query.Where(s => s.OwnerId == context.OwnerId && s.ActiveUntilDate >= context.Today),
                cancellationToken: ct));

        var skipSet = activeSkips
            .Select(s => (s.PlantInstanceId, s.ActionType))
            .ToHashSet();

        // =================================================================================
        // PHASE 5: CALCULATE LAST WATERING DATE PER PLANT INSTANCE
        // =================================================================================
        // For each plant instance that has watering logs, we find the MAX (most recent)
        // WateredAtUtc. This dictionary will be used to calculate next due dates.
        //
        // Note: If a plant instance has NO logs, it will be absent from this dictionary.
        // That's intentional - the CalculateDueDate method treats missing entries as
        // "no history" and defaults due date to TODAY.
        // =================================================================================
        var latestWateringByInstance = wateringLogs
            .GroupBy(l => l.PlantInstanceId)
            .ToDictionary(g => g.Key, g => g.MaxBy(x => x.WateredAtUtc)!.WateredAtUtc);

        // =================================================================================
        // PHASE 6: BUILD ACTION ROWS FROM SCHEDULES
        // =================================================================================
        // For each watering schedule:
        //   1. Look up the plant instance (skip if missing - defensive programming)
        //   2. Calculate the next due date using:
        //      - Last watering date (or null if no history)
        //      - FrequencyDays from the schedule
        //   3. Build a DashboardActionModel with all display properties:
        //      - Plant name (from PlantMap lookup)
        //      - Nickname (user-provided friendly name)
        //      - Location and garden spot names
        //      - Due date, due status (Overdue/Today/Upcoming), and delta days
        //      - Action-specific data (water amount in liters)
        //
        // The BuildActionRow helper handles all the defensive null checks and fallback
        // values (e.g., "—" for missing plant names).
        // =================================================================================
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

        // =================================================================================
        // PHASE 7: LOAD LOCALIZED UI STRINGS
        // =================================================================================
        // These strings come from resource files (.resx) and support multi-language UI.
        // We fetch them once here and reuse throughout the method.
        // =================================================================================
        var allSpotsText = await _localizationService.GetResourceAsync("dashboard.owner.filter.allspots");
        var inDaysTemplate = await _localizationService.GetResourceAsync("dashboard.owner.group.inxdays");

        // =================================================================================
        // PHASE 8: FILTER OUT SKIPPED ACTIONS & SORT
        // =================================================================================
        // Remove any action where an active skip exists for (PlantInstanceId, Watering).
        // Then sort by:
        //   1. DueStatus (Overdue < Today < Upcoming) - critical for UX, puts urgent items first
        //   2. DueDate (earliest first) - within same status, oldest due dates first
        //   3. PlantName (alphabetical) - for consistent ordering when dates/statuses tie
        // =================================================================================
        var actionableWatering = wateringRows
            .Where(a => !skipSet.Contains((a.PlantInstanceId, ActionSkipType.Watering)))
            .OrderBy(a => a.DueStatus)
            .ThenBy(a => a.DueDate)
            .ThenBy(a => a.PlantName)
            .ToList();

        // =================================================================================
        // PHASE 9: BUILD SPOT FILTER DROPDOWN
        // =================================================================================
        // The spot filter allows users to filter the dashboard by a specific garden spot.
        // We build the dropdown from the DISTINCT GardenSpotIds in the current result set.
        // This ensures the filter only shows spots that actually have actionable items.
        //
        // The filter is built BEFORE applying the user's spot filter so that we can
        // preserve the full list of available spots in the model (for the dropdown).
        // Then we apply the filter to the actionable list.
        // =================================================================================
        model.AvailableGardenSpots = BuildGardenSpotFilter(actionableWatering, context.SpotMap, allSpotsText, dashboardQuery.SpotId);

        if (dashboardQuery.SpotId.HasValue)
            actionableWatering = actionableWatering
                .Where(a => a.GardenSpotId == dashboardQuery.SpotId.Value)
                .ToList();

        // =================================================================================
        // PHASE 10: SPLIT INTO TODAY/OVERDUE vs UPCOMING BUCKETS
        // =================================================================================
        // SplitActionRowsByTodayAndUpcoming creates two buckets:
        //   - TodayOverdueActions: DueStatus = Overdue OR Today
        //   - UpcomingActions: DueStatus = Upcoming AND DeltaDays <= HorizonDays
        //
        // Actions beyond HorizonDays (e.g., due in 3 weeks when horizon is 14 days)
        // are discarded. This prevents UI clutter and improves perceived performance.
        // =================================================================================
        var buckets = SplitActionRowsByTodayAndUpcoming(actionableWatering, context.HorizonDays);

        // Build the final model with groups for upcoming actions
        model = BuildWateringTabModel(buckets, context.Today, inDaysTemplate);

        // Re-attach the spot filter to the model (BuildWateringTabModel creates a new model)
        model.AvailableGardenSpots = BuildGardenSpotFilter(actionableWatering, context.SpotMap, allSpotsText, dashboardQuery.SpotId);

        // =================================================================================
        // PHASE 11: POPULATE PLANT PHOTOS (ASYNC)
        // =================================================================================
        // Plant photos are fetched asynchronously AFTER the main data is built.
        // This is an optimization: we don't block the main flow on photo URLs.
        // The UI can show a placeholder while photos load, or we await here before return.
        //
        // For each plant instance in the result set, we find the photo with the lowest
        // DisplayOrder (the "primary" photo) and generate a URL via IPictureService.
        // =================================================================================
        var actionsForPhotos = model.TodayOverdueActions
            .Concat(model.UpcomingActionGroups.SelectMany(g => g.Actions));
        await PopulateActionPhotosAsync(context.OwnerId, actionsForPhotos, ct);

        return model;
    }

    /// <summary>
    /// Builds the fertilizing dashboard tab by loading fertilizing schedules, resolving due dates,
    /// applying skip rules, filtering by garden spot, grouping upcoming actions,
    /// and enriching actions with plant photos.
    /// </summary>
    /// <remarks>
    /// Loads all fertilizing schedules, fertilizer metadata, logs, and skips.
    /// Due dates are calculated based on the last applied fertilizer record.
    /// Results are split into (a) today/overdue items and (b) upcoming items bounded by <see cref="DashboardOptions.UpcomingHorizonDays"/>.
    /// Fertilizer names are resolved via a separate cache lookup using <see cref="DashboardCacheDefaults.FertilizersLookupCacheKey"/>.
    /// </remarks>
    public async Task<DashboardFertilizingTabModel> PrepareFertilizingTabAsync(
        DashboardQueryModel? query = null,
        CancellationToken ct = default)
    {
        // ================================================================================
        // FERTILIZING TAB - MAIN EXECUTION FLOW
        // ================================================================================
        // This method parallels PrepareWateringTabAsync with fertilizer-specific logic.
        // Key differences are documented at each phase. The overall structure is identical
        // to maintain consistency and reduce cognitive load for developers.
        // ================================================================================

        var model = new DashboardFertilizingTabModel();
        var dashboardQuery = NormalizeQuery(query);

        // =================================================================================
        // PHASE 1: LOAD SHARED CONTEXT (IDENTICAL TO WATERING TAB)
        // =================================================================================
        // Same context loading as watering tab. The shared context includes all active
        // plant instances, today's date, current season, and lookup dictionaries.
        // =================================================================================
        var context = await LoadDashboardContextAsync(ct);
        if (context is null)
            return model;

        var ownerToken = context.OwnerId.ToString("N");
        var seasonToken = context.Season.ToString();
        var plantInstanceIds = context.PlantInstanceIds;
        var instanceMap = context.PlantInstances.ToDictionary(pi => pi.Id);

        // =================================================================================
        // PHASE 2: FETCH FERTILIZING SCHEDULES (CACHED)
        // =================================================================================
        // Cache Key: FertilizingSchedulesCacheKey = $"Dashboard.Fertilizing.Schedules.{ownerId}.{season}"
        //
        // Same season logic as watering: include schedules for current season OR AllYear.
        // Filter to active plant instances after cache retrieval.
        // =================================================================================
        var fertilizingSchedules = await _staticCacheManager.GetAsync(
            _staticCacheManager.PrepareKey(DashboardCacheDefaults.FertilizingSchedulesCacheKey, ownerToken, seasonToken),
            () => _fertilizingScheduleRepository.GetAllAsync(
                query => query.Where(s => s.OwnerId == context.OwnerId
                    && (s.Season == context.Season || s.Season == GardenSeason.AllYear)),
                cancellationToken: ct));

        fertilizingSchedules = fertilizingSchedules
            .Where(s => plantInstanceIds.Contains(s.PlantInstanceId))
            .ToList();

        // =================================================================================
        // PHASE 3: FETCH FERTILIZER NAMES (ADDITIONAL LOOKUP - UNIQUE TO FERTILIZING)
        // =================================================================================
        // Fertilizing schedules reference FertilizerId (foreign key to Fertilizer table).
        // We need to fetch the fertilizer names for display in the UI.
        //
        // Cache Key: FertilizersLookupCacheKey = $"Dashboard.Fertilizers.Lookup.{token}"
        // where token = BuildIntSetToken(fertilizerIds) e.g., "2-5-9"
        //
        // Why a custom token? Different schedule sets may have different fertilizer IDs.
        // Using a deterministic sort+join creates cache reuse when the same set of IDs
        // appears in different orders or from different schedule batches.
        //
        // Example: Schedules with FertilizerIds [5,2,9] and [9,5,2] both produce key "2-5-9"
        // =================================================================================
        var fertilizerIds = fertilizingSchedules.Select(s => s.FertilizerId).Distinct().ToList();
        var fertilizersLookupToken = BuildIntSetToken(fertilizerIds);
        var fertilizers = await _staticCacheManager.GetAsync(
            _staticCacheManager.PrepareKey(DashboardCacheDefaults.FertilizersLookupCacheKey, fertilizersLookupToken),
            () => _fertilizerRepository.GetByIdsAsync(fertilizerIds, cancellationToken: ct));
        var fertilizerMap = fertilizers.ToDictionary(f => f.Id, f => f.Name);

        // =================================================================================
        // PHASE 4: FETCH FERTILIZING LOGS (CACHED)
        // =================================================================================
        // Cache Key: FertilizingLogsCacheKey = $"Dashboard.Fertilizing.Logs.{ownerId}"
        //
        // Similar to watering logs: fetch all logs for the owner, filter to active plants,
        // then group by PlantInstanceId to find the MOST RECENT application date.
        // =================================================================================
        var fertilizingLogs = await _staticCacheManager.GetAsync(
            _staticCacheManager.PrepareKey(DashboardCacheDefaults.FertilizingLogsCacheKey, ownerToken),
            () => _fertilizingLogRepository.GetAllAsync(
                query => query.Where(l => l.OwnerId == context.OwnerId),
                cancellationToken: ct));

        fertilizingLogs = fertilizingLogs
            .Where(l => plantInstanceIds.Contains(l.PlantInstanceId))
            .ToList();

        // =================================================================================
        // PHASE 5: FETCH ACTIVE SKIPS (CACHED)
        // =================================================================================
        // Same cache key as watering tab, but we filter by ActionSkipType.Fertilizing later.
        // The cache includes ALL skip types for the owner, which is fine because:
        //   - The set is small (typically < 50 active skips per owner)
        //   - Sharing the cache reduces duplicate queries across tabs
        // =================================================================================
        var activeSkips = await _staticCacheManager.GetAsync(
            _staticCacheManager.PrepareKey(DashboardCacheDefaults.ActiveSkipsCacheKey, ownerToken, context.Today.ToString("yyyyMMdd")),
            () => _actionSkipRepository.GetAllAsync(
                query => query.Where(s => s.OwnerId == context.OwnerId && s.ActiveUntilDate >= context.Today),
                cancellationToken: ct));

        var skipSet = activeSkips
            .Select(s => (s.PlantInstanceId, s.ActionType))
            .ToHashSet();

        // =================================================================================
        // PHASE 6: CALCULATE LAST APPLICATION DATE PER PLANT INSTANCE
        // =================================================================================
        // Identical to watering: find the most recent AppliedAtUtc for each plant instance.
        // Missing entries mean "no fertilizer history" → due date defaults to today.
        // =================================================================================
        var latestFertilizingByInstance = fertilizingLogs
            .GroupBy(l => l.PlantInstanceId)
            .ToDictionary(g => g.Key, g => g.MaxBy(x => x.AppliedAtUtc)!.AppliedAtUtc);

        // =================================================================================
        // PHASE 7: BUILD ACTION ROWS FROM SCHEDULES (FERTILIZER-SPECIFIC)
        // =================================================================================
        // For each fertilizing schedule:
        //   1. Look up the plant instance
        //   2. Calculate next due date from last application date + FrequencyDays
        //   3. Build DashboardActionModel with fertilizer-specific fields:
        //      - FertilizerName (from the pre-fetched fertilizerMap)
        //      - FertilizerQuantity (decimal, e.g., 2.5)
        //      - FertilizerQuantityUnit (enum: grams, milliliters, pumps, etc.)
        //      - WaterAmountL is NULL (not applicable)
        // =================================================================================
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

        // =================================================================================
        // PHASE 8: LOAD LOCALIZED UI STRINGS
        // =================================================================================
        // Same localization strings as watering tab. The "in X days" template is reused.
        // =================================================================================
        var allSpotsText = await _localizationService.GetResourceAsync("dashboard.owner.filter.allspots");
        var inDaysTemplate = await _localizationService.GetResourceAsync("dashboard.owner.group.inxdays");

        // =================================================================================
        // PHASE 9: FILTER OUT SKIPPED ACTIONS & SORT
        // =================================================================================
        // KEY DIFFERENCE: We filter by ActionSkipType.Fertilizing instead of Watering.
        // A user could skip watering but NOT fertilizing, or vice versa.
        // =================================================================================
        var actionableFertilizing = fertilizingRows
            .Where(a => !skipSet.Contains((a.PlantInstanceId, ActionSkipType.Fertilizing)))
            .OrderBy(a => a.DueStatus)
            .ThenBy(a => a.DueDate)
            .ThenBy(a => a.PlantName)
            .ToList();

        // =================================================================================
        // PHASE 10: BUILD SPOT FILTER DROPDOWN & APPLY FILTER
        // =================================================================================
        // Same logic as watering tab: build filter from distinct spots in results,
        // then apply user's spot filter if present.
        // =================================================================================
        model.AvailableGardenSpots = BuildGardenSpotFilter(actionableFertilizing, context.SpotMap, allSpotsText, dashboardQuery.SpotId);

        if (dashboardQuery.SpotId.HasValue)
            actionableFertilizing = actionableFertilizing
                .Where(a => a.GardenSpotId == dashboardQuery.SpotId.Value)
                .ToList();

        // =================================================================================
        // PHASE 11: SPLIT INTO BUCKETS & BUILD MODEL
        // =================================================================================
        // Same bucket logic as watering. The BuildFertilizingTabModel helper creates
        // a DashboardFertilizingTabModel with the same structure as watering model
        // (TodayOverdueActions, UpcomingActionGroups, etc.)
        // =================================================================================
        var buckets = SplitActionRowsByTodayAndUpcoming(actionableFertilizing, context.HorizonDays);
        model = BuildFertilizingTabModel(buckets, context.Today, inDaysTemplate);
        model.AvailableGardenSpots = BuildGardenSpotFilter(actionableFertilizing, context.SpotMap, allSpotsText, dashboardQuery.SpotId);

        // =================================================================================
        // PHASE 12: POPULATE PLANT PHOTOS (ASYNC)
        // =================================================================================
        // Photos are shared across tabs (same PlantInstanceId, same primary photo).
        // The photo service caches URLs internally, so repeated calls are cheap.
        // =================================================================================
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

    #region Private Methods

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

    /// <summary>
    /// Creates a unique string token from a list of integers, sorted and deduplicated.
    /// Used for cache keys to look up multiple entities by IDs.
    /// </summary>
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