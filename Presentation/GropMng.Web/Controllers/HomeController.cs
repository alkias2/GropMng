using GropMng.Core.Domain.Garden.Care;
using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Health;
using GropMng.Core.Domain.Garden.Locations;
using GropMng.Core.Domain.Garden.Plants;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.User;
using GropMng.Web.Models.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GropMng.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICurrentOwnerProvider _currentOwnerProvider;
        private readonly IRepository<PlantInstance> _plantInstanceRepository;
        private readonly IRepository<Plant> _plantRepository;
        private readonly IRepository<GardenSpot> _gardenSpotRepository;
        private readonly IRepository<Location> _locationRepository;
        private readonly IRepository<WateringSchedule> _wateringScheduleRepository;
        private readonly IRepository<FertilizingSchedule> _fertilizingScheduleRepository;
        private readonly IRepository<WateringLog> _wateringLogRepository;
        private readonly IRepository<FertilizingLog> _fertilizingLogRepository;
        private readonly IRepository<RepottingLog> _repottingLogRepository;
        private readonly IRepository<PlantDiseaseRecord> _plantDiseaseRecordRepository;
        private readonly IRepository<Fertilizer> _fertilizerRepository;
        private readonly IRepository<ActionSkip> _actionSkipRepository;

        public HomeController(
            ICurrentOwnerProvider currentOwnerProvider,
            IRepository<PlantInstance> plantInstanceRepository,
            IRepository<Plant> plantRepository,
            IRepository<GardenSpot> gardenSpotRepository,
            IRepository<Location> locationRepository,
            IRepository<WateringSchedule> wateringScheduleRepository,
            IRepository<FertilizingSchedule> fertilizingScheduleRepository,
            IRepository<WateringLog> wateringLogRepository,
            IRepository<FertilizingLog> fertilizingLogRepository,
            IRepository<RepottingLog> repottingLogRepository,
            IRepository<PlantDiseaseRecord> plantDiseaseRecordRepository,
            IRepository<Fertilizer> fertilizerRepository,
            IRepository<ActionSkip> actionSkipRepository)
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
            _repottingLogRepository = repottingLogRepository;
            _plantDiseaseRecordRepository = plantDiseaseRecordRepository;
            _fertilizerRepository = fertilizerRepository;
            _actionSkipRepository = actionSkipRepository;
        }

        #region Methods

        /// <summary>
        /// Public landing page. Authenticated owners are redirected to their dashboard.
        /// </summary>
        [AllowAnonymous]
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction(nameof(Dashboard));

            return View();
        }

        /// <summary>
        /// Placeholder owner dashboard — will be replaced in feature/owner-dashboard-core.
        /// </summary>
        [Authorize]
        public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
        {
            var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);
            var model = new OwnerDashboardModel();

            var nowUtc = DateTime.UtcNow;
            var today = DateOnly.FromDateTime(nowUtc);
            var season = ResolveCurrentSeason(today);

            var plantInstances = await _plantInstanceRepository.GetAllAsync(
                query => query
                    .Where(pi => pi.OwnerId == ownerId && pi.IsActive)
                    .OrderBy(pi => pi.Id),
                cancellationToken: cancellationToken);

            model.ActivePlantsCount = plantInstances.Count;

            if (plantInstances.Count == 0)
                return View(model);

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

            var activeDiseaseCases = await _plantDiseaseRecordRepository.CountAsync(
                record => record.OwnerId == ownerId
                    && (record.Outcome == null || record.Outcome == PlantDiseaseOutcome.Ongoing),
                cancellationToken: cancellationToken);
            model.ActiveDiseaseCasesCount = activeDiseaseCases;

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

            var wateringLogs = await _wateringLogRepository.GetAllAsync(
                query => query.Where(l => l.OwnerId == ownerId && plantInstanceIds.Contains(l.PlantInstanceId)),
                cancellationToken: cancellationToken);

            var fertilizingLogs = await _fertilizingLogRepository.GetAllAsync(
                query => query.Where(l => l.OwnerId == ownerId && plantInstanceIds.Contains(l.PlantInstanceId)),
                cancellationToken: cancellationToken);

            var repottingLogs = await _repottingLogRepository.GetAllAsync(
                query => query.Where(l => l.OwnerId == ownerId && plantInstanceIds.Contains(l.PlantInstanceId)),
                cancellationToken: cancellationToken);

            var latestWateringByInstance = wateringLogs
                .GroupBy(l => l.PlantInstanceId)
                .ToDictionary(g => g.Key, g => g.MaxBy(x => x.WateredAtUtc)!.WateredAtUtc);

            var latestFertilizingByInstance = fertilizingLogs
                .GroupBy(l => l.PlantInstanceId)
                .ToDictionary(g => g.Key, g => g.MaxBy(x => x.AppliedAtUtc)!.AppliedAtUtc);

            var actionRows = new List<DashboardActionModel>();

            foreach (var schedule in wateringSchedules)
            {
                var instance = plantInstances.First(pi => pi.Id == schedule.PlantInstanceId);
                var dueDate = CalculateDueDate(latestWateringByInstance, schedule.PlantInstanceId, schedule.FrequencyDays, today);
                actionRows.Add(BuildActionRow(
                    instance,
                    DashboardActionType.Watering,
                    schedule.FrequencyDays,
                    schedule.Season,
                    dueDate,
                    today,
                    plantMap,
                    spotMap,
                    locationMap));
            }

            foreach (var schedule in fertilizingSchedules)
            {
                var instance = plantInstances.First(pi => pi.Id == schedule.PlantInstanceId);
                var dueDate = CalculateDueDate(latestFertilizingByInstance, schedule.PlantInstanceId, schedule.FrequencyDays, today);
                actionRows.Add(BuildActionRow(
                    instance,
                    DashboardActionType.Fertilizing,
                    schedule.FrequencyDays,
                    schedule.Season,
                    dueDate,
                    today,
                    plantMap,
                    spotMap,
                    locationMap));
            }

            model.OverdueActionsCount = actionRows.Count(a => a.DueStatus == DashboardDueStatus.Overdue);
            // ActionsTodayCount = only due today (not overdue) — matches the "Today" badge in the list
            model.ActionsTodayCount = actionRows.Count(a => a.DueStatus == DashboardDueStatus.Today);

            // Load active skips for this owner and filter them out of the displayed list
            var activeSkips = await _actionSkipRepository.GetAllAsync(
                query => query.Where(s => s.OwnerId == ownerId && s.ActiveUntilDate >= today),
                cancellationToken: cancellationToken);

            var skipSet = activeSkips
                .Select(s => (s.PlantInstanceId, s.ActionType))
                .ToHashSet();

            // Show all actions that need attention (overdue + today), no cap — totals match KPIs exactly
            model.TodayActions = actionRows
                .Where(a => a.DueStatus == DashboardDueStatus.Overdue || a.DueStatus == DashboardDueStatus.Today)
                .Where(a => !skipSet.Contains((a.PlantInstanceId, a.ActionType == DashboardActionType.Watering
                    ? ActionSkipType.Watering
                    : ActionSkipType.Fertilizing)))
                .OrderBy(a => a.DueStatus)
                .ThenBy(a => a.DueDate)
                .ThenBy(a => a.PlantName)
                .ToList();

            var fertilizerIds = fertilizingLogs.Select(f => f.FertilizerId).Distinct().ToList();
            var fertilizers = fertilizerIds.Count > 0
                ? await _fertilizerRepository.GetByIdsAsync(fertilizerIds, cancellationToken: cancellationToken)
                : Array.Empty<Fertilizer>();
            var fertilizerMap = fertilizers.ToDictionary(f => f.Id);

            var activityRows = new List<DashboardActivityModel>();

            activityRows.AddRange(wateringLogs.Select(log => BuildWateringActivity(log, plantInstances, plantMap)));
            activityRows.AddRange(fertilizingLogs.Select(log => BuildFertilizingActivity(log, plantInstances, plantMap, fertilizerMap)));
            activityRows.AddRange(repottingLogs.Select(log => BuildRepottingActivity(log, plantInstances, plantMap)));

            model.RecentActivity = activityRows
                .OrderByDescending(a => a.OccurredAtUtc)
                .Take(12)
                .ToList();

            return View(model);
        }

        private static GardenSeason ResolveCurrentSeason(DateOnly today)
            => today.Month switch
            {
                >= 3 and <= 5 => GardenSeason.Spring,
                >= 6 and <= 8 => GardenSeason.Summer,
                >= 9 and <= 11 => GardenSeason.Autumn,
                _ => GardenSeason.Winter
            };

        private static DateOnly CalculateDueDate(IReadOnlyDictionary<int, DateTime> latestByInstance, int plantInstanceId, byte frequencyDays, DateOnly today)
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

        private static DashboardActivityModel BuildWateringActivity(
            WateringLog log,
            IReadOnlyList<PlantInstance> plantInstances,
            IReadOnlyDictionary<int, Plant> plantMap)
        {
            var instance = plantInstances.First(pi => pi.Id == log.PlantInstanceId);
            var amountText = log.WaterAmountL.HasValue ? $"{log.WaterAmountL:0.##} L" : "—";

            return new DashboardActivityModel
            {
                PlantInstanceId = instance.Id,
                PlantName = plantMap.TryGetValue(instance.PlantId, out var plant) ? plant.ScientificName : "—",
                Nickname = instance.Nickname,
                ActivityType = DashboardActivityType.Watering,
                OccurredAtUtc = log.WateredAtUtc,
                Summary = $"Watered: {amountText}"
            };
        }

        private static DashboardActivityModel BuildFertilizingActivity(
            FertilizingLog log,
            IReadOnlyList<PlantInstance> plantInstances,
            IReadOnlyDictionary<int, Plant> plantMap,
            IReadOnlyDictionary<int, Fertilizer> fertilizerMap)
        {
            var instance = plantInstances.First(pi => pi.Id == log.PlantInstanceId);
            var fertilizerName = fertilizerMap.TryGetValue(log.FertilizerId, out var fertilizer) ? fertilizer.Name : "Fertilizer";
            var quantityText = log.Quantity.HasValue ? $"{log.Quantity:0.###} {log.Unit}" : "—";

            return new DashboardActivityModel
            {
                PlantInstanceId = instance.Id,
                PlantName = plantMap.TryGetValue(instance.PlantId, out var plant) ? plant.ScientificName : "—",
                Nickname = instance.Nickname,
                ActivityType = DashboardActivityType.Fertilizing,
                OccurredAtUtc = log.AppliedAtUtc,
                Summary = $"{fertilizerName}: {quantityText}"
            };
        }

        private static DashboardActivityModel BuildRepottingActivity(
            RepottingLog log,
            IReadOnlyList<PlantInstance> plantInstances,
            IReadOnlyDictionary<int, Plant> plantMap)
        {
            var instance = plantInstances.First(pi => pi.Id == log.PlantInstanceId);

            return new DashboardActivityModel
            {
                PlantInstanceId = instance.Id,
                PlantName = plantMap.TryGetValue(instance.PlantId, out var plant) ? plant.ScientificName : "—",
                Nickname = instance.Nickname,
                ActivityType = DashboardActivityType.Repotting,
                OccurredAtUtc = log.RepottedAtUtc,
                Summary = log.Notes ?? "Repotting recorded"
            };
        }

        #endregion
    }
}
