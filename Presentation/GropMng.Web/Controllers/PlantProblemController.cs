using GropMng.Core.Common.Exceptions;
using GropMng.Core.Domain.Garden.Health;
using GropMng.Core.Interfaces.Services.Garden.Health;
using GropMng.Core.Interfaces.Services.Garden.Plants;
using GropMng.Core.Interfaces.Services.User;
using GropMng.Web.Factories.Garden;
using GropMng.Web.Models.Garden;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GropMng.Web.Controllers;

/// <summary>
/// Handles user-facing CRUD for plant problem records and treatment schedules via modals.
/// </summary>
[Authorize]
[Route("my-garden/problems")]
public class PlantProblemController : Controller
{
    #region Fields

    private readonly IPlantProblemService _plantProblemService;
    private readonly IPlantInstanceService _plantInstanceService;
    private readonly ICurrentOwnerProvider _currentOwnerProvider;
    private readonly PlantProblemFactory _factory;

    #endregion

    #region Ctor

    public PlantProblemController(
        IPlantProblemService plantProblemService,
        IPlantInstanceService plantInstanceService,
        ICurrentOwnerProvider currentOwnerProvider,
        PlantProblemFactory factory)
    {
        _plantProblemService = plantProblemService ?? throw new ArgumentNullException(nameof(plantProblemService));
        _plantInstanceService = plantInstanceService ?? throw new ArgumentNullException(nameof(plantInstanceService));
        _currentOwnerProvider = currentOwnerProvider ?? throw new ArgumentNullException(nameof(currentOwnerProvider));
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    #endregion

    #region Problems Tab

    [HttpGet("tab/{plantInstanceId:int}")]
    public async Task<IActionResult> ProblemsTab(int plantInstanceId, CancellationToken cancellationToken)
    {
        var model = await _factory.PrepareProblemsTabViewModelAsync(plantInstanceId, cancellationToken);
        return PartialView("_ProblemsTab", model);
    }

    #endregion

    #region Problem Record CRUD

    [HttpGet("create-modal/{plantInstanceId:int}")]
    public async Task<IActionResult> GetCreateModal(int plantInstanceId, CancellationToken cancellationToken)
    {
        var model = await _factory.PrepareCreateModelAsync(plantInstanceId, cancellationToken);
        return PartialView("_CreateProblemModal", model);
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProblemRecordEditModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return Json(new { success = false, message = errors });
        }

        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);

        try
        {
            var record = new PlantProblemRecord
            {
                OwnerId = ownerId,
                PlantInstanceId = model.PlantInstanceId,
                ProblemName = model.ProblemName.Trim(),
                DiseaseKnowledgeId = model.DiseaseKnowledgeId,
                DetectedDate = model.DetectedDate,
                Severity = Enum.Parse<Core.Domain.Garden.Enums.Severity>(model.Severity),
                ProblemStatus = Enum.Parse<Core.Domain.Garden.Enums.ProblemStatus>(model.ProblemStatus),
                InfoSource = Enum.Parse<Core.Domain.Garden.Enums.InfoSource>(model.InfoSource),
                Notes = model.Notes?.Trim(),
                NotifyAdmin = model.NotifyAdmin
            };

            var created = await _plantProblemService.CreateAsync(record, cancellationToken);

            var message = model.NotifyAdmin && !model.DiseaseKnowledgeId.HasValue
                ? "Το πρόβλημα καταγράφηκε. Ο διαχειριστής ειδοποιήθηκε."
                : "Το πρόβλημα καταγράφηκε επιτυχώς.";

            return Json(new { success = true, message });
        }
        catch (DomainException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("edit-modal/{id:int}")]
    public async Task<IActionResult> GetEditModal(int id, CancellationToken cancellationToken)
    {
        try
        {
            var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);

            // Pre-fill PlantInstanceName
            var record = await _plantProblemService.GetByIdAsync(id, ownerId, cancellationToken);
            var instance = await _plantInstanceService.GetPlantInstanceByIdAsync(record.PlantInstanceId, ownerId, includeDetails: false, cancellationToken);

            var model = await _factory.PrepareEditModelAsync(id, cancellationToken);
            model.PlantInstanceName = instance?.Nickname ?? $"Plant #{record.PlantInstanceId}";

            return PartialView("_EditProblemModal", model);
        }
        catch (DomainException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProblemRecordEditModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return Json(new { success = false, message = errors });
        }

        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);

        try
        {
            var record = new PlantProblemRecord
            {
                Id = id,
                OwnerId = ownerId,
                PlantInstanceId = model.PlantInstanceId,
                ProblemName = model.ProblemName.Trim(),
                DiseaseKnowledgeId = model.DiseaseKnowledgeId,
                DetectedDate = model.DetectedDate,
                Severity = Enum.Parse<Core.Domain.Garden.Enums.Severity>(model.Severity),
                ProblemStatus = Enum.Parse<Core.Domain.Garden.Enums.ProblemStatus>(model.ProblemStatus),
                InfoSource = Enum.Parse<Core.Domain.Garden.Enums.InfoSource>(model.InfoSource),
                Notes = model.Notes?.Trim(),
                NotifyAdmin = model.NotifyAdmin
            };

            await _plantProblemService.UpdateAsync(record, ownerId, cancellationToken);

            return Json(new { success = true, message = "Οι αλλαγές αποθηκεύτηκαν." });
        }
        catch (DomainException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);

        try
        {
            await _plantProblemService.DeleteAsync(id, ownerId, cancellationToken);
            return Json(new { success = true, message = "Το πρόβλημα διαγράφηκε." });
        }
        catch (DomainException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    #endregion

    #region Schedule CRUD

    [HttpGet("schedule/create-modal/{problemId:int}")]
    public async Task<IActionResult> GetCreateScheduleModal(int problemId, CancellationToken cancellationToken)
    {
        try
        {
            var model = await _factory.PrepareCreateScheduleModelAsync(problemId, cancellationToken);
            return PartialView("_CreateScheduleModal", model);
        }
        catch (DomainException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("schedule/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSchedule(ScheduleEditModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return Json(new { success = false, message = errors });
        }

        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);

        try
        {
            var schedule = new PlantProblemSchedule
            {
                OwnerId = ownerId,
                PlantProblemRecordId = model.PlantProblemRecordId,
                ActionName = model.ActionName.Trim(),
                FrequencyValue = model.FrequencyValue,
                FrequencyUnit = Enum.Parse<Core.Domain.Garden.Enums.ScheduleFrequencyUnit>(model.FrequencyUnit),
                DosageNotes = model.DosageNotes?.Trim(),
                StartDate = model.StartDate,
                ScheduleStatus = Enum.Parse<Core.Domain.Garden.Enums.ScheduleStatus>(model.ScheduleStatus)
            };

            await _plantProblemService.AddScheduleAsync(schedule, cancellationToken);

            return Json(new { success = true, message = "Το πρόγραμμα θεραπείας αποθηκεύτηκε." });
        }
        catch (DomainException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("schedule/edit-modal/{id:int}")]
    public async Task<IActionResult> GetEditScheduleModal(int id, CancellationToken cancellationToken)
    {
        try
        {
            var model = await _factory.PrepareEditScheduleModelAsync(id, cancellationToken);
            return PartialView("_EditScheduleModal", model);
        }
        catch (DomainException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("schedule/edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditSchedule(int id, ScheduleEditModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return Json(new { success = false, message = errors });
        }

        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);

        try
        {
            var schedule = new PlantProblemSchedule
            {
                Id = id,
                OwnerId = ownerId,
                PlantProblemRecordId = model.PlantProblemRecordId,
                ActionName = model.ActionName.Trim(),
                FrequencyValue = model.FrequencyValue,
                FrequencyUnit = Enum.Parse<Core.Domain.Garden.Enums.ScheduleFrequencyUnit>(model.FrequencyUnit),
                DosageNotes = model.DosageNotes?.Trim(),
                StartDate = model.StartDate,
                ScheduleStatus = Enum.Parse<Core.Domain.Garden.Enums.ScheduleStatus>(model.ScheduleStatus)
            };

            await _plantProblemService.UpdateScheduleAsync(schedule, ownerId, cancellationToken);

            return Json(new { success = true, message = "Το πρόγραμμα ενημερώθηκε." });
        }
        catch (DomainException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("schedule/delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSchedule(int id, CancellationToken cancellationToken)
    {
        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);

        try
        {
            await _plantProblemService.DeleteScheduleAsync(id, ownerId, cancellationToken);
            return Json(new { success = true, message = "Το πρόγραμμα διαγράφηκε." });
        }
        catch (DomainException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    #endregion
}