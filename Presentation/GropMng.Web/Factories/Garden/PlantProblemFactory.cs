using GropMng.Core.Domain.Garden.Enums;
using GropMng.Core.Domain.Garden.Health;
using GropMng.Core.Interfaces.Services.Garden.Health;
using GropMng.Core.Interfaces.Services.Garden.Plants;
using GropMng.Core.Interfaces.Services.User;
using GropMng.Web.Models.Garden;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GropMng.Web.Factories.Garden;

/// <summary>
/// Prepares ViewModels for the Problem Record / Schedule user-facing UI.
/// </summary>
public class PlantProblemFactory
{
    #region Fields

    private readonly IPlantProblemService _plantProblemService;
    private readonly IPlantInstanceService _plantInstanceService;
    private readonly ICurrentOwnerProvider _currentOwnerProvider;

    #endregion

    #region Ctor

    public PlantProblemFactory(
        IPlantProblemService plantProblemService,
        IPlantInstanceService plantInstanceService,
        ICurrentOwnerProvider currentOwnerProvider)
    {
        _plantProblemService = plantProblemService ?? throw new ArgumentNullException(nameof(plantProblemService));
        _plantInstanceService = plantInstanceService ?? throw new ArgumentNullException(nameof(plantInstanceService));
        _currentOwnerProvider = currentOwnerProvider ?? throw new ArgumentNullException(nameof(currentOwnerProvider));
    }

    #endregion

    #region Public

    /// <summary>
    /// Prepares the ViewModel for the Problems tab, including all problem cards with their schedules.
    /// </summary>
    public async Task<ProblemsTabViewModel> PrepareProblemsTabViewModelAsync(
        int plantInstanceId,
        CancellationToken cancellationToken = default)
    {
        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);

        var instance = await _plantInstanceService.GetPlantInstanceByIdAsync(plantInstanceId, ownerId, includeDetails: false, cancellationToken)
            ?? throw new GropMng.Core.Common.Exceptions.DomainException($"PlantInstance with id '{plantInstanceId}' was not found.");

        var records = await _plantProblemService.GetByPlantInstanceAsync(plantInstanceId, ownerId, cancellationToken);

        var cards = new List<ProblemRecordCardModel>(records.Count);
        foreach (var record in records)
        {
            // Load schedules for this record
            var allSchedules = await _plantProblemService.GetUpcomingSchedulesAsync(ownerId, cancellationToken);
            var recordSchedules = allSchedules
                .Where(s => s.PlantProblemRecordId == record.Id && !s.IsDeleted)
                .OrderBy(s => s.NextDueDate)
                .ToList();

            var scheduleLines = recordSchedules.Select(s => new ScheduleLineModel
            {
                Id = s.Id,
                ActionName = s.ActionName,
                FrequencyValue = s.FrequencyValue,
                FrequencyUnitDisplay = GetFrequencyUnitDisplay(s.FrequencyUnit),
                NextDueDate = s.NextDueDate,
                ScheduleStatusDisplay = GetScheduleStatusDisplay(s.ScheduleStatus),
                ScheduleStatusBadgeClass = GetScheduleStatusBadgeClass(s.ScheduleStatus)
            }).ToList();

            cards.Add(new ProblemRecordCardModel
            {
                Id = record.Id,
                ProblemName = record.ProblemName,
                DiseaseKnowledgeId = record.DiseaseKnowledgeId,
                DiseaseKnowledgeCommonName = null, // Loaded separately if needed
                DetectedDate = record.DetectedDate,
                SeverityDisplay = GetSeverityDisplay(record.Severity),
                SeverityBadgeClass = GetSeverityBadgeClass(record.Severity),
                ProblemStatusDisplay = GetProblemStatusDisplay(record.ProblemStatus),
                ProblemStatusBadgeClass = GetProblemStatusBadgeClass(record.ProblemStatus),
                InfoSourceDisplay = GetInfoSourceDisplay(record.InfoSource),
                Notes = record.Notes,
                ResolvedDate = record.ResolvedDate,
                Schedules = scheduleLines
            });
        }

        return new ProblemsTabViewModel
        {
            PlantInstanceId = plantInstanceId,
            PlantInstanceName = instance.Nickname ?? $"Plant #{plantInstanceId}",
            Records = cards
        };
    }

    /// <summary>
    /// Prepares the create problem modal ViewModel with default values.
    /// </summary>
    public async Task<ProblemRecordEditModel> PrepareCreateModelAsync(
        int plantInstanceId,
        CancellationToken cancellationToken = default)
    {
        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);

        var instance = await _plantInstanceService.GetPlantInstanceByIdAsync(plantInstanceId, ownerId, includeDetails: false, cancellationToken)
            ?? throw new GropMng.Core.Common.Exceptions.DomainException($"PlantInstance with id '{plantInstanceId}' was not found.");

        return new ProblemRecordEditModel
        {
            PlantInstanceId = plantInstanceId,
            PlantInstanceName = instance.Nickname ?? $"Plant #{plantInstanceId}",
            SeverityOptions = GetSeveritySelectList(),
            ProblemStatusOptions = GetProblemStatusSelectList(),
            InfoSourceOptions = GetInfoSourceSelectList()
        };
    }

    /// <summary>
    /// Prepares the edit problem modal ViewModel pre-filled from an existing record.
    /// </summary>
    public async Task<ProblemRecordEditModel> PrepareEditModelAsync(
        int problemId,
        CancellationToken cancellationToken = default)
    {
        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);

        var record = await _plantProblemService.GetByIdAsync(problemId, ownerId, cancellationToken);

        return new ProblemRecordEditModel
        {
            Id = record.Id,
            PlantInstanceId = record.PlantInstanceId,
            PlantInstanceName = string.Empty, // Will be resolved by controller
            ProblemName = record.ProblemName,
            DiseaseKnowledgeId = record.DiseaseKnowledgeId,
            DetectedDate = record.DetectedDate,
            Severity = record.Severity.ToString(),
            ProblemStatus = record.ProblemStatus.ToString(),
            InfoSource = record.InfoSource.ToString(),
            Notes = record.Notes,
            NotifyAdmin = record.NotifyAdmin,
            SeverityOptions = GetSeveritySelectList(record.Severity.ToString()),
            ProblemStatusOptions = GetProblemStatusSelectList(record.ProblemStatus.ToString()),
            InfoSourceOptions = GetInfoSourceSelectList(record.InfoSource.ToString())
        };
    }

    /// <summary>
    /// Prepares the create schedule modal ViewModel with default values.
    /// </summary>
    public async Task<ScheduleEditModel> PrepareCreateScheduleModelAsync(
        int problemId,
        CancellationToken cancellationToken = default)
    {
        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);

        // Verify ownership via parent record
        await _plantProblemService.GetByIdAsync(problemId, ownerId, cancellationToken);

        return new ScheduleEditModel
        {
            PlantProblemRecordId = problemId,
            FrequencyUnitOptions = GetFrequencyUnitSelectList(),
            ScheduleStatusOptions = GetScheduleStatusSelectList()
        };
    }

    /// <summary>
    /// Prepares the edit schedule modal ViewModel pre-filled from an existing schedule.
    /// </summary>
    public async Task<ScheduleEditModel> PrepareEditScheduleModelAsync(
        int scheduleId,
        CancellationToken cancellationToken = default)
    {
        var ownerId = await _currentOwnerProvider.GetCurrentOwnerIdAsync(cancellationToken);

        var schedules = await _plantProblemService.GetUpcomingSchedulesAsync(ownerId, cancellationToken);
        var schedule = schedules.FirstOrDefault(s => s.Id == scheduleId);

        if (schedule is null)
        {
            // Try to find it even if not "upcoming" — load by parent records
            throw new GropMng.Core.Common.Exceptions.DomainException($"Schedule with id '{scheduleId}' was not found.");
        }

        return new ScheduleEditModel
        {
            Id = schedule.Id,
            PlantProblemRecordId = schedule.PlantProblemRecordId,
            ActionName = schedule.ActionName,
            FrequencyValue = schedule.FrequencyValue,
            FrequencyUnit = schedule.FrequencyUnit.ToString(),
            DosageNotes = schedule.DosageNotes,
            StartDate = schedule.StartDate,
            NextDueDate = schedule.NextDueDate,
            ScheduleStatus = schedule.ScheduleStatus.ToString(),
            FrequencyUnitOptions = GetFrequencyUnitSelectList(schedule.FrequencyUnit.ToString()),
            ScheduleStatusOptions = GetScheduleStatusSelectList(schedule.ScheduleStatus.ToString())
        };
    }

    #endregion

    #region Private — Enum Select Lists

    private static List<SelectListItem> GetSeveritySelectList(string? selected = null)
    {
        return Enum.GetValues<Severity>().Select(e => new SelectListItem
        {
            Value = e.ToString(),
            Text = GetSeverityDisplay(e),
            Selected = e.ToString() == selected
        }).ToList();
    }

    private static List<SelectListItem> GetProblemStatusSelectList(string? selected = null)
    {
        return Enum.GetValues<ProblemStatus>().Select(e => new SelectListItem
        {
            Value = e.ToString(),
            Text = GetProblemStatusDisplay(e),
            Selected = e.ToString() == selected
        }).ToList();
    }

    private static List<SelectListItem> GetInfoSourceSelectList(string? selected = null)
    {
        return Enum.GetValues<InfoSource>().Select(e => new SelectListItem
        {
            Value = e.ToString(),
            Text = GetInfoSourceDisplay(e),
            Selected = e.ToString() == selected
        }).ToList();
    }

    private static List<SelectListItem> GetFrequencyUnitSelectList(string? selected = null)
    {
        return Enum.GetValues<ScheduleFrequencyUnit>().Select(e => new SelectListItem
        {
            Value = e.ToString(),
            Text = GetFrequencyUnitDisplay(e),
            Selected = e.ToString() == selected
        }).ToList();
    }

    private static List<SelectListItem> GetScheduleStatusSelectList(string? selected = null)
    {
        return Enum.GetValues<ScheduleStatus>().Select(e => new SelectListItem
        {
            Value = e.ToString(),
            Text = GetScheduleStatusDisplay(e),
            Selected = e.ToString() == selected
        }).ToList();
    }

    #endregion

    #region Private — Enum Display Helpers

    private static string GetSeverityDisplay(Severity severity) => severity switch
    {
        Severity.Low => "Χαμηλή",
        Severity.Medium => "Μέτρια",
        Severity.High => "Υψηλή",
        _ => severity.ToString()
    };

    private static string GetProblemStatusDisplay(ProblemStatus status) => status switch
    {
        ProblemStatus.Active => "Ενεργό",
        ProblemStatus.Monitoring => "Παρακολούθηση",
        ProblemStatus.Resolved => "Επιλύθηκε",
        _ => status.ToString()
    };

    private static string GetInfoSourceDisplay(InfoSource source) => source switch
    {
        InfoSource.OwnKnowledge => "Δική μου γνώση",
        InfoSource.Agronomist => "Γεωπόνος",
        InfoSource.AITool => "Εργαλείο ΑΙ",
        InfoSource.Internet => "Διαδίκτυο",
        InfoSource.Other => "Άλλο",
        _ => source.ToString()
    };

    private static string GetFrequencyUnitDisplay(ScheduleFrequencyUnit unit) => unit switch
    {
        ScheduleFrequencyUnit.Days => "Ημέρες",
        ScheduleFrequencyUnit.Weeks => "Εβδομάδες",
        ScheduleFrequencyUnit.Months => "Μήνες",
        _ => unit.ToString()
    };

    private static string GetScheduleStatusDisplay(ScheduleStatus status) => status switch
    {
        ScheduleStatus.Active => "Ενεργό",
        ScheduleStatus.Completed => "Ολοκληρώθηκε",
        ScheduleStatus.Cancelled => "Ακυρώθηκε",
        _ => status.ToString()
    };

    private static string GetSeverityBadgeClass(Severity severity) => severity switch
    {
        Severity.Low => "badge bg-label-info",
        Severity.Medium => "badge bg-label-warning",
        Severity.High => "badge bg-label-danger",
        _ => "badge bg-label-secondary"
    };

    private static string GetProblemStatusBadgeClass(ProblemStatus status) => status switch
    {
        ProblemStatus.Active => "badge bg-label-danger",
        ProblemStatus.Monitoring => "badge bg-label-warning",
        ProblemStatus.Resolved => "badge bg-label-success",
        _ => "badge bg-label-secondary"
    };

    private static string GetScheduleStatusBadgeClass(ScheduleStatus status) => status switch
    {
        ScheduleStatus.Active => "badge bg-label-primary",
        ScheduleStatus.Completed => "badge bg-label-success",
        ScheduleStatus.Cancelled => "badge bg-label-secondary",
        _ => "badge bg-label-secondary"
    };

    #endregion
}