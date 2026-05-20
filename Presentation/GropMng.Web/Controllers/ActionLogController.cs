using GropMng.Core.Domain.Garden.Care;
using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Plants;
using GropMng.Core.Interfaces.Repositories;
using GropMng.Core.Interfaces.Services.User;
using GropMng.Web.Models.ActionLog;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GropMng.Web.Controllers;

/// <summary>
/// Handles quick "done today" action logging (watering, fertilizing) from the dashboard.
/// </summary>
[Authorize]
[Route("action-log")]
public class ActionLogController : Controller
{
    #region Fields

    private readonly ICurrentOwnerProvider _currentOwnerProvider;
    private readonly IRepository<PlantInstance> _plantInstanceRepository;
    private readonly IRepository<WateringLog> _wateringLogRepository;
    private readonly IRepository<FertilizingLog> _fertilizingLogRepository;
    private readonly IRepository<FertilizingSchedule> _fertilizingScheduleRepository;
    private readonly IRepository<WateringSchedule> _wateringScheduleRepository;
    private readonly IRepository<ActionSkip> _actionSkipRepository;

    #endregion

    #region Ctor

    public ActionLogController(
        ICurrentOwnerProvider currentOwnerProvider,
        IRepository<PlantInstance> plantInstanceRepository,
        IRepository<WateringLog> wateringLogRepository,
        IRepository<FertilizingLog> fertilizingLogRepository,
        IRepository<FertilizingSchedule> fertilizingScheduleRepository,
        IRepository<WateringSchedule> wateringScheduleRepository,
        IRepository<ActionSkip> actionSkipRepository)
    {
        _currentOwnerProvider = currentOwnerProvider;
        _plantInstanceRepository = plantInstanceRepository;
        _wateringLogRepository = wateringLogRepository;
        _fertilizingLogRepository = fertilizingLogRepository;
        _fertilizingScheduleRepository = fertilizingScheduleRepository;
        _wateringScheduleRepository = wateringScheduleRepository;
        _actionSkipRepository = actionSkipRepository;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Records a quick watering event for a plant instance (today, no amount).
    /// </summary>
    [HttpPost("watering")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogWatering([FromForm] LogWateringRequest request, CancellationToken cancellationToken)
    {
        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);

        // Verify the plant instance belongs to this owner
        var instance = await _plantInstanceRepository.FirstOrDefaultAsync(
            pi => pi.Id == request.PlantInstanceId && pi.OwnerId == ownerId && pi.IsActive,
            cancellationToken: cancellationToken);

        if (instance is null)
            return Json(new { success = false, message = "Plant instance not found." });

        await _wateringLogRepository.CreateAsync(new WateringLog
        {
            OwnerId = ownerId,
            PlantInstanceId = instance.Id,
            WateredAtUtc = DateTime.UtcNow,
            WaterAmountL = request.WaterAmountL
        }, cancellationToken: cancellationToken);

        await CreateSuppressionForDashboardDoneAsync(
            ownerId,
            instance.Id,
            ActionSkipType.Watering,
            request.DueStatus,
            request.DueDate,
            cancellationToken);

        return Json(new { success = true });
    }

    /// <summary>
    /// Records a quick fertilizing event for a plant instance (today, using the first active schedule's fertilizer).
    /// </summary>
    [HttpPost("fertilizing")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogFertilizing([FromForm] LogFertilizingRequest request, CancellationToken cancellationToken)
    {
        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);

        // Verify the plant instance belongs to this owner
        var instance = await _plantInstanceRepository.FirstOrDefaultAsync(
            pi => pi.Id == request.PlantInstanceId && pi.OwnerId == ownerId && pi.IsActive,
            cancellationToken: cancellationToken);

        if (instance is null)
            return Json(new { success = false, message = "Plant instance not found." });

        // Resolve fertilizer from the active schedule for this instance
        var schedule = await _fertilizingScheduleRepository.FirstOrDefaultAsync(
            s => s.OwnerId == ownerId && s.PlantInstanceId == instance.Id,
            cancellationToken: cancellationToken);

        if (schedule is null)
            return Json(new { success = false, message = "No fertilizing schedule found for this plant." });

        await _fertilizingLogRepository.CreateAsync(new FertilizingLog
        {
            OwnerId = ownerId,
            PlantInstanceId = instance.Id,
            FertilizerId = schedule.FertilizerId,
            AppliedAtUtc = DateTime.UtcNow,
            Quantity = request.Quantity ?? schedule.Quantity,
            Unit = request.Unit ?? schedule.Unit
        }, cancellationToken: cancellationToken);

        await CreateSuppressionForDashboardDoneAsync(
            ownerId,
            instance.Id,
            ActionSkipType.Fertilizing,
            request.DueStatus,
            request.DueDate,
            cancellationToken);

        return Json(new { success = true });
    }

    /// <summary>
    /// Records watering for all specified plant instances in a single request.
    /// WaterAmountL is resolved from the active schedule for each instance.
    /// </summary>
    [HttpPost("watering-all")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> WateringAll([FromForm] LogWateringAllRequest request, CancellationToken cancellationToken)
    {
        if (request.PlantInstanceIds == null || request.PlantInstanceIds.Count == 0)
            return Json(new { success = false, message = "No plant instances provided." });

        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var season = ResolveCurrentSeason(today);

        var instances = await _plantInstanceRepository.GetAllAsync(
            query => query.Where(pi =>
                request.PlantInstanceIds.Contains(pi.Id)
                && pi.OwnerId == ownerId
                && pi.IsActive),
            cancellationToken: cancellationToken);

        if (instances.Count == 0)
            return Json(new { success = false, message = "No valid plant instances found." });

        var instanceIds = instances.Select(pi => pi.Id).ToHashSet();

        var schedules = await _wateringScheduleRepository.GetAllAsync(
            query => query.Where(s =>
                s.OwnerId == ownerId
                && instanceIds.Contains(s.PlantInstanceId)
                && (s.Season == season || s.Season == GardenSeason.AllYear)),
            cancellationToken: cancellationToken);

        var scheduleMap = schedules
            .GroupBy(s => s.PlantInstanceId)
            .ToDictionary(g => g.Key, g => g.First());

        var count = 0;
        foreach (var instance in instances)
        {
            scheduleMap.TryGetValue(instance.Id, out var schedule);

            await _wateringLogRepository.CreateAsync(new WateringLog
            {
                OwnerId = ownerId,
                PlantInstanceId = instance.Id,
                WateredAtUtc = DateTime.UtcNow,
                WaterAmountL = schedule?.WaterAmountL
            }, cancellationToken: cancellationToken);

            await CreateSuppressionForDashboardDoneAsync(
                ownerId,
                instance.Id,
                ActionSkipType.Watering,
                dueStatus: "today",
                dueDate: null,
                cancellationToken);

            count++;
        }

        return Json(new { success = true, count });
    }

    /// <summary>
    /// Records fertilizing for all specified plant instances in a single request.
    /// Quantity/Unit/FertilizerId are resolved from the active schedule for each instance.
    /// </summary>
    [HttpPost("fertilizing-all")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FertilizingAll([FromForm] LogFertilizingAllRequest request, CancellationToken cancellationToken)
    {
        if (request.PlantInstanceIds == null || request.PlantInstanceIds.Count == 0)
            return Json(new { success = false, message = "No plant instances provided." });

        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var season = ResolveCurrentSeason(today);

        var instances = await _plantInstanceRepository.GetAllAsync(
            query => query.Where(pi =>
                request.PlantInstanceIds.Contains(pi.Id)
                && pi.OwnerId == ownerId
                && pi.IsActive),
            cancellationToken: cancellationToken);

        if (instances.Count == 0)
            return Json(new { success = false, message = "No valid plant instances found." });

        var instanceIds = instances.Select(pi => pi.Id).ToHashSet();

        var schedules = await _fertilizingScheduleRepository.GetAllAsync(
            query => query.Where(s =>
                s.OwnerId == ownerId
                && instanceIds.Contains(s.PlantInstanceId)
                && (s.Season == season || s.Season == GardenSeason.AllYear)),
            cancellationToken: cancellationToken);

        var scheduleMap = schedules
            .GroupBy(s => s.PlantInstanceId)
            .ToDictionary(g => g.Key, g => g.First());

        var count = 0;
        foreach (var instance in instances)
        {
            if (!scheduleMap.TryGetValue(instance.Id, out var schedule))
                continue;

            await _fertilizingLogRepository.CreateAsync(new FertilizingLog
            {
                OwnerId = ownerId,
                PlantInstanceId = instance.Id,
                FertilizerId = schedule.FertilizerId,
                AppliedAtUtc = DateTime.UtcNow,
                Quantity = schedule.Quantity,
                Unit = schedule.Unit
            }, cancellationToken: cancellationToken);

            await CreateSuppressionForDashboardDoneAsync(
                ownerId,
                instance.Id,
                ActionSkipType.Fertilizing,
                dueStatus: "today",
                dueDate: null,
                cancellationToken);

            count++;
        }

        return Json(new { success = true, count });
    }

    /// <summary>
    /// Records an owner's decision to skip a scheduled care action until a computed date.
    /// SkipMode "today"  → active until today (reappears tomorrow).
    /// SkipMode "next"   → active until today + FrequencyDays - 1 (reappears on next due date).
    /// </summary>
    [HttpPost("skip")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SkipAction([FromForm] SkipActionRequest request, CancellationToken cancellationToken)
    {
        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);

        var instance = await _plantInstanceRepository.FirstOrDefaultAsync(
            pi => pi.Id == request.PlantInstanceId && pi.OwnerId == ownerId && pi.IsActive,
            cancellationToken: cancellationToken);

        if (instance is null)
            return Json(new { success = false, message = "Plant instance not found." });

        if (!Enum.TryParse<ActionSkipType>(request.ActionType, ignoreCase: true, out var actionType))
            return Json(new { success = false, message = "Invalid action type." });

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var activeUntil = request.SkipMode == "next" && request.FrequencyDays > 0
            ? today.AddDays(request.FrequencyDays - 1)
            : today;

        await _actionSkipRepository.CreateAsync(new ActionSkip
        {
            OwnerId = ownerId,
            PlantInstanceId = instance.Id,
            ActionType = actionType,
            SkippedAtUtc = DateTime.UtcNow,
            ActiveUntilDate = activeUntil
        }, cancellationToken: cancellationToken);

        return Json(new { success = true });
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

    /// <summary>
    /// Applies suppression after a dashboard "done" action:
    /// - upcoming early completion: suppress until original due date
    /// - today/overdue completion: suppress for the rest of today
    /// </summary>
    private async Task CreateSuppressionForDashboardDoneAsync(
        Guid ownerId,
        int plantInstanceId,
        ActionSkipType actionType,
        string? dueStatus,
        DateOnly? dueDate,
        CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var activeUntil = today;

        if (string.Equals(dueStatus, "upcoming", StringComparison.OrdinalIgnoreCase)
            && dueDate.HasValue
            && dueDate.Value > today)
        {
            activeUntil = dueDate.Value;
        }

        await _actionSkipRepository.CreateAsync(new ActionSkip
        {
            OwnerId = ownerId,
            PlantInstanceId = plantInstanceId,
            ActionType = actionType,
            SkippedAtUtc = DateTime.UtcNow,
            ActiveUntilDate = activeUntil
        }, cancellationToken: cancellationToken);
    }

    #endregion
}
