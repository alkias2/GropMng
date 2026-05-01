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
    private readonly IRepository<ActionSkip> _actionSkipRepository;

    #endregion

    #region Ctor

    public ActionLogController(
        ICurrentOwnerProvider currentOwnerProvider,
        IRepository<PlantInstance> plantInstanceRepository,
        IRepository<WateringLog> wateringLogRepository,
        IRepository<FertilizingLog> fertilizingLogRepository,
        IRepository<FertilizingSchedule> fertilizingScheduleRepository,
        IRepository<ActionSkip> actionSkipRepository)
    {
        _currentOwnerProvider = currentOwnerProvider;
        _plantInstanceRepository = plantInstanceRepository;
        _wateringLogRepository = wateringLogRepository;
        _fertilizingLogRepository = fertilizingLogRepository;
        _fertilizingScheduleRepository = fertilizingScheduleRepository;
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
            WateredAtUtc = DateTime.UtcNow
        }, cancellationToken: cancellationToken);

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
            AppliedAtUtc = DateTime.UtcNow
        }, cancellationToken: cancellationToken);

        return Json(new { success = true });
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
}
